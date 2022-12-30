using Microsoft.Extensions.Options;

namespace HVO.PowerMonitor.V1.HostedServices.Voltronic
{
    public class VoltronicInverterHost : BackgroundService
    {
        private readonly ILogger<VoltronicInverterHost> _logger;
        private readonly InverterClientOptions _options;
        private readonly IInverterClient _inverterClient;

        public VoltronicInverterHost(ILogger<VoltronicInverterHost> logger, IOptions<InverterClientOptions> options, LoggerFactory loggerFactory) 
        {
            this._logger = logger;
            this._options = options.Value;


            var clientLogger = loggerFactory.CreateLogger<IInverterClient>();
            switch (this._options.PortType)
            {
                case PortDeviceType.Serial:
                    this._inverterClient = new InverterSerialClient(clientLogger, this._options);
                    break;
                case PortDeviceType.Hidraw:
                    this._inverterClient = new InverterHidrawClient(clientLogger, this._options);
                    break;
                case PortDeviceType.USB:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new NotImplementedException();
                    break;
            }
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }

}
