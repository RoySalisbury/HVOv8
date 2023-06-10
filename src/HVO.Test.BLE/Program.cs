using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Linux.Bluetooth;
using Linux.Bluetooth.Extensions;

namespace HVO.Test.BLE
{
    internal class Program
    {
        static TimeSpan timeout = TimeSpan.FromSeconds(15);


        public const string deviceFilter = "C8:47:8C:E4:54:B1";
        public const string _serviceUUID = "0000ffe0-0000-1000-8000-00805f9b34fb";

        static async Task Main(string[] args)
        {
            Adapter adapter = await BlueZManager.GetAdapterAsync("hci0", false);

            var adapterPath = adapter.ObjectPath.ToString();
            var adapterName = adapterPath[(adapterPath.LastIndexOf("/") + 1)..];
            Console.WriteLine($"Using Bluetooth adapter {adapterName}");

            adapter.PoweredOn += Adapter_PoweredOnAsync;
            adapter.DeviceFound += Adapter_DeviceFoundAsync;

            Console.WriteLine("Waiting for events. Use Control-C to quit.");
            Console.WriteLine();
            await Task.Delay(-1);
        }

        private static async Task<string> GetDeviceDescriptionAsync(IDevice1 device)
        {
            var deviceProperties = await device.GetAllAsync();
            return $"{deviceProperties.Address} (Alias: {deviceProperties.Alias}, RSSI: {deviceProperties.RSSI})";
        }

        private static async Task Adapter_PoweredOnAsync(Adapter sender, BlueZEventArgs e)
        {
            try
            {
                if (e.IsStateChange) {
                    Console.WriteLine("Bluetooth adapter powered on.");
                } else {
                    Console.WriteLine("Bluetooth adapter already powered on.");
                }

                Console.WriteLine("Starting scan...");
                await sender.StartDiscoveryAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task Adapter_DeviceFoundAsync(Adapter sender, DeviceFoundEventArgs e)
        {
            try
            {
                var device = e.Device;

                var deviceDescription = await GetDeviceDescriptionAsync(device);
                
                if (e.IsStateChange) {
                    //Console.WriteLine($"Found: [NEW] {deviceDescription}");
                } else {
                    //Console.WriteLine($"Found: {deviceDescription}");
                }

                var deviceAddress = await device.GetAddressAsync();
                var deviceName = await device.GetAliasAsync();
                if (deviceAddress.Equals(deviceFilter, StringComparison.OrdinalIgnoreCase) || deviceName.Contains(deviceFilter, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Requested device '{deviceAddress}' found. Stopping scan....");
                    try
                    {
                        await sender.StopDiscoveryAsync();
                        Console.WriteLine("Scan stopped.");
                    }
                    catch (Exception ex)
                    {
                        // Best effort. Sometimes BlueZ gets in a state where you can't stop the scan.
                        Console.Error.WriteLine($"Error stopping scan: {ex.Message}");
                    }

                    device.Connected += Device_ConnectedAsync;
                    device.Disconnected += Device_DisconnectedAsync;
                    device.ServicesResolved += Device_ServicesResolvedAsync;

                    Console.WriteLine($"Connecting to {await device.GetAddressAsync()}...");
                    await device.ConnectAsync();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task Device_ConnectedAsync(Device sender, BlueZEventArgs e)
        {
            try
            {
                if (e.IsStateChange) {
                    Console.WriteLine($"Connected to {await sender.GetAddressAsync()}");
                } else {
                    Console.WriteLine($"Already connected to {await sender.GetAddressAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task Device_DisconnectedAsync(Device sender, BlueZEventArgs e)
        {
            try
            {
                Console.WriteLine($"Disconnected from {await sender.GetAddressAsync()}");
                await Task.Delay(TimeSpan.FromSeconds(15));

                Console.WriteLine($"Attempting to reconnect to {await sender.GetAddressAsync()}...");
                await sender.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task Device_ServicesResolvedAsync(Device sender, BlueZEventArgs e)
        {
            try
            {
                if (e.IsStateChange) {
                    Console.WriteLine($"Services resolved for {await sender.GetAddressAsync()}");
                } else {
                    Console.WriteLine($"Services already resolved for {await sender.GetAddressAsync()}");
                }

                var servicesUUIDs = await sender.GetUUIDsAsync();

                Console.WriteLine($"Device offers {servicesUUIDs.Length} service(s).");
                // foreach (var uuid in servicesUUIDs)
                // {
                //   Console.WriteLine(uuid);
                // }

                var service = await sender.GetServiceAsync(_serviceUUID);
                if (service == null)
                {
                    Console.WriteLine($"Service UUID {_serviceUUID} not found. Do you need to pair first?");
                    return;
                }

                GattCharacteristic writeCharacteristic = null;
                GattCharacteristic notifyCharacteristic = null;

                var characteristics = await service.GetCharacteristicsAsync();
                Console.WriteLine($"Service offers {characteristics.Count} characteristics(s).");
                foreach (var item in characteristics)
                {
                    var flags = await item.GetFlagsAsync();
                    if ((writeCharacteristic == null) && flags.Intersect(new [] {"write", "write-without-response" }).Any())
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

                if (writeCharacteristic != null)
                {
                    var getInfo = new byte[] { 0xAA, 0x55, 0x90, 0xEB, 0x97, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x11 };
                    var options = new Dictionary<string, object>();
                    await writeCharacteristic.WriteValueAsync(getInfo, options);
                }

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static async Task NotifyCharacteristic_Value(GattCharacteristic sender, GattCharacteristicValueEventArgs e)
        {
            try
            {
                var uuid = await sender.GetUUIDAsync();
                Console.WriteLine($"Notify Sender: {uuid}, \tLength: {e.Value.Length} \tValue: {BitConverter.ToString(e.Value)}");

                var response = AssembleResponseValue(sender, e.Value, 320);
                if (response.Item1 == true)
                {
                    Console.WriteLine($"=====Value: {BitConverter.ToString(response.Item2)}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private static byte[] responseBuffer = new byte[0];

        private static (bool, byte[]) AssembleResponseValue(GattCharacteristic sender, byte[] value, int minLength)
        {
            // Flush buffer on every preamble
            if ((value[0] == 0x55) && (value[1] == 0xAA) && (value[2] == 0xEB) && (value[3] == 0x90))
            {
                responseBuffer = value;
            } else {
                responseBuffer = responseBuffer.Concat(value).ToArray();
            }

            if (responseBuffer.Length < minLength)
            {
                return (false, null);
            }

            var result = (true, responseBuffer.ToArray());
            responseBuffer = new byte[0];

            return result;
        }
    }
}