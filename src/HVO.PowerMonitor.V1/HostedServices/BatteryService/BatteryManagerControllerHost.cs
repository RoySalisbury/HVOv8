using System.Threading;

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
            // We need to create an instance of each configured BMS and add it to the controller.

            var service = new JBDBatteryManagerService() { ServiceId = Guid.NewGuid() }; // This shuuld be a unique value so we have a consistent ID each time we start.

            if (this._batteryManagerController.TryAddService(service))
            {
                service.Initialize();
//                service.Start(cancellationToken);
            }


            throw new NotImplementedException();
        }
    }
}
