using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
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
        private MQTTnet.Extensions.ManagedClient.IManagedMqttClient _mqttClient;

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
                    var mqttFactory = new MqttFactory();
                    using (this._mqttClient = mqttFactory.CreateManagedMqttClient())
                    {
                        var mqttClientOptions = new ManagedMqttClientOptionsBuilder()
                            .WithClientOptions(options =>
                            {
                                options.WithCredentials("homeassistant", "iawuaPhoNg9ohp1top7oowangushahNgaegeehuegheiba0Pa8em2cahjae9hod1");
                                options.WithTcpServer("192.168.0.10");
                            })
                            .Build();

                        await this._mqttClient.StartAsync(mqttClientOptions);

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

                            var balanceAction = JkMqtt.GenerateSensorData<byte>(deviceId, "Balance Action", "balance_action", null, "measurement", null, null, info.BalanceAction, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var balanceCurrent = JkMqtt.GenerateSensorData<float>(deviceId, "Balance Current", "balance_current", "current", "measurement", "A",  "mdi:current-dc", info.BalanceCurrent * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var balancerEnabled = JkMqtt.GenerateSensorData<int>(deviceId, "Balance Enabled", "balancer_enabled", null, "measurement", null, null, info.BalancerStatus ? 1 : 0, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var capacityRemaining = JkMqtt.GenerateSensorData<float>(deviceId, "Remaining Capacity", "capacity_remaining", "current", "measurement", "Ah", null, info.CapacityRemaining * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);

                            var cellResistance = new List<(string ConfigTopic, dynamic ConfigData, string StateTopic, float Value)>();
                            for (int i = 0; i < info.CellResistance.Length; i++)
                            {
                                var cr = JkMqtt.GenerateSensorData<float>(deviceId, $"Cell Resistance {i+1:00}", $"cell_resistance_{i+1:00}", null, "measurement", "Ohm", null, info.CellResistance[i] * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
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
                            var cellVoltageMaxIndex = JkMqtt.GenerateSensorData<int>(deviceId, "Cell Voltage Max Index", "cell_voltage_max_index", null, "measurement", null, null, cvMaxIndex + 1, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cellVoltageMin = JkMqtt.GenerateSensorData<float>(deviceId, "Cell Voltage Min", "cell_voltage_min", "voltage", "measurement", "V", "mdi:flash-triangle", cvMin * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cellVoltageMinIndex = JkMqtt.GenerateSensorData<int>(deviceId, "Cell Voltage Min Index", "cell_voltage_min_index", null, "measurement", null, null, cvMinIndex + 1, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);

                            var chargeCycleCount = JkMqtt.GenerateSensorData<uint>(deviceId, "Charge Cycle Count", "charge_cycle_count", null, "measurement", null, null, info.CycleCount, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var chargingEnabled = JkMqtt.GenerateSensorData<int>(deviceId, "Charging Enabled", "charging_enabled", null, "measurement", null, null, info.CharginMosfetEnabled ? 1: 0, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var current = JkMqtt.GenerateSensorData<float>(deviceId, "Current", "current", "current", "measurement", "A", "mdi:current-dc", info.ChargeCurrent * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var cycleCapacity = JkMqtt.GenerateSensorData<float>(deviceId, "Cycle Capacity", "cycle_capacity", "current", "total_increasing", "Ah", null, info.CycleCapacity * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var dischargingEnabled = JkMqtt.GenerateSensorData<int>(deviceId, "Discharging Enabled", "discharging_enabled", null, "measurement", null, null, info.DisCharginMosfetEnabled ? 1 : 0, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var nominalCapacity = JkMqtt.GenerateSensorData<float>(deviceId, "Nominal Capacity", "nominal_capacity", "current", "measurement", "Ah", "mdi:current-dc", info.NominalCapacity * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);

                            var power = JkMqtt.GenerateSensorData<float>(deviceId, "Power", "power", "power", "measurement", "W", null, info.BatteryPower * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var stateOfCharge = JkMqtt.GenerateSensorData<byte>(deviceId, "State Of Charge", "state_of_charge", "battery", "measurement", "%", null, info.StateOfCharge, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var temperature1 = JkMqtt.GenerateSensorData<float>(deviceId, "Temperature 1", "temperature_1", "temperature", "measurement", "C", null, info.TemperatureProbe01 * 0.1f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var temperature2 = JkMqtt.GenerateSensorData<float>(deviceId, "Temperature 2", "temperature_2", "temperature", "measurement", "C", null, info.TemperatureProbe02 * 0.1f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var temperatureMosfet = JkMqtt.GenerateSensorData<float>(deviceId, "Temperature Mosfet", "temperature_mosfet", "temperature", "measurement", "C", null, info.PowerTubeTemperature * 0.1f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var totalRuntime = JkMqtt.GenerateSensorData<double>(deviceId, "Total Runtime", "total_runtime", "duration", "total_increasing", "s", null, info.TotalRuntime.TotalSeconds, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);
                            var totalVoltage = JkMqtt.GenerateSensorData<float>(deviceId, "Total Voltage", "total_voltage", "voltage", "measurement", "V", null, info.BatteryVoltage * 0.001f, deviceName, "Jikong", deviceModel, hardwareVersion, softwareVersion, deviceSerialNumber);


                            // Publish the device configuration data every 60 seconds
                            if (DateTime.Now.Subtract(this._lastDeviceConfigPublish).TotalSeconds > 60)
                            {
                                await JkMqtt.Publish(this._mqttClient, balanceAction.ConfigTopic, balanceAction.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, balanceCurrent.ConfigTopic, balanceCurrent.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, balancerEnabled.ConfigTopic, balancerEnabled.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, capacityRemaining.ConfigTopic, capacityRemaining.ConfigData);

                                foreach (var item in cellResistance)
                                {
                                    await JkMqtt.Publish(this._mqttClient, item.ConfigTopic, item.ConfigData);
                                }

                                foreach (var item in cellVoltage)
                                {
                                    await JkMqtt.Publish(this._mqttClient, item.ConfigTopic, item.ConfigData);
                                }

                                await JkMqtt.Publish(this._mqttClient, cellVoltgeAverage.ConfigTopic, cellVoltgeAverage.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, cellVoltageDelta.ConfigTopic, cellVoltageDelta.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, cellVoltgeMax.ConfigTopic, cellVoltgeMax.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, cellVoltageMaxIndex.ConfigTopic, cellVoltageMaxIndex.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, cellVoltageMin.ConfigTopic, cellVoltageMin.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, cellVoltageMinIndex.ConfigTopic, cellVoltageMinIndex.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, chargeCycleCount.ConfigTopic, chargeCycleCount.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, chargingEnabled.ConfigTopic, chargingEnabled.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, current.ConfigTopic, current.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, cycleCapacity.ConfigTopic, cycleCapacity.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, dischargingEnabled.ConfigTopic, dischargingEnabled.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, nominalCapacity.ConfigTopic, nominalCapacity.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, power.ConfigTopic, power.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, stateOfCharge.ConfigTopic, stateOfCharge.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, temperature1.ConfigTopic, temperature1.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, temperature2.ConfigTopic, temperature2.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, temperatureMosfet.ConfigTopic, temperatureMosfet.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, totalRuntime.ConfigTopic, totalRuntime.ConfigData);
                                await JkMqtt.Publish(this._mqttClient, totalVoltage.ConfigTopic, totalVoltage.ConfigData);

                                this._lastDeviceConfigPublish = DateTime.Now;
                            }

                            // Publish the state data
                            await JkMqtt.Publish(this._mqttClient, balanceCurrent.StateTopic, balanceCurrent.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, balanceAction.StateTopic, balanceAction.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, balancerEnabled.StateTopic, balancerEnabled.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, capacityRemaining.StateTopic, capacityRemaining.Value.ToString("0.000"));

                            foreach (var item in cellResistance)
                            {
                                await JkMqtt.Publish(this._mqttClient, item.StateTopic, item.Value.ToString("0.000"));
                            }

                            foreach (var item in cellVoltage)
                            {
                                await JkMqtt.Publish(this._mqttClient, item.StateTopic, item.Value.ToString("0.000"));
                            }

                            await JkMqtt.Publish(this._mqttClient, cellVoltgeAverage.StateTopic, cellVoltgeAverage.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, cellVoltageDelta.StateTopic, cellVoltageDelta.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, cellVoltgeMax.StateTopic, cellVoltgeMax.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, cellVoltageMaxIndex.StateTopic, cellVoltageMaxIndex.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, cellVoltageMin.StateTopic, cellVoltageMin.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, cellVoltageMinIndex.StateTopic, cellVoltageMinIndex.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, chargeCycleCount.StateTopic, chargeCycleCount.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, chargingEnabled.StateTopic, chargingEnabled.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, current.StateTopic, current.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, cycleCapacity.StateTopic, cycleCapacity.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, dischargingEnabled.StateTopic, dischargingEnabled.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, nominalCapacity.StateTopic, nominalCapacity.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, power.StateTopic, power.Value.ToString("0.000"));
                            await JkMqtt.Publish(this._mqttClient, stateOfCharge.StateTopic, stateOfCharge.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, temperature1.StateTopic, temperature1.Value.ToString("0.00"));
                            await JkMqtt.Publish(this._mqttClient, temperature2.StateTopic, temperature2.Value.ToString("0.00"));
                            await JkMqtt.Publish(this._mqttClient, temperatureMosfet.StateTopic, temperatureMosfet.Value.ToString("0.00"));
                            await JkMqtt.Publish(this._mqttClient, totalRuntime.StateTopic, totalRuntime.Value.ToString());
                            await JkMqtt.Publish(this._mqttClient, totalVoltage.StateTopic, totalVoltage.Value.ToString("0.000"));

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
