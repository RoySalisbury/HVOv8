using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Buffers.Binary;

namespace HVO.JKBmsMonitor
{
    public sealed class JkBmsMonitorClient : IDisposable
    {
        private readonly ILogger<JkBmsMonitorClient> _logger;
        private readonly JkBmsMonitorClientOptions _jkBmsMonitorClientOptions;
        private bool _disposed;

        private const string JkBmsServiceUUID = "0000ffe0-0000-1000-8000-00805f9b34fb";

        private Adapter _bluetoothAdapter;
        private GattCharacteristic _writeCharacteristic = null;
        private GattCharacteristic _notifyCharacteristic = null;

        private readonly List<byte> _notificationBuffer = new List<byte>();

        public JkBmsMonitorClient(ILogger<JkBmsMonitorClient> logger, IOptions<JkBmsMonitorClientOptions> jkBmsMonitorClientOptions)
        {
            this._logger = logger;
            this._jkBmsMonitorClientOptions = jkBmsMonitorClientOptions.Value;
        }

        public bool AdaptorInitialized { get; private set; } = false;

        public async Task<bool> InitializeAdaptorAsync(string adaptorName, bool fullName = false, CancellationToken stoppingToken = default)
        {
            if (this.AdaptorInitialized == true)
            {
                return true;
            }

            this._bluetoothAdapter = await BlueZManager.GetAdapterAsync(adaptorName, fullName);
            this.AdaptorInitialized = true;

            return this.AdaptorInitialized;
        }

        private async Task<Device> FindDeviceAsync(string deviceAddress, bool scanIfNecessary, int timeout = 20)
        {
            // First try to direct connect to the device (short cut the scan delay).
            Device device = await this._bluetoothAdapter.GetDeviceAsync(deviceAddress);
            if ((device is null) && (scanIfNecessary == true))
            {
                using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                using (await this._bluetoothAdapter.WatchDevicesAddedAsync(async foundDevice =>
                {
                    if (cancellationTokenSource.IsCancellationRequested == false)
                    {
                        var a = await foundDevice.GetAddressAsync();
                        var deviceName = await foundDevice.GetAliasAsync();

                        if (a.Equals(deviceAddress, StringComparison.OrdinalIgnoreCase) || deviceName.Contains(deviceAddress, StringComparison.OrdinalIgnoreCase))
                        {
                            device = foundDevice;
                            cancellationTokenSource.Cancel();
                        }
                    }
                }))
                {
                    await this._bluetoothAdapter.StartDiscoveryAsync();
                    await Task.Delay(TimeSpan.FromSeconds(timeout), cancellationTokenSource.Token).ContinueWith(async t =>
                    {
                        await this._bluetoothAdapter.StopDiscoveryAsync();
                    });
                }

            }

            return device;
        }


        public async Task ConnectToDeviceAsync(string deviceAddress, bool scanIfNecessary, int timeout = 20)
        {
            if (AdaptorInitialized == false)
            {
                throw new Exception("Adaptor not initialized");
            }

            var device = await FindDeviceAsync(deviceAddress, scanIfNecessary, timeout);
            if (device is null)
            {
                throw new Exception("Device not found");
            }

            await device.ConnectAsync();
            // TODO: Wait for the connect property to be set

            //await device.GetServicesAsync();
            // TODO: Wait for the ServicesResolved property to be set

            //var servicesUUIDs = await device.GetUUIDsAsync();

            var service = await device.GetServiceAsync(JkBmsServiceUUID);
            if (service is null)
            {
                Console.WriteLine($"Service UUID {JkBmsServiceUUID} not found. Do you need to pair first?");
            }

            var characteristics = await service.GetCharacteristicsAsync();
            foreach (var item in characteristics)
            {
                var flags = await item.GetFlagsAsync();
                if ((this._writeCharacteristic is null) && flags.Intersect(new[] { "write" }).Any())
                {
                    this._writeCharacteristic = await service.GetCharacteristicAsync(await item.GetUUIDAsync());
                }

                if ((this._notifyCharacteristic is null) && flags.Intersect(new[] { "notify" }).Any())
                {
                    this._notifyCharacteristic = await service.GetCharacteristicAsync(await item.GetUUIDAsync());
                    this._notifyCharacteristic.Value += DeviceNotifyCharacteristic_Value;
                }

                if ((this._writeCharacteristic is not null) && (this._notifyCharacteristic is not null))
                {
                    break;
                }
            }

        }

        private async Task DeviceNotifyCharacteristic_Value(GattCharacteristic sender, GattCharacteristicValueEventArgs eventArgs)
        {
            var header = new byte[] { 0x55, 0xAA, 0xEB, 0x90 };
            if (header.SequenceEqual(eventArgs.Value[0..4])) 
            {
                this._notificationBuffer.Clear();
            }

            this._notificationBuffer.AddRange(eventArgs.Value);
            if (this._notificationBuffer.Count >= 300)
            {
                if (this._notificationBuffer.Count > 320)
                {
                    Console.WriteLine("Buffer longer than expected");
                }

                DecodeResponse(this._notificationBuffer.ToArray());

                Console.WriteLine($"Notify: {BitConverter.ToString(this._notificationBuffer.ToArray())}");
                this._notificationBuffer.Clear();
            }
        }

