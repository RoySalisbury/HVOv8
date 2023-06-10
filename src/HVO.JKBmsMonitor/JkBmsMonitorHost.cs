using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.JKBmsMonitor
{
    public class JkBmsMonitorHost : BackgroundService
    {
        private readonly ILogger<JkBmsMonitorHost> _logger;
        private readonly JkBmsMonitorClient _jkBmsMonitorClient;
        private readonly JkBmsMonitorHostOptions _jkBmsMonitorHostOptions;

        public JkBmsMonitorHost(ILogger<JkBmsMonitorHost> logger, JkBmsMonitorClient jkBmsMonitorClient, IOptions<JkBmsMonitorHostOptions> jkBmsMonitorHostOptions)
        {
            _logger = logger;
            _jkBmsMonitorClient = jkBmsMonitorClient;
            _jkBmsMonitorHostOptions = jkBmsMonitorHostOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => this._logger.LogDebug($"{nameof(JkBmsMonitorHost)} background task is stopping."));

            this._logger.LogDebug($"{nameof(JkBmsMonitorHost)} background task is starting.");
            try
            {
                // Loop this until the service is requested to stop
                while (stoppingToken.IsCancellationRequested == false)
                {
                    if (this._jkBmsMonitorClient is JkBmsMonitorClient client)
                    {
                        try
                        {
                            Console.WriteLine($"Initializing {nameof(JkBmsMonitorClient)} instance...");
                            await client.Initialize(stoppingToken);
                            try
                            {
                                var result = await client.ScanAndConnect();
                                if (result == true)
                                {
                                    await this._jkBmsMonitorClient.Test();
                                }

                                Console.WriteLine($"Press Ctrl-C to stop instance...");
                                await Task.Delay(-1, stoppingToken);
                            }
                            finally
                            {
                                // We ALWAYS want to error on the side of caution and STOP the motors.  This will call dispose, which will in turn call shutdown.
                                ((IDisposable)client).Dispose();
                                this._logger.LogDebug($"{nameof(JkBmsMonitorClient)} instance disposed.");
                            }
                        }
                        catch (TaskCanceledException)
                        {
                            this._logger.LogDebug($"{nameof(JkBmsMonitorHost)} TaskCanceledException.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            this._logger.LogError($"{nameof(JkBmsMonitorHost)} initialization error: {ex.Message}. Restarting in {this._jkBmsMonitorHostOptions.RestartOnFailureWaitTime} seconds unless cancelled.");
                            this._logger.LogError($"{nameof(JkBmsMonitorHost)} initialization error: {ex.StackTrace}");
                            await Task.Delay(TimeSpan.FromSeconds(this._jkBmsMonitorHostOptions.RestartOnFailureWaitTime), stoppingToken);
                        }
                    }
                }
            }
            finally
            {
                this._logger.LogDebug($"{nameof(JkBmsMonitorHost)} background task has stopped.");
            }
        }
    }
}
