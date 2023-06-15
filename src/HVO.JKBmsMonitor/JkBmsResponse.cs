namespace HVO.JKBmsMonitor
{
    public abstract class JkBmsResponse
    {
        protected JkBmsResponse(int protocolVersion) 
        {
            this.ProtocolVersion = protocolVersion;
        }

        protected int ProtocolVersion { get; set; }

        public ReadOnlyMemory<byte> Payload { get; private set; }

        protected virtual void InitializeFromPayload(ReadOnlySpan<byte> payload)
        {
            this.Payload = payload.ToArray();
        }

        public static JkBmsResponse CreateInstance(ReadOnlySpan<byte> data, int protocolVersion = 2)
        {
            if (data.IsEmpty)
            {
                return null;
            }

            // The response must be within the expected size range
            if ((data.Length < 300) || (data.Length > 320))
            {
                return null;
            }

            var header = new byte[] { 0x55, 0xAA, 0xEB, 0x90 };
            if (MemoryExtensions.SequenceEqual(header, data[0..4])  == false)
            {
                return null;
            }

            // Validate the CRC. 
            if (ValidateCrc(data[0..299], data[299]) == false)
            {
                //throw new ArgumentException("CRC validation of the repsonse does not match expected calculation.");
                return null;
            }

            // All responses are of this type
            JkBmsResponse result = null;

            switch (data[6])
            {
                case 0x01:
                    Console.WriteLine("JK02 - Response Type 1");
                    break;
                case 0x02:
                    Console.WriteLine("JK02 - Response Type 2");
                    result = new JkBmsGetCellInfoResponse(protocolVersion);
                    break;
                case 0x03:
                    Console.WriteLine("JK02 - Response Type 3");
                    result = new JkBmsGetDeviceInfoResponse(protocolVersion);
                    break;
                default:
                    Console.WriteLine("JK02 - Response Type ?");
                    break;
            }

            // Initialize the instance from the data
            result?.InitializeFromPayload(data);
            return result;
        }

        private static bool ValidateCrc(ReadOnlySpan<byte> data, byte originalCrc)
        {
            int calculatedCrc = 0;
            for (int i = 0; i < data.Length; i++)
            {
                calculatedCrc += data[i];
            }

            return (calculatedCrc & 0xFF) == originalCrc;
        }
    }



}