        private void DecodeResponse(ReadOnlySpan<byte> data)
        {
            // The first 4 bytes are the header

            // Byte 5 is the command code (0x96, 0x97)

            // byte 6 is the record type (0x01, 0x02, 0x03)

            // The CRC is at byte 300, even if the packet is larger (e.g., 320)

            switch (data[4])
            {
                case 0x01: // settings
                    {
                        break;
                    }
                case 0x02: // JK02 uses 2 byte cell values
                    {
                        // (0) 55-AA-EB-90
                        // (4) 02
                        // (5) 44
                        // (6) 2E-0E   CellVoltage01
                        // (8) 2F-0E   CellVoltage02
                        // (10) 2D-0E   CellVoltage03
                        // (12) 2F-0E   CellVoltage04
                        // (14) 2F-0E   CellVoltage05
                        // (16) 2F-0E   CellVoltage06
                        // (18) 2F-0E   CellVoltage07
                        // (20) 2F-0E   CellVoltage08
                        // (22) 2E-0E   CellVoltage08
                        // (24) 2E-0E   CellVoltage11
                        // (26) 2F-0E   CellVoltage11
                        // (28) 2F-0E   CellVoltage12
                        // (30) 2F-0E   CellVoltage13
                        // (32) 2E-0E   CellVoltage14
                        // (34) 2F-0E   CellVoltage15
                        // (36) 2E-0E   CellVoltage16
                        // (38) 00-00   CellVoltage17
                        // (40) 00-00   CellVoltage18
                        // (42) 00-00   CellVoltage19
                        // (44) 00-00   CellVoltage20
                        // (46) 00-00   CellVoltage21
                        // (48) 00-00   CellVoltage22
                        // (50) 00-00   CellVoltage23
                        // (52) 00-00   CellVoltage24
                        //              ... Can go to 32 cells with offset on some devices ...
                        //
                        // (54) FF-FF-00-00  EnabledCellsBitmask
                        // (58) 2E-0E  Average Cell Voltage
                        // (60) 02-00  delta Cell Voltage
                        // (62) 01  Max Voltage Cell Index
                        // (63) 02  Min Voltage Cell Index

                        // (64)  37-00  CellResistance01
                        // (66)  37-00  CellResistance02
                        // (68)  35-00  CellResistance03
                        // (70)  34-00  CellResistance04
                        // (72)  34-00  CellResistance05
                        // (74)  34-00  CellResistance06
                        // (76)  34-00  CellResistance07
                        // (78)  35-00  CellResistance08
                        // (80)  35-00  CellResistance09
                        // (82)  36-00  CellResistance10
                        // (84)  39-00  CellResistance11
                        // (86)  38-00  CellResistance12
                        // (88)  3B-00  CellResistance13
                        // (90)  3A-00  CellResistance14
                        // (92)  34-00  CellResistance15
                        // (94)  35-00  CellResistance16
                        // (96)  00-00  CellResistance17
                        // (98)  00-00  CellResistance18
                        // (100)  00-00  CellResistance19
                        // (102)  00-00  CellResistance20
                        // (104)  00-00  CellResistance21
                        // (106)  00-00  CellResistance22
                        // (108)  00-00  CellResistance23
                        // (110)  00-00  CellResistance24
                        //              ... Can go to 32 cells with offset on some devices ...

                        // OffsetStart


                        // (112)  00-00  Power Tube Temperature
                        // (114)  00-00-00-00   Wire resistance warning bitmask

                        // (118)  E4-E2-00-00   Battery Voltage
                        // (122)  00-00-00-00   Battery Power
                        // (126)  00-00-00-00   Charge Current

                        // (130)  06-01  Temp Sensor 1
                        // (132)  01-01  Temp Sensor 2
                        // (134)  03-01  Power Tube Temp Sensor -OR- Errors Bitmask
                        //
                        // (136)  00-00  System Alarms Bitmask
                        // (138)  00-00  Balance Current
                        // (140)  00     Bslance Action (0 = off, 1 = Charging, 2 = Discharging) 
                        // (141)  62     State of Charge
                        // (142)  C5-3A-04-00 Capacity Remaining
                        // (146)  C0-45-04-00 Nominal Capacity
                        // (150)  20-00-00-00 Cycle Count
                        // (154)  05-98-89-00 Cycle Capacity
                        // (158)  64-00
                        // (160)  6A-0A
                        // (162)  88-AF-7B-01 Total runtime in seconds
                        // (166)  01    Charge Mosfet Enabled (0, 1)
                        // (167)  01    Discharge Mosfet enabled (0, 1)
                        // (168)  6B
                        // (169)  06-00
                        // (171)  00-00
                        // (173)  00-00
                        // (175)  00-00
                        // (177)  00-00
                        // (179)  00-00
                        // (181)  00-07
                        // (183)  00-01
                        // (185)  00-00
                        // (187)  00-C1
                        // (189)  03-00
                        // (190)  00
                        // (191)  00
                        // (192)  00
                        // (193)  28-C5
                        // (195)  3E-40
                        // (197)  00-00-00-00-E2-04-00-00-00-00
                        // (207)  00-01-00-05-00-00-E9
                        // (214)  25-56-08-00 Uptime 100ms
                        // (218)  00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
                        // (299)  3E    CRC

                        int offset = 0;
                        int numberOfCells = 24 + (offset / 2);

                        var cellVoltages = new short[numberOfCells];
                        var cellResistance = new short[numberOfCells];

                        for (int i = 0; i < numberOfCells; i++)
                        {
                            cellVoltages[i] = BitConverter.ToInt16(data.Slice((i * 2) + 6, 2));
                            cellResistance[i] = BitConverter.ToInt16(data.Slice((i * 2) + 64 + offset, 2));
                        }

                        var averageCellVoltage = BitConverter.ToInt16(data.Slice((58 + offset), 2));
                        var deltaCellVoltge = BitConverter.ToInt16(data.Slice((60 + offset), 2));
                        var maxCellVolageIndex = data[62];
                        var minCellVolageIndex = data[63];

                        offset = offset * 2;

                        var powerTubeTemperature = BitConverter.ToInt16(data.Slice(112 + offset, 2));
                        var wireResistatnceWarning01 = data[114 + offset];
                        var wireResistatnceWarning02 = data[115 + offset];
                        var wireResistatnceWarning03 = data[116 + offset];
                        var wireResistatnceWarning04 = data[117 + offset];

                        var batteryVoltage = BitConverter.ToInt32(data.Slice(118 + offset, 4));
                        var batteryPower = BitConverter.ToInt32(data.Slice(122 + offset, 4)); // WARNING: Unsigned. Calculate manually (v*c)
                        var chargeCurrent = BitConverter.ToInt32(data.Slice(126 + offset, 4));

                        




                        break;
                    }
                case 0x03:
                    {
                        var vendorId = ASCIIEncoding.ASCII.GetString(data.Slice(6, 16));
                        var hardwareVersion = ASCIIEncoding.ASCII.GetString(data.Slice(22, 8));
                        var softwareVersion = ASCIIEncoding.ASCII.GetString(data.Slice(30, 8));
                        var uptime = BitConverter.ToInt32(data.Slice(38, 4));
                        var powerOnCount = BitConverter.ToInt32(data.Slice(42, 4));
                        var deviceName = ASCIIEncoding.ASCII.GetString(data.Slice(46, 16));
                        var devicePasscode = ASCIIEncoding.ASCII.GetString(data.Slice(62, 16));
                        var manufactureDate = ASCIIEncoding.ASCII.GetString(data.Slice(78, 8));
                        var serialNumber = ASCIIEncoding.ASCII.GetString(data.Slice(86, 11));
                        var passcode = ASCIIEncoding.ASCII.GetString(data.Slice(97, 5));
                        var userData = ASCIIEncoding.ASCII.GetString(data.Slice(102, 16));
                        var setupPasscode = ASCIIEncoding.ASCII.GetString(data.Slice(118, 16));

                        break;
                    }
                default:
                    break;
            }
        }

