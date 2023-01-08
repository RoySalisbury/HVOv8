namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    public abstract class VoltronicInverterResponse 
    {
        protected readonly ReadOnlyMemory<byte> _response;

        protected VoltronicInverterResponse(ReadOnlyMemory<byte> response) 
        {
            this._response = response;
        }
    }

}
