namespace HVO.PowerMonitor.V1.HostedServices.BatteryService
{
    public class BatteryManagerControllerHost : BackgroundService
    {
        private readonly ILogger<BatteryManagerControllerHost> _logger;
        private readonly IBatteryManagerController _batteryManagerController;

        public BatteryManagerControllerHost(ILogger<BatteryManagerControllerHost> logger, IBatteryManagerController batteryManagerController)
        {
            this._logger = logger;
            this._batteryManagerController = batteryManagerController;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
    }
}
