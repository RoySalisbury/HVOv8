namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterClientOptions
    {
        //public string PortPath { get; set; } = "/dev/hidraw0";
        //public PortDeviceType PortType { get; set; } = PortDeviceType.Hidraw;
        public string PortPath { get; set; } = "/dev/ttyUSB0";
        public PortDeviceType PortType { get; set; } = PortDeviceType.Serial;
        public ushort MaxPollingRateMs { get; set; } = 50;
    }

    public enum PortDeviceType 
    {
        Serial,
        Hidraw,
        USB
    }
}