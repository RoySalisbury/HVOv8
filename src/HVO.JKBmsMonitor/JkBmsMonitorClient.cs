using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using System;
using System.Reflection;

namespace HVO.JKBmsMonitor
{
    public class JkBmsMonitorClient : IDisposable
    {
        private readonly ILogger<JkBmsMonitorClient> _logger;
        private readonly JkBmsMonitorClientOptions _jkBmsMonitorClientOptions;
        private bool _disposed;

        private Adapter _bluetoothAdapter;

        public const string deviceFilter = "C8:47:8C:E4:54:B1";
        public const string _serviceUUID = "0000ffe0-0000-1000-8000-00805f9b34fb";


        public JkBmsMonitorClient(ILogger<JkBmsMonitorClient> logger, IOptions<JkBmsMonitorClientOptions> jkBmsMonitorClientOptions)
        {
            this._logger = logger;
            this._jkBmsMonitorClientOptions = jkBmsMonitorClientOptions.Value;
        }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(JkBmsMonitorClient));
            }

            if (this.IsInitialized)
            {
                throw new Exception("Already Initialized");
            }
            
            this._bluetoothAdapter = await BlueZManager.GetAdapterAsync("hci0", false);

            var adapterPath = _bluetoothAdapter.ObjectPath.ToString();
            var adapterName = adapterPath[(adapterPath.LastIndexOf("/") + 1)..];
            Console.WriteLine($"Using Bluetooth adapter {adapterName}");

            this.IsInitialized = true;
        }

        public bool IsInitialized { get; private set; } = false;


        public async Task<bool> ScanAndConnect()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(JkBmsMonitorClient));
            }

            // Make sure the adapter is powered up
            var powerState = await this._bluetoothAdapter.GetPoweredAsync();
            if (powerState == false)
            {
                await this._bluetoothAdapter.SetPoweredAsync(true);
            }

            // First try to direct connect to the device (short cut the scan delay).
            Device device = null;
            try
            {
                device = await this._bluetoothAdapter.GetDeviceAsync(deviceFilter);
            }
            catch (Exception ex)
            {
                using (await this._bluetoothAdapter.WatchDevicesAddedAsync(async d =>
                {
                    var deviceAddress = await d.GetAddressAsync();
                    var deviceName = await d.GetAliasAsync();
                    if (deviceAddress.Equals(deviceFilter, StringComparison.OrdinalIgnoreCase) || deviceName.Contains(deviceFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        await this._bluetoothAdapter.StopDiscoveryAsync();
                        device = d;
                    }
                })) 
                {
                    await this._bluetoothAdapter.StartDiscoveryAsync();
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    await this._bluetoothAdapter.StopDiscoveryAsync();
                }
            }

            if (device == null)
            {
                return false;
            }
            return await SetupDevice(device);
        }

        GattCharacteristic writeCharacteristic = null;
        GattCharacteristic notifyCharacteristic = null;

        private async Task<bool> SetupDevice(Device device)
        {
            Console.WriteLine($"Connecting to {await device.GetAddressAsync()}...");
            await device.ConnectAsync();

            await device.GetServicesAsync();
            var servicesUUIDs = await device.GetUUIDsAsync();

            var service = await device.GetServiceAsync(_serviceUUID);
            if (service == null)
            {
                Console.WriteLine($"Service UUID {_serviceUUID} not found. Do you need to pair first?");
                return false;
            }


            var characteristics = await service.GetCharacteristicsAsync();
            Console.WriteLine($"Service offers {characteristics.Count} characteristics(s).");
            foreach (var item in characteristics)
            {
                var flags = await item.GetFlagsAsync();
                if ((writeCharacteristic == null) && flags.Intersect(new[] { "write", "write-without-response" }).Any())
                {
                    writeCharacteristic = await service.GetCharacteristicAsync(await item.GetUUIDAsync());
                    Console.WriteLine($"Write Characteristic: {await item.GetUUIDAsync()}");
                }

                if ((notifyCharacteristic == null) && flags.Intersect(new[] { "notify" }).Any())
                {
                    notifyCharacteristic = await service.GetCharacteristicAsync(await item.GetUUIDAsync());
                    Console.WriteLine($"Notify Characteristic: {await item.GetUUIDAsync()}");

                    notifyCharacteristic.Value += NotifyCharacteristic_Value;
                }

                if ((writeCharacteristic != null) && (notifyCharacteristic != null))
                {
                    break;
                }
            }

            if ((writeCharacteristic == null) || (notifyCharacteristic == null))
            {
                return false;
            }

            return true;
        }

        private async Task NotifyCharacteristic_Value(GattCharacteristic sender, GattCharacteristicValueEventArgs eventArgs)
        {
            try
            {
                var uuid = await sender.GetUUIDAsync();
                Console.WriteLine($"Notify Sender: {uuid}, \tLength: {eventArgs.Value.Length} \tValue: {BitConverter.ToString(eventArgs.Value)}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public async Task Test()
        {
            if (writeCharacteristic != null)
            {
                var getInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x97, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 };
                var options = new Dictionary<string, object>();
                await writeCharacteristic.WriteValueAsync(getInfo, options);
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this._bluetoothAdapter?.Dispose();
                    this._bluetoothAdapter = null;
                }

                _disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }


    }
}
