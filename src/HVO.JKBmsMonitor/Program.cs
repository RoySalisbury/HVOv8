using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HVO.JKBmsMonitor
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args);
            await host.RunConsoleAsync(o => o.SuppressStatusMessages = true);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
             .UseSystemd()
             .ConfigureServices((context, services) =>
             {
                 services.AddSystemd();
                 services.Configure<JkBmsMonitorHostOptions>(context.Configuration.GetSection(nameof(JkBmsMonitorHostOptions)));
                 services.Configure<JkBmsMonitorClientOptions>(context.Configuration.GetSection(nameof(JkBmsMonitorClientOptions)));

                 services.AddSingleton<JkBmsMonitorClient>();
                 services.AddHostedService<JkBmsMonitorHost>();
             });
    }
}