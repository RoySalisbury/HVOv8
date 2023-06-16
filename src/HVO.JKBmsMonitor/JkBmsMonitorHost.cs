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
            // Whne we first start receiving packet data, we should wait for the response type of 3 before decoding any other types.  This allows us to get the
            // hardware/firmware versions of the device because we will need these to properly decode the type 2 packet data.

            switch (e.Packet[4])
            {
                case 0x01:
                    {
                        this._jkBmsMonitorClient.LatestDeviceSettings = new JkBmsGetDeviceSettingsResponse(e.Packet);

                        var info = this._jkBmsMonitorClient.LatestDeviceSettings;
                        Console.WriteLine($"Device Settings - Cell Count: {info.CellCount}, Nominal Capacity: {info.NominalBatteryCapacity} mA");

                        break;
                    }
                case 0x02:
                    {
                        //Console.WriteLine("JK02 - Response Type 2");
                        if (this._jkBmsMonitorClient.LatestDeviceInfo is null)
                        {
                            // Should have already received this ... ask again.
                            await this._jkBmsMonitorClient.RequestDeviceInfo();
                            break;
                        }

                        var hardwareVersion = this._jkBmsMonitorClient.LatestDeviceInfo.HardwareVersion;
                        //var softwareVersion = this._jkBmsMonitorClient.LatestDeviceInfo.SoftwareVersion;

                        if (hardwareVersion.ToUpperInvariant().StartsWith("11."))
                        {
                            this._jkBmsMonitorClient.LatestCellInfoInfo = new JkBmsGetCellInfo32Response(e.Packet);
                        }
                        else
                        {
                            this._jkBmsMonitorClient.LatestCellInfoInfo = new JkBmsGetCellInfo24Response(e.Packet);
                        }

                        var info = this._jkBmsMonitorClient.LatestCellInfoInfo;
                        Console.WriteLine($"Cell Info   - AVG: {info.AverageCellVoltage} mV, MIN: {info.CellVoltages.Where(x => x > 0).Min()} mV, MAX: {info.CellVoltages.Max()} mV, SOC: {info.StateOfCharge}%, Power: {info.BatteryPower} mW, Total Runtime: {info.TotalRuntime}");

                        break;
                    }
                case 0x03:
                    {
                        //Console.WriteLine("JK02 - Response Type 3");
                        this._jkBmsMonitorClient.LatestDeviceInfo = new JkBmsGetDeviceInfoResponse(e.Packet);

                        var info = this._jkBmsMonitorClient.LatestDeviceInfo;
                        Console.WriteLine($"Device Info - Name: {info.DeviceName}, Uptime: {info.Uptime}, HV Version: {info.HardwareVersion}, SW Version: {info.SoftwareVersion}");

                        // Once we have the Type 3 packet, we can start requesting the type 1 and type 2 packets
                        await this._jkBmsMonitorClient.RequestDeviceSettings();

                        // This seems to get lost sometimes
                        await Task.Delay(500);
                        await this._jkBmsMonitorClient.RequestDeviceSettings();

                        break;
                    }
                default:
                    Console.WriteLine("JK02 - Response Type ?");
                    break;
            }

            //Console.WriteLine($"PacketReceived: {BitConverter.ToString(e.Packet)}");
        }
    }
}
