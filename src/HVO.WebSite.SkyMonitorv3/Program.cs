using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;

namespace HVO.WebSite.SkyMonitorv3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), MapAndLoad);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static IntPtr MapAndLoad(string libraryName, Assembly assembly, DllImportSearchPath? dllImportSearchPath)
        {
            var libPath = "runtimes";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        libraryName = System.IO.Path.Combine(libPath, "win-x86", libraryName);
                        break;
                    case Architecture.X64:
                        libraryName = System.IO.Path.Combine(libPath, "win-x64", libraryName);
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X86:
                        libraryName = System.IO.Path.Combine(libPath, "linux-x86", $"lib{libraryName}");
                        break;
                    case Architecture.X64:
                        libraryName = System.IO.Path.Combine(libPath, "linux-x64", $"lib{libraryName}");
                        break;
                    case Architecture.Arm:
                        libraryName = System.IO.Path.Combine(libPath, "linux-arm", $"lib{libraryName}");
                        break;
                    case Architecture.Arm64:
                        libraryName = System.IO.Path.Combine(libPath, "linux-arm64", $"lib{libraryName}");
                        break;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                switch (RuntimeInformation.OSArchitecture)
                {
                    case Architecture.X64:
                        libraryName = System.IO.Path.Combine(libPath, "osx-x64", $"lib{libraryName}");
                        break;
                }
            }

            //Console.WriteLine($"Loading Library: {libraryName}");
            NativeLibrary.TryLoad(libraryName, assembly, dllImportSearchPath, out var libHandle);
            return libHandle;
        }
    }
}
