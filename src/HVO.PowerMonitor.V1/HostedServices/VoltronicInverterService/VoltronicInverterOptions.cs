namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    public class VoltronicInverterOptions
    {
        public string PortPath { get; set; } = "COM3";
        public PortDeviceType PortType { get; set; } = PortDeviceType.Serial;
        public ushort MaxPollingRateMs { get; set; } = 25;
    }

}
