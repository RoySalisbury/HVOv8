namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterCommunicationsClientOptions
    {
        public string PortPath { get; set; } = "/dev/hidrawX";
        public ushort MaxPollingRateMs { get; set; } = 200;
    }
}