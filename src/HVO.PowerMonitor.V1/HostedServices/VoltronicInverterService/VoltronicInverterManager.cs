using System.Collections.Concurrent;

namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    public sealed class VoltronicInverterManager : IVoltronicInverterManager, IDisposable
    {
        private readonly ConcurrentDictionary<string, VoltronicInverter> _inverterDictionary = new ConcurrentDictionary<string, VoltronicInverter>();

        public VoltronicInverterManager() { }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

}
