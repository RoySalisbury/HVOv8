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
             .ConfigureServices(context =>
             {
                 context.AddOptions();
                 context.AddOptions<JkBmsMonitorClientOptions>(nameof(JkBmsMonitorClientOptions));
                 context.AddOptions<JkBmsMonitorHostOptions>(nameof(JkBmsMonitorHostOptions));

                 context.AddSingleton<JkBmsMonitorClient>();
                 context.AddHostedService<JkBmsMonitorHost>();
             });
    }
}