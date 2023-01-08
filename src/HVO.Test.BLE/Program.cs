using ProrepubliQ.DotNetBlueZ;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.Test.BLE
{
    internal class Program
    {
        private static TimeSpan timeout = TimeSpan.FromSeconds(180);

        private static async Task Main(string[] args)
        {
            //if (args.Length < 1)
            {
                // 24:0A:C4:0E:87:F2
                // C8:47:8C:E4:54:B1
                // 70:3E:97:08:17:37
                //Console.WriteLine("Usage: PrintDeviceInfo <deviceAddress> [adapterName]");
                //Console.WriteLine("Example: PrintDeviceInfo AA:BB:CC:11:22:33 hci1");
                //return;
            }

            var deviceAddress = "70:3E:97:08:17:37"; //args[0];

            IAdapter1 adapter;
            if (args.Length > 1)
            {
                adapter = await BlueZManager.GetAdapterAsync(args[1]);
            }
            else
            {
                var adapters = await BlueZManager.GetAdaptersAsync();
                if (adapters.Count == 0) throw new Exception("No Bluetooth adapters found.");

                adapter = adapters.First();
            }

            var adapterPath = adapter.ObjectPath.ToString();
            var adapterName = adapterPath.Substring(adapterPath.LastIndexOf("/") + 1);
            Console.WriteLine($"Using Bluetooth adapter {adapterName}");

            // Find the Bluetooth peripheral.
            var device = await adapter.GetDeviceAsync(deviceAddress);
            if (device == null)
            {
                Console.WriteLine(
                    $"Bluetooth peripheral with address '{deviceAddress}' not found. Use `bluetoothctl` or Bluetooth Manager to scan and possibly pair first.");
                return;
            }

            Console.WriteLine("Connecting...");
            await device.ConnectAsync();
            await device.WaitForPropertyValueAsync("Connected", value: true, timeout);
            Console.WriteLine("Connected.");

            Console.WriteLine("Waiting for services to resolve...");
            await device.WaitForPropertyValueAsync("ServicesResolved", value: true, timeout);

            var service = await device.GetServiceAsync("0000ff00-0000-1000-8000-00805f9b34fb");
            var characteristicRX = await service.GetCharacteristicAsync("0000ff01-0000-1000-8000-00805f9b34fb");
            var characteristicTX = await service.GetCharacteristicAsync("0000ff02-0000-1000-8000-00805f9b34fb");

            if (characteristicRX != null)
            {
                var properties = await characteristicRX.GetAllAsync();
                Console.WriteLine($"UUID: {properties.UUID}, \tFlags: {string.Join(", ", properties.Flags)}");
            }

            if (characteristicTX != null)
            {
                var properties = await characteristicTX.GetAllAsync();
                Console.WriteLine($"UUID: {properties.UUID}, \tFlags: {string.Join(", ", properties.Flags)}");
            }


            //var servicesUUID = await device.GetUUIDsAsync();
            //Console.WriteLine($"Device offers {servicesUUID.Length} service(s).");
            //foreach (var s in servicesUUID)
            //{
            //    Console.WriteLine($"ServiceUUID: {s}");

            //    var service1 = await device.GetServiceAsync(s);
            //    if (service1 != null)
            //    {
            //        var serviceProperties = await service1.GetAllAsync();

            //        var characteristics = await service1.GetCharacteristicsAsync();
            //        foreach (var characteristic in characteristics)
            //        {
            //            var properties = await characteristic.GetAllAsync();
            //            Console.WriteLine($"UUID: {properties.UUID}");
            //            Console.WriteLine($"UUID: {properties.UUID}");

            //            var uuid = await characteristic.GetUUIDAsync();
            //            Console.WriteLine($"\tCharacteristicUUID: {uuid}");

            //            var flags = await characteristic.GetFlagsAsync();
            //            foreach (var flag in flags)
            //            {
            //                Console.WriteLine($"\t\tFlag: {flag}");
            //            }

            //        }
            //    }
            //}





            //var deviceInfoServiceFound = servicesUUID.Any(uuid =>
            //    String.Equals(uuid, GattConstants.DeviceInformationServiceUUID, StringComparison.OrdinalIgnoreCase));
            //if (!deviceInfoServiceFound)
            //{
            //    Console.WriteLine("Device doesn't have the Device Information Service. Try pairing first?");
            //    return;
            //}

            //// Console.WriteLine("Retrieving Device Information service...");
            //var service = await device.GetServiceAsync(GattConstants.DeviceInformationServiceUUID);
            //var modelNameCharacteristic = await service.GetCharacteristicAsync(GattConstants.ModelNameCharacteristicUUID);
            //var manufacturerCharacteristic =
            //    await service.GetCharacteristicAsync(GattConstants.ManufacturerNameCharacteristicUUID);

            //int characteristicsFound = 0;
            //if (modelNameCharacteristic != null)
            //{
            //    characteristicsFound++;
            //    Console.WriteLine("Reading model name characteristic...");
            //    var modelNameBytes = await modelNameCharacteristic.ReadValueAsync(timeout);
            //    Console.WriteLine($"Model name: {Encoding.UTF8.GetString(modelNameBytes)}");
            //}

            //if (manufacturerCharacteristic != null)
            //{
            //    characteristicsFound++;
            //    Console.WriteLine("Reading manufacturer characteristic...");
            //    var manufacturerBytes = await manufacturerCharacteristic.ReadValueAsync(timeout);
            //    Console.WriteLine($"Manufacturer: {Encoding.UTF8.GetString(manufacturerBytes)}");
            //}

            //if (characteristicsFound == 0) Console.WriteLine("Model name and manufacturer characteristics not found.");

            await device.DisconnectAsync();
            Console.WriteLine("Disconnected.");
        }
    }
}