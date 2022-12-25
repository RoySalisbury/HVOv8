namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterClientOptions
    {
        public string PortPath { get; set; } = "/dev/hidraw3";
        public ushort MaxPollingRateMs { get; set; } = 50;
    }
}