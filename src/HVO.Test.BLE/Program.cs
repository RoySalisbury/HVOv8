using ProrepubliQ.DotNetBlueZ;

namespace HVO.Test.BLE
{
    internal class Program
    {
        private static TimeSpan timeout = TimeSpan.FromSeconds(15);

        private static async Task Main(string[] args)
        {
            // if (args.Length < 1 || args.Length > 2 || args[0].ToLowerInvariant() == "-h" ||
            //     !int.TryParse(args[0], out int scanSeconds))
            // {
            //     Console.WriteLine("Usage: scan <SecondsToScan> [adapterName]");
            //     Console.WriteLine("Example: scan 15 hci0");
            //     return;
            // }

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

            // Print out the devices we already know about.
            var devices = await adapter.GetDevicesAsync();
            foreach (var device in devices)
            {
                string deviceDescription = await GetDeviceDescriptionAsync(device);
                Console.WriteLine(deviceDescription);
            }

            Console.WriteLine($"{devices.Count} device(s) found ahead of scan.");

            Console.WriteLine();

            // Scan for more devices.
            Console.WriteLine($"Scanning for {(int)timeout.TotalSeconds} seconds...");

            int newDevices = 0;
            using (await adapter.WatchDevicesAddedAsync(async device =>
            {
                newDevices++;
                // Write a message when we detect new devices during the scan.
                string deviceDescription = await GetDeviceDescriptionAsync(device);
                Console.WriteLine($"[NEW] {deviceDescription}");
            }))
            {
                await adapter.StartDiscoveryAsync();
                await Task.Delay(TimeSpan.FromSeconds((int)timeout.TotalSeconds));
            }

            Console.WriteLine($"Scan complete. {newDevices} new device(s) found.");
        }

        private static async Task<string> GetDeviceDescriptionAsync(IDevice1 device)
        {
            var address = await device.GetAddressAsync();
            var alias = await device.GetAliasAsync();
            return $"{alias} (Address: {address})";
        }
    }
}