using HVO.ObservatoryAgent.DavisVantageProAgent.NotificationServices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.HostedService
{
    public sealed class DavisVantageProAgentServiceHost : BackgroundService
    {
        private readonly ILogger<DavisVantageProAgentServiceHost> _logger;
        private readonly DavisVantageProAgentServiceOptions _options;
        private readonly RabbitMQConfigurationOptions _rabbitMqOptions;
        private readonly RabbitMQ.Client.IConnectionFactory _rabbitMqConnectionFactory;
        private readonly WeatherUpdateNotificationService _weatherUpdateNotificationService;

        public DavisVantageProAgentServiceHost(ILogger<DavisVantageProAgentServiceHost> logger, IOptions<DavisVantageProAgentServiceOptions> options, IOptions<RabbitMQConfigurationOptions> rabbitMqOptions, IWeatherUpdateNotificationService weatherUpdateNotificationService)
        {
            this._logger = logger;
            this._options = options.Value;
            this._rabbitMqOptions = rabbitMqOptions.Value;

            this._weatherUpdateNotificationService = (WeatherUpdateNotificationService)weatherUpdateNotificationService;

            this._rabbitMqConnectionFactory = new RabbitMQ.Client.ConnectionFactory()
            {
                UserName = this._rabbitMqOptions.UserName,
                Password = this._rabbitMqOptions.Password,
                HostName = this._rabbitMqOptions.HostName,
                Port = this._rabbitMqOptions.Port,
                AutomaticRecoveryEnabled = this._rabbitMqOptions.AutomaticRecoveryEnabled
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._logger.LogTrace($"{nameof(DavisVantageProAgentServiceHost)} background task is starting.");
            try
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    try
                    {
                        await IncommingMessageLogic(stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        this._logger.LogInformation($"{nameof(DavisVantageProAgentServiceHost)} background task cancelled.");
                        break;
                    }
                    catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException ex)
                    {
                        this._logger.LogError("RabbitMq: {message}. Retry in {waitTime} seconds.", ex.Message, this._options.RestartOnFailureWaitTime);
                        await Task.Delay(TimeSpan.FromSeconds(this._options.RestartOnFailureWaitTime), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "{serviceName} background task stopped unexpectedly. Restarting in {waitTime} seconds.", nameof(DavisVantageProAgentServiceHost), this._options.RestartOnFailureWaitTime);
                        await Task.Delay(TimeSpan.FromSeconds(this._options.RestartOnFailureWaitTime), stoppingToken);
                    }
                }
            }
            finally
            {
                this._logger.LogTrace($"{nameof(DavisVantageProAgentServiceHost)} background task has stopped.");
            }
        }

        private async Task IncommingMessageLogic(CancellationToken cancellationToken)
        {
            if (IPAddress.TryParse(this._options.RemoteConsoleAddress, out var remoteConsoleAddress) == false)
            {
                this._logger.LogError("RemoteConsoleAddress is not a valid IPAddress. {address}", this._options.RemoteConsoleAddress);
                return;
            }

            using (var connection = this._rabbitMqConnectionFactory.CreateConnection(nameof(DavisVantageProAgentServiceHost)))
            {
                try
                {
                    using (var model = connection.CreateModel())
                    {
                        // Make sure the queue exists
                        model.QueueDeclare(this._options.StorageQueueName, true, false, false, null);

                        // Start reading the weather records from the station
                        using (var weatherStation = new Weather.DavisVantagePro.DavisVantageProWeatherLinkIP(remoteConsoleAddress, this._options.RemoteConsolePort))
                        {
                            weatherStation.OnConsoleRecordReceived += (s, e) =>
                            {
                                if (cancellationToken.IsCancellationRequested == false)
                                {
                                    // This is the message that goes into the queue.
                                    var properties = model.CreateBasicProperties();
                                    properties.ContentType = "application/json";

                                    properties.DeliveryMode = 2; // PERSISTENT
                                    
                                    // The shovel does not seem to transfer these over, so we need to reconfigure the packet
                                    properties.Headers = new Dictionary<string, object>()
                                    {
                                        { "RecordDateTime", e.RecordDateTime.ToString("O") }
                                    };


                                    // Just send the entire event class.  It has both the values we need
                                    var json = System.Text.Json.JsonSerializer.Serialize(e);

                                    // Serialize the data to Json
                                    model.BasicPublish("", this._options.StorageQueueName, true, properties, System.Text.ASCIIEncoding.UTF8.GetBytes(json));
                                    this._logger.LogTrace($"Weather Record Published: {e.RecordDateTime}");

                                    try
                                    {
                                        var consoleRecord = Weather.DavisVantagePro.DavisVantageProConsoleRecord.Create(e.ConsoleRecord, e.RecordDateTime);
                                        this._weatherUpdateNotificationService.Update(consoleRecord);

                                        //Console.WriteLine("DT: {0}\tOT: {1}\tWS: {2}\tBV: {3}", consoleRecord.RecordDateTime.ToString("O"), consoleRecord.OutsideTemperature?.Fahrenheit, consoleRecord.WindSpeed, consoleRecord.ConsoleBatteryVoltage);
                                    }
                                    catch
                                    {
                                    }
                                }
                            };

                            await weatherStation.StartMonitorAsync(cancellationToken);
                        }
                    }
                }
                finally
                {
                    try
                    {
                        connection?.Close();
                    }
                    catch { }
                }
            }
        }
    }
}
