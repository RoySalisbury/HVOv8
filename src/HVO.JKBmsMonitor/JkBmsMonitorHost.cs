using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        private DateTime _lastDeviceConfigPublish = DateTime.MinValue;
        private DateTime _lastDeviceStatePublish = DateTime.MinValue;


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

                        if (hardwareVersion.ToUpperInvariant().StartsWith("11."))
                        {
                            this._jkBmsMonitorClient.LatestCellInfoInfo = new JkBmsGetCellInfo32Response(e.Packet);
                        }
                        else
                        {
                            this._jkBmsMonitorClient.LatestCellInfoInfo = new JkBmsGetCellInfo24Response(e.Packet);
                        }

                        var info = this._jkBmsMonitorClient.LatestCellInfoInfo;
                        Console.WriteLine($"Cell Info   - AVG: {info.AverageCellVoltage} mV, MIN: {info.CellVoltage.Where(x => x > 0).Min()} mV, MAX: {info.CellVoltage.Max()} mV, SOC: {info.StateOfCharge}%, Power: {info.BatteryPower} mW, Total Runtime: {info.TotalRuntime}");

                        if (DateTime.Now.Subtract(this._lastDeviceStatePublish).TotalSeconds > 2)
                        {
                            var deviceId = "jkbms_280_01";
                            var softwareVersion = this._jkBmsMonitorClient.LatestDeviceInfo.SoftwareVersion;
                            var deviceSerialNumber = this._jkBmsMonitorClient.LatestDeviceInfo.SerialNumber;
                            var deviceModel = this._jkBmsMonitorClient.LatestDeviceInfo.VendorId;
                            var deviceName = this._jkBmsMonitorClient.LatestDeviceInfo.DeviceName;

                            var balanceCurrent = JkMqtt.GenerateSensorData<float>(deviceId, "Balance Current", "balance_current",          "power", "measurement", "A",  "mdi:current-dc", info.BalanceCurrent * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var balanceEnabled = JkMqtt.GenerateSensorData<float>(deviceId, "Balance Enabled", "balance_enabled",          "power", "measurement", "",   "mdi:current-dc", info.BalanceAction, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var capacityNominal = JkMqtt.GenerateSensorData<float>(deviceId, "Nominal Capacity", "capacity_nominal",       "power", "measurement", "Ah", "mdi:current-dc", info.CycleCapacity * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var capacityRemaining = JkMqtt.GenerateSensorData<float>(deviceId, "Remaining Capacity", "capacity_remaining", "power", "measurement", "Ah", "mdi:current-dc", info.CapacityRemaining * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);

                            var cellResistance = new List<(string ConfigTopic, dynamic ConfigData, string StateTopic, float Value)>();
                            for (int i = 0; i < info.CellResistance.Length; i++)
                            {
                                var cr = JkMqtt.GenerateSensorData<float>(deviceId, $"Cell Resistance {i+1:00}", $"cell_resistance_{i+1:00}", "power", "measurement", "Ohm", "mdi:current-dc", info.CellResistance[i] * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                                cellResistance.Add(cr);
                            }

                            var cellVoltage = new List<(string ConfigTopic, dynamic ConfigData, string StateTopic, float Value)>();
                            for (int i = 0; i < info.CellVoltage.Length; i++)
                            {
                                var cv = JkMqtt.GenerateSensorData<float>(deviceId, $"Cell Voltage {i + 1:00}", $"cell_voltage_{i + 1:00}", "voltage", "measurement", "V", "mdi:flash-triangle", info.CellVoltage[i] * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                                cellVoltage.Add(cv);
                            }

                            var cvMax = info.CellVoltage.Max();
                            var cvMaxIndex = Array.IndexOf(info.CellVoltage, cvMax);
                            var cvMin = info.CellVoltage.Where(x => x > 0).Min();
                            var cvMinIndex = Array.IndexOf(info.CellVoltage, cvMin);

                            var cellVoltgeAverage = JkMqtt.GenerateSensorData<float>(deviceId, "Cell Voltage Average", "cell_voltage_avg", "voltage", "measurement", "V", "mdi:flash-triangle", info.AverageCellVoltage * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cellVoltageDelta = JkMqtt.GenerateSensorData<float>(deviceId, "Cell Voltage Delta", "cell_voltage_delta", "voltage", "measurement", "V", "mdi:flash-triangle", info.DeltaCellVoltage * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, hardwareVersion, deviceSerialNumber);
                            var cellVoltgeMax = JkMqtt.GenerateSensorData<float>(deviceId, "Cell Voltage Max", "cell_voltage_max", "voltage", "measurement", "V", "mdi:flash-triangle", cvMax * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cellVoltageMaxIndex = JkMqtt.GenerateSensorData<int>(deviceId, "Cell Voltage Max Index", "cell_voltage_max_index", "", "measurement", "", "", cvMaxIndex + 1, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cellVoltageMin = JkMqtt.GenerateSensorData<float>(deviceId, "Cell Voltage Min", "cell_voltage_min", "voltage", "measurement", "V", "mdi:flash-triangle", cvMin * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cellVoltageMinIndex = JkMqtt.GenerateSensorData<int>(deviceId, "Cell Voltage Min Index", "cell_voltage_min_index", "", "measurement", "", "", cvMinIndex + 1, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);

                            var chargeCycleCount = JkMqtt.GenerateSensorData<uint>(deviceId, "Charge Cycle Count", "charge_cycle_count", "", "measurement", "", "", info.CycleCount, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var chargingEnabled = JkMqtt.GenerateSensorData<int>(deviceId, "Charging Enabled", "charging_enabled", "", "measurement", "", "", info.CharginMosfetEnabled ? 1: 0, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var current = JkMqtt.GenerateSensorData<float>(deviceId, "Current", "current", "power", "measurement", "A", "mdi:current-dc", info.ChargeCurrent * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cycleCapacity = JkMqtt.GenerateSensorData<float>(deviceId, "Cycle Capacity", "cycle_capacity", "power", "measurement", "Ah", "", info.CycleCapacity * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var dischargingEnabled = JkMqtt.GenerateSensorData<int>(deviceId, "Discharging Enabled", "discharging_enabled", "", "measurement", "", "", info.DisCharginMosfetEnabled ? 1 : 0, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);

                            var power = JkMqtt.GenerateSensorData<float>(deviceId, "Power", "power", "power", "measurement", "W", "", info.BatteryPower * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var stateOfCharge = JkMqtt.GenerateSensorData<byte>(deviceId, "State Of Charge", "state_of_charge", "", "measurement", "", "", info.StateOfCharge, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var temperature1 = JkMqtt.GenerateSensorData<float>(deviceId, "Temperature 1", "temperature_1", "", "measurement", "C", "", info.TemperatureProbe01 * 0.1f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var temperature2 = JkMqtt.GenerateSensorData<float>(deviceId, "Temperature 2", "temperature_2", "", "measurement", "C", "", info.TemperatureProbe02 * 0.1f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var temperatureMosfet = JkMqtt.GenerateSensorData<float>(deviceId, "Temperature Mosfet", "temperature_mosfet", "", "measurement", "C", "", info.PowerTubeTemperature * 0.1f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var totalRuntime = JkMqtt.GenerateSensorData<string>(deviceId, "Total Runtime", "total_runtime", "", "measurement", "", "", info.TotalRuntime.ToString(), deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var totalVoltage = JkMqtt.GenerateSensorData<float>(deviceId, "Total Voltage", "total_voltage", "voltage", "measurement", "V", "", info.BatteryVoltage * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);


                            // Publish the device configuration data every 60 seconds
                            if (DateTime.Now.Subtract(this._lastDeviceConfigPublish).TotalSeconds > 60)
                            {
                                JkMqtt.Publish(balanceCurrent.ConfigTopic, JsonSerializer.Serialize<dynamic>(balanceCurrent.ConfigData));
                                JkMqtt.Publish(balanceEnabled.ConfigTopic, JsonSerializer.Serialize<dynamic>(balanceEnabled.ConfigData));
                                JkMqtt.Publish(capacityNominal.ConfigTopic, JsonSerializer.Serialize<dynamic>(capacityNominal.ConfigData));
                                JkMqtt.Publish(capacityRemaining.ConfigTopic, JsonSerializer.Serialize<dynamic>(capacityRemaining.ConfigData));

                                foreach (var item in cellResistance)
                                {
                                    JkMqtt.Publish(item.ConfigTopic, JsonSerializer.Serialize<dynamic>(item.ConfigData));
                                }

                                foreach (var item in cellVoltage)
                                {
                                    JkMqtt.Publish(item.ConfigTopic, JsonSerializer.Serialize<dynamic>(item.ConfigData));
                                }

                                JkMqtt.Publish(cellVoltgeAverage.ConfigTopic, JsonSerializer.Serialize<dynamic>(cellVoltgeAverage.ConfigData));
                                JkMqtt.Publish(cellVoltageDelta.ConfigTopic, JsonSerializer.Serialize<dynamic>(cellVoltageDelta.ConfigData));
                                JkMqtt.Publish(cellVoltgeMax.ConfigTopic, JsonSerializer.Serialize<dynamic>(cellVoltgeMax.ConfigData));
                                JkMqtt.Publish(cellVoltageMaxIndex.ConfigTopic, JsonSerializer.Serialize<dynamic>(cellVoltageMaxIndex.ConfigData));
                                JkMqtt.Publish(cellVoltageMin.ConfigTopic, JsonSerializer.Serialize<dynamic>(cellVoltageMin.ConfigData));
                                JkMqtt.Publish(cellVoltageMinIndex.ConfigTopic, JsonSerializer.Serialize<dynamic>(cellVoltageMinIndex.ConfigData));
                                JkMqtt.Publish(chargeCycleCount.ConfigTopic, JsonSerializer.Serialize<dynamic>(chargeCycleCount.ConfigData));
                                JkMqtt.Publish(chargingEnabled.ConfigTopic, JsonSerializer.Serialize<dynamic>(chargingEnabled.ConfigData));
                                JkMqtt.Publish(current.ConfigTopic, JsonSerializer.Serialize<dynamic>(current.ConfigData));
                                JkMqtt.Publish(cycleCapacity.ConfigTopic, JsonSerializer.Serialize<dynamic>(cycleCapacity.ConfigData));
                                JkMqtt.Publish(dischargingEnabled.ConfigTopic, JsonSerializer.Serialize<dynamic>(dischargingEnabled.ConfigData));
                                JkMqtt.Publish(power.ConfigTopic, JsonSerializer.Serialize<dynamic>(power.ConfigData));
                                JkMqtt.Publish(temperature1.ConfigTopic, JsonSerializer.Serialize<dynamic>(temperature1.ConfigData));
                                JkMqtt.Publish(temperature2.ConfigTopic, JsonSerializer.Serialize<dynamic>(temperature2.ConfigData));
                                JkMqtt.Publish(temperatureMosfet.ConfigTopic, JsonSerializer.Serialize<dynamic>(temperatureMosfet.ConfigData));
                                JkMqtt.Publish(totalRuntime.ConfigTopic, JsonSerializer.Serialize<dynamic>(totalRuntime.ConfigData));
                                JkMqtt.Publish(totalVoltage.ConfigTopic, JsonSerializer.Serialize<dynamic>(totalVoltage.ConfigData));

                                this._lastDeviceConfigPublish = DateTime.Now;
                            }

                            // Publish the state data
                            JkMqtt.Publish(balanceCurrent.StateTopic, balanceCurrent.Value);
                            JkMqtt.Publish(balanceEnabled.StateTopic, balanceEnabled.Value);
                            JkMqtt.Publish(capacityNominal.StateTopic, capacityNominal.Value);
                            JkMqtt.Publish(capacityRemaining.StateTopic, capacityRemaining.Value);

                            foreach (var item in cellResistance)
                            {
                                JkMqtt.Publish(item.StateTopic, item.Value);
                            }

                            foreach (var item in cellVoltage)
                            {
                                JkMqtt.Publish(item.StateTopic, item.Value);
                            }

                            JkMqtt.Publish(cellVoltgeAverage.StateTopic, cellVoltgeAverage.Value);
                            JkMqtt.Publish(cellVoltageDelta.StateTopic, cellVoltageDelta.Value);
                            JkMqtt.Publish(cellVoltgeMax.StateTopic, cellVoltgeMax.Value);
                            JkMqtt.Publish(cellVoltageMaxIndex.StateTopic, cellVoltageMaxIndex.Value);
                            JkMqtt.Publish(cellVoltageMin.StateTopic, cellVoltageMin.Value);
                            JkMqtt.Publish(cellVoltageMinIndex.StateTopic, cellVoltageMinIndex.Value);
                            JkMqtt.Publish(chargeCycleCount.StateTopic, chargeCycleCount.Value);
                            JkMqtt.Publish(chargingEnabled.StateTopic, chargingEnabled.Value);
                            JkMqtt.Publish(current.StateTopic, current.Value);
                            JkMqtt.Publish(cycleCapacity.StateTopic, cycleCapacity.Value);
                            JkMqtt.Publish(dischargingEnabled.StateTopic, dischargingEnabled.Value);
                            JkMqtt.Publish(power.StateTopic, power.Value);
                            JkMqtt.Publish(temperature1.StateTopic, temperature1.Value);
                            JkMqtt.Publish(temperature2.StateTopic, temperature2.Value);
                            JkMqtt.Publish(temperatureMosfet.StateTopic, temperatureMosfet.Value);
                            JkMqtt.Publish(totalRuntime.StateTopic, totalRuntime.Value);
                            JkMqtt.Publish(totalVoltage.StateTopic, totalVoltage.Value);

                            this._lastDeviceStatePublish = DateTime.Now;
                        }
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
