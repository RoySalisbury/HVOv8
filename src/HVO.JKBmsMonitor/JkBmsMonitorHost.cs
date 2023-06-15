using Microsoft.Extensions.DependencyInjection;
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
        private JkBmsMonitorClient _jkBmsMonitorClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly JkBmsMonitorHostOptions _jkBmsMonitorHostOptions;

        public JkBmsMonitorHost(ILogger<JkBmsMonitorHost> logger, IServiceProvider serviceProvider, IOptions<JkBmsMonitorHostOptions> jkBmsMonitorHostOptions)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _jkBmsMonitorHostOptions = jkBmsMonitorHostOptions.Value; // hci0 and deviceAddress should come from here
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
                    using (this._jkBmsMonitorClient = this._serviceProvider.GetRequiredService<JkBmsMonitorClient>())
                    {
                        try
                        {
                            Console.WriteLine($"Initializing {nameof(JkBmsMonitorClient)} instance...");
                            await this._jkBmsMonitorClient.InitializeAdaptorAsync("hci0", false, stoppingToken);

                            await this._jkBmsMonitorClient.ConnectToDeviceAsync("C8:47:8C:E4:54:B1", true, 20);
                            //await this._jkBmsMonitorClient.RequestDeviceInfo();
                            await this._jkBmsMonitorClient.RequestCellInfo01();

                            Console.WriteLine($"Press Ctrl-C to stop instance...");
                            await Task.Delay(-1, stoppingToken);
                        }
                        catch (TaskCanceledException)
                        {
                            this._logger.LogDebug($"{nameof(JkBmsMonitorHost)} TaskCanceledException.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            this._logger.LogError($"{nameof(JkBmsMonitorHost)} initialization error: {ex.Message}. Restarting in {this._jkBmsMonitorHostOptions.RestartOnFailureWaitTime} seconds unless cancelled.");
                            //this._logger.LogError($"{nameof(JkBmsMonitorHost)} initialization error: {ex.StackTrace}");
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
