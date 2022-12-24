namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterClientOptions
    {
        public string PortPath { get; set; } = "/dev/hidraw2";
        public ushort MaxPollingRateMs { get; set; } = 50;
    }
}