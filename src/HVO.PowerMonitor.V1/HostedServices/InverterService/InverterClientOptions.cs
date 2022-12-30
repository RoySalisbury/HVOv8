namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public sealed class InverterClientOptions
    {
        //public string PortPath { get; set; } = "/dev/hidraw0";
        //public PortDeviceType PortType { get; set; } = PortDeviceType.Hidraw;

        //public string PortPath { get; set; } = "/dev/ttyUSB0";

        public string PortPath { get; set; } = "COM3";
        public PortDeviceType PortType { get; set; } = PortDeviceType.Serial;
        public ushort MaxPollingRateMs { get; set; } = 25;
    }


}
