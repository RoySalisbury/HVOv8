namespace HVO.Hardware.PowerSystems.Voltronic
{
    public abstract class InverterRequestMessage : InverterMessage
    {
        protected InverterRequestMessage(string command) : base(command)
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
            var packetData = new byte[header.Length + payload.Length + crc.Length];

            Array.Copy(header, 0, packetData, 0, header.Length);
            Array.Copy(payload, 0, packetData, header.Length, payload.Length);
            Array.Copy(crc, 0, packetData, packetData.Length - crc.Length, crc.Length);

            return packetData;
        }

        protected internal virtual byte[] PayloadBytes()
        {
            return System.Text.Encoding.ASCII.GetBytes(Command);
        }
    }
}