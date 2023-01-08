using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    public class VoltronicInverterManagerHost : BackgroundService
    {
        private readonly ILogger<VoltronicInverterManagerHost> _logger;
        private readonly IVoltronicInverterManager _inverterManager;

        public VoltronicInverterManagerHost(ILogger<VoltronicInverterManagerHost> logger, IVoltronicInverterManager inverterManager)
        {
            this._logger = logger;
            this._inverterManager = inverterManager;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }

}
