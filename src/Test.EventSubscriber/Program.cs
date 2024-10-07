using MassTransit;
using Test.Event;

namespace Test.EventSubscriber
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<PingConsumer, PingConsumerDefination>(cfg =>
                {
                });

                //x.AddConfigureEndpointsCallback((c, configurator) =>
                //{
                //    if (configurator is IServiceBusReceiveEndpointConfigurator sbConfigurator)
                //    {
                //        sbConfigurator.DefaultMessageTimeToLive = TimeSpan.FromDays(1);
                //        sbConfigurator.EnableDeadLetteringOnMessageExpiration = true;
                //    }   
                //});

                x.SetKebabCaseEndpointNameFormatter();
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host("Endpoint=sb://hualapaivalleyobservatory.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=c171+vZy2bWw5w38qDLHn4tdBjvsUlHXmzYzsel/Xu0=");

                    cfg.ConfigureEndpoints(context);
                    cfg.AutoStart = true;
                });

            });

            var app = builder.Build();
            app.Run();
        }
    }

    public class PingConsumerDefination : ConsumerDefinition<PingConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<PingConsumer> consumerConfigurator, IRegistrationContext context)
        {
            endpointConfigurator.UseMessageRetry(r => r.Immediate(5));
        }
    }

    public class PingConsumer : IConsumer<Test.Event.PingEvent>
    {
        private readonly ILogger<PingConsumer> _logger;

        public PingConsumer(ILogger<PingConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Test.Event.PingEvent> context)
        {
            _logger.LogInformation("Ping: {Button}", context.Message.Button);
            return Task.CompletedTask;
        }
    }
}