        public async Task RequestDeviceInfo()
        {
            if (this._writeCharacteristic is not null)
            {
                var getInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x97, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
                getInfo[^1] = JKCRC(getInfo[0..^1]);

                var options = new Dictionary<string, object>();
                await this._writeCharacteristic.WriteValueAsync(getInfo, options);
            }
        }

        public async Task RequestCellInfo()
        {
            if (this._writeCharacteristic is not null)
            {
                var getCellInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x96, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
                getCellInfo[^1] = JKCRC(getCellInfo[0..^1]);

                var options = new Dictionary<string, object>();
                await this._writeCharacteristic.WriteValueAsync(getCellInfo, options);
            }
        }

        static byte JKCRC(byte[] data)
        {
            return (byte)(data.Sum(x => x) & 0xFF);
        }



        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (this._notifyCharacteristic is not null)
                    {
                        this._notifyCharacteristic.Value -= DeviceNotifyCharacteristic_Value;
                        this._notifyCharacteristic?.Dispose();
                        this._notifyCharacteristic = null;
                    }

                    this._writeCharacteristic?.Dispose();
                    this._writeCharacteristic = null;

                    this._bluetoothAdapter?.Dispose();
                    this._bluetoothAdapter = null;

                    this.AdaptorInitialized = false;
                }

                _disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
