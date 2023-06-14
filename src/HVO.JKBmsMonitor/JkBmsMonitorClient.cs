using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Reflection;

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

        public JkBmsMonitorClient(ILogger<JkBmsMonitorClient> logger, IOptions<JkBmsMonitorClientOptions> jkBmsMonitorClientOptions)
        {
            this._logger = logger;
            this._jkBmsMonitorClientOptions = jkBmsMonitorClientOptions.Value;
        }

        public bool AdaptorInitialized { get; private set; } = false;

        public async Task<bool> InitializeAdaptorAsync(string adaptorName, bool fullName = false)
        {
            if (this.AdaptorInitialized == true)
            {
                return true;
            }

            this._bluetoothAdapter = await BlueZManager.GetAdapterAsync("hci0", false);
            return true;
        }

        private async Task<Device> FindDeviceAsync(string deviceAddress, bool scanIfNecessary, int timeout = 20)
        {
            // First try to direct connect to the device (short cut the scan delay).
            try
            {
                return await this._bluetoothAdapter.GetDeviceAsync(deviceAddress);
            }
            catch (Exception)
            {
                if (scanIfNecessary)
                {
                    Device device = null;
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
                        await Task.Delay(timeout, cancellationTokenSource.Token).ContinueWith(async t => 
                        { 
                            await this._bluetoothAdapter.StopDiscoveryAsync();
                        });
                    }

                    return device;
                }
            }

            return null;
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

            device.Connected += Device_Connected;

            device.Disconnected += Device_Disconnected;
            await device.ConnectAsync();
        }

        private async Task Device_Connected(Device sender, BlueZEventArgs eventArgs)
        {
            sender.Connected -= Device_Connected;

            var service = await sender.GetServiceAsync(JkBmsServiceUUID);
            if (service == null)
            {
                Console.WriteLine($"Service UUID {JkBmsServiceUUID} not found. Do you need to pair first?");
            }

            var characteristics = await service.GetCharacteristicsAsync();
            foreach (var item in characteristics)
            {
                var flags = await item.GetFlagsAsync();
                if ((this._writeCharacteristic is null) && flags.Intersect(new[] { "write", "write-without-response" }).Any())
                {
                    this._writeCharacteristic = await service.GetCharacteristicAsync(await item.GetUUIDAsync());
                    Console.WriteLine($"Write Characteristic: {await item.GetUUIDAsync()}");
                }

                if ((this._notifyCharacteristic is null) && flags.Intersect(new[] { "notify" }).Any())
                {
                    this._notifyCharacteristic = await service.GetCharacteristicAsync(await item.GetUUIDAsync());
                    Console.WriteLine($"Notify Characteristic: {await item.GetUUIDAsync()}");

                    await this._notifyCharacteristic.StopNotifyAsync();
                    this._notifyCharacteristic.Value += DeviceNotifyCharacteristic_Value;
                }

                if ((this._writeCharacteristic is not null) && (this._notifyCharacteristic is not null))
                {
                    break;
                }
            }
        }

        private Task Device_Disconnected(Device sender, BlueZEventArgs eventArgs)
        {
            sender.Disconnected -= Device_Disconnected;
            return Task.CompletedTask; 
        }


        //private async Task Device_ServicesResolved(Device sender, BlueZEventArgs eventArgs)
        //{
        //    // The device is connected and the services are resolved for this device.  We can now setup the write/notify hanlders.
        //    sender.ServicesResolved -= Device_ServicesResolved;

        //}

        private async Task DeviceNotifyCharacteristic_Value(GattCharacteristic sender, GattCharacteristicValueEventArgs eventArgs)
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


        public async Task RequestDeviceInfo()
        {
            if (this._writeCharacteristic != null)
            {
                var getInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x97, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 };
                var options = new Dictionary<string, object>();
                await this._writeCharacteristic.WriteValueAsync(getInfo, options);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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
