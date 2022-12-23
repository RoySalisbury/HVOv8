namespace HVO.Hardware.PowerSystems.Voltronic
{
    public abstract class InverterRequest : InverterMessage
    {
        protected InverterRequest(string command) : base(command)
        {
        }

        public virtual byte[] ToBytes()
        {
            // Since this is actually a HID device, the ReportID goes here.
            var header = new byte[1] { HidReportId };

            // The specific instance will be responsible for overriding this method and providing us the data as bytes.
            var payload = PayloadBytes();

            // Get the CRC for this payload
            var crc = CalculateCrc(payload, 0);

            // Im sure this is a better way to do this, but this is clean and easy to understand.
            List<byte> packetData = new List<byte>() { HidReportId };
            packetData.AddRange(payload);
            packetData.AddRange(crc);
            packetData.Add(0x0D);

            return packetData.ToArray();
        }

        protected internal virtual byte[] PayloadBytes()
        {
            return System.Text.Encoding.ASCII.GetBytes(Command);
        }
    }
}