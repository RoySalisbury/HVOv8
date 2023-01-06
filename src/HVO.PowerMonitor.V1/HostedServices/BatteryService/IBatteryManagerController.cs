namespace HVO.PowerMonitor.V1.HostedServices.BatteryService
{
    // The public interface that will be used for accessing the individual BatteryManagerService (BMS) instances that are configured
    public interface IBatteryManagerController
    {
        bool IsInitialized { get; }
        IEnumerable<BatteryManagerService> Services { get; }

        // Adds a new service instance to the controllers list.
        bool TryAddService(BatteryManagerService service);

        // Removes a service from the running colleciton (not from the configuration). It is up to the CALLER to dispose of the instance returned.
        bool TryRemoveService(Guid serviceId, out BatteryManagerService service);

        bool TryGetService(Guid serviceId, out BatteryManagerService service);
    }

    public sealed class BatteryManagerController : IBatteryManagerController 
    {
        private readonly ILogger<IBatteryManagerController> _logger;

        public BatteryManagerController(ILogger<IBatteryManagerController> logger) 
        {
            this._logger = logger;
        }

        public bool IsInitialized => throw new NotImplementedException();

        public IEnumerable<BatteryManagerService> Services => throw new NotImplementedException();

        public bool TryAddService(BatteryManagerService service)
        {
            throw new NotImplementedException();
        }

        public bool TryGetService(Guid serviceId, out BatteryManagerService service)
        {
            throw new NotImplementedException();
        }

        public bool TryRemoveService(Guid serviceId, out BatteryManagerService service)
        {
            throw new NotImplementedException();
        }
    }
}
