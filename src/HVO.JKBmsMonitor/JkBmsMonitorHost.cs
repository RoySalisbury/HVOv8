using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
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
                        this._jkBmsMonitorClient.PacketReceived += JKBmsMonitorClient_PacketReceived;

                        try
                        {
                            Console.WriteLine($"Initializing {nameof(JkBmsMonitorClient)} instance...");
                            await this._jkBmsMonitorClient.InitializeAdaptorAsync("hci0", false, stoppingToken);

                            await this._jkBmsMonitorClient.ConnectToDeviceAsync("C8:47:8C:E4:54:B1", true, 20);
                            //await this._jkBmsMonitorClient.ConnectToDeviceAsync("C8:47:8C:EC:1E:B5", true, 20);

                            // Make this call to get the hardway/firmware versions so we can decode the CellInfo packets correctly.
                            await this._jkBmsMonitorClient.RequestDeviceInfo();

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
                        finally
                        {
                            this._jkBmsMonitorClient.PacketReceived -= JKBmsMonitorClient_PacketReceived;
                        }
                    }
                }
            }
            finally
            {
                this._logger.LogDebug($"{nameof(JkBmsMonitorHost)} background task has stopped.");
            }
        }

        private async void JKBmsMonitorClient_PacketReceived(object sender, PacketReceivedEventArgs e)
        {
            //JkBmsResponse response = null;
            switch (e.Packet[4])
            {
                case 0x01:
                    // 55-AA-EB-90-01-42-58-02-00-00-28-0A-00-00-5A-0A-00-00-3D-0E-00-00-2E-0E-00-00-03-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-C4-09-00-00-A0-86-01-00-1E-00-00-00-3C-00-00-00-F0-49-02-00-2C-01-00-00-3C-00-00-00-3C-00-00-00-D0-07-00-00-BC-02-00-00-58-02-00-00-BC-02-00-00-58-02-00-00-38-FF-FF-FF-9C-FF-FF-FF-84-03-00-00-BC-02-00-00-10-00-00-00-01-00-00-00-01-00-00-00-01-00-00-00-C0-45-04-00-DC-05-00-00-48-0D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-D1
                    Console.WriteLine("JK02 - Response Type 1");
                    break;
                case 0x02:
                    // 55-AA-EB-90-02-42-00-0D-FF-0C-FF-0C-FF-0C-FF-0C-FF-0C-FF-0C-00-0D-FF-0C-00-0D-00-0D-FF-0C-00-0D-00-0D-00-0D-FF-0C-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-00-00-FF-0C-01-00-06-00-37-00-37-00-35-00-34-00-34-00-34-00-34-00-35-00-35-00-36-00-39-00-38-00-3B-00-3A-00-34-00-35-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-F1-CF-00-00-6F-07-02-00-3E-F6-FF-FF-06-01-09-01-F2-00-00-00-00-00-00-5D-CE-03-04-00-C0-45-04-00-20-00-00-00-38-CF-89-00-64-00-7A-02-4A-00-7C-01-01-01-6A-06-00-00-00-00-00-00-00-00-00-00-00-00-07-00-01-00-00-00-C1-03-00-00-0D-00-28-C5-3E-40-00-00-00-00-E2-04-00-00-00-00-00-01-00-05-00-00-77-4D-59-08-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-B4
                    Console.WriteLine("JK02 - Response Type 2");
                    //response = new JkBmsGetCellInfoResponse(2, e.Packet);
                    break;
                case 0x03:
                    Console.WriteLine("JK02 - Response Type 3");
                    await this._jkBmsMonitorClient.RequestDeviceSettings();
                    //response = new JkBmsGetDeviceInfoResponse(2, e.Packet);
                    break;
                default:
                    Console.WriteLine("JK02 - Response Type ?");
                    break;
            }


            //Console.WriteLine($"PacketReceived: {BitConverter.ToString(e.Packet)}");
        }
    }
}
