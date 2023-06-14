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
                        var cellVoltage01 = BitConverter.ToInt16(data.Slice(6, 2)) / 1000.0;
                        var cellVoltage02 = BitConverter.ToInt16(data.Slice(8, 2));
                        var cellVoltage03 = BitConverter.ToInt16(data.Slice(10, 2));
                        var cellVoltage04 = BitConverter.ToInt16(data.Slice(12, 2));
                        var cellVoltage05 = BitConverter.ToInt16(data.Slice(14, 2));
                        var cellVoltage06 = BitConverter.ToInt16(data.Slice(16, 2));
                        var cellVoltage07 = BitConverter.ToInt16(data.Slice(18, 2));
                        var cellVoltage08 = BitConverter.ToInt16(data.Slice(20, 2));




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
