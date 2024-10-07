using MassTransit;
using System.Reflection;
using Test.Event;

namespace Test.EventPublisher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddMassTransit(x =>
            {
                x.SetKebabCaseEndpointNameFormatter();
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host("Endpoint=sb://hualapaivalleyobservatory.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=c171+vZy2bWw5w38qDLHn4tdBjvsUlHXmzYzsel/Xu0=");

                    cfg.Publish<PingEvent>(x => {
                        x.CreateTopicOptions.DefaultMessageTimeToLive = TimeSpan.FromDays(7);
                        x.CreateTopicOptions.SupportOrdering = true;
                    });
                    cfg.AutoStart = true;
                });

            });

            builder.Services.AddHostedService<PingPublisher>();

            var app = builder.Build();
            app.Run();
        }
    }


    public class PingPublisher : BackgroundService
    {
        private readonly ILogger<PingPublisher> _logger;
        private readonly IBus _bus;

        public PingPublisher(ILogger<PingPublisher> logger, IBus bus)
        {
            _logger = logger;
            _bus = bus;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Yield();

                var keyPressed = Console.ReadKey(true);
                if (keyPressed.Key != ConsoleKey.Escape)
                {
                    //_logger.LogInformation("Pressed key: {key}", keyPressed.Key);
                    await _bus.Publish(new Test.Event.PingEvent(keyPressed.Key.ToString()), stoppingToken);
                }

                await Task.Delay(200, stoppingToken);
            }
        }
    }
}
