namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    public sealed class InverterResponseNak : VoltronicInverterResponse
    {
        public InverterResponseNak(ReadOnlyMemory<byte> response) : base(response)
        {
        }
    }

}
