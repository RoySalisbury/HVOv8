using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Buffers.Binary;
using System;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.FileSystemGlobbing;
using Tmds.DBus;

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

                Action<Device> watchForDeviceHandler = async (foundDevice) =>
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
                };

                using (await this._bluetoothAdapter.WatchDevicesAddedAsync(watchForDeviceHandler))
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

        private Task _devicePropertyWatcherTask;

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

            this._devicePropertyWatcherTask = device.WatchPropertiesAsync(delegate (PropertyChanges propertyChanges)
            {
                try
                {
                    foreach (var kvp in propertyChanges.Changed)
                    {
                        Console.WriteLine($"Property Changed: {kvp.Key}, Value: {kvp.Value}");
                    }
                    foreach (var s in propertyChanges.Invalidated)
                    {
                        Console.WriteLine($"Property Invalidated: {s}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex}");
                }
            });

            await device.ConnectAsync();

            // Are we connected?
            if (await device.GetConnectedAsync() == false)
            {
                try
                {
                    await device.WaitForPropertyValueAsync("Connected", value: true, timeout: TimeSpan.FromSeconds(timeout));
                }
                catch (TimeoutException)
                {
                    throw;
                }
            }

            // Are the services resolved?
            if (await device.GetServicesResolvedAsync() == false)
            {
                try
                {
                    await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout: TimeSpan.FromSeconds(timeout));
                }
                catch (TimeoutException)
                {
                    throw;
                }
            }

            //// Wait for the ServicesResolved property to be set
            //var retryResolveCheckCount = 0;
            //while (retryResolveCheckCount < 20)
            //{
            //    if (await device.GetServicesResolvedAsync())
            //    {
            //        break;
            //    }
            //    Console.WriteLine("Resolving Services");
            //    await Task.Delay(250);
            //    retryResolveCheckCount++;
            //}

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
                    //this._notifyCharacteristic.Value += DeviceNotifyCharacteristic_Value;
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

                var buffer = this._notificationBuffer.ToArray();
                this._notificationBuffer.Clear();

                var protocolVersion = 2;
                var response = JkBmsResponse.CreateInstance(buffer, protocolVersion);
                if (response is not null)
                {
                    if (response is JkBmsGetCellInfoResponse cellInfoResponse)
                    {
                        Console.WriteLine($"CellInfo  -  Count: {cellInfoResponse.CellVoltages.Length}, Battery Voltage: {cellInfoResponse.BatteryVoltage} mV, Temp #1: {cellInfoResponse.TemperatureProbe01}, Temp #2: {cellInfoResponse.TemperatureProbe02}, Mosfet Temp: {cellInfoResponse.PowerTubeTemperature}");
                    }
                    // Fire new EventHandler with completed packet response
                }

                Console.WriteLine($"Notify: {BitConverter.ToString(buffer)}");
            }
        }

        public async Task RequestDeviceInfo()
        {
            if (this._writeCharacteristic is not null)
            {
                var getInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x97, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
                getInfo[^1] = JKCRC(getInfo[0..^1]);

                var options = new Dictionary<string, object>
                {
                    { "type", "request" }
                };
                await this._writeCharacteristic.WriteValueAsync(getInfo, options);
            }
        }

        public async Task RequestCellInfo02()
        {
            if (this._writeCharacteristic is not null)
            {
                var getCellInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x96, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
                getCellInfo[^1] = JKCRC(getCellInfo[0..^1]);

                var options = new Dictionary<string, object>
                {
                    { "type", "request" }
                };
                await this._writeCharacteristic.WriteValueAsync(getCellInfo, options);
            }
        }

        public async Task RequestCellInfo01()
        {
            if (this._writeCharacteristic is not null)
            {
                var getCellInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x96, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF };
                getCellInfo[^1] = JKCRC(getCellInfo[0..^1]);

                var options = new Dictionary<string, object>
                {
                    { "type", "request" }
                };

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
