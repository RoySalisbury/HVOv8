namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    public sealed class InverterResponseAck : VoltronicInverterResponse
    {
        public InverterResponseAck(ReadOnlyMemory<byte> response) : base(response) 
        { 
        }

    }

}
