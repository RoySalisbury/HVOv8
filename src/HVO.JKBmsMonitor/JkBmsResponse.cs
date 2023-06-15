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

            switch (data[4])
            {
                case 0x01:
                    // 55-AA-EB-90-01-42-58-02-00-00-28-0A-00-00-5A-0A-00-00-3D-0E-00-00-2E-0E-00-00-03-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-C4-09-00-00-A0-86-01-00-1E-00-00-00-3C-00-00-00-F0-49-02-00-2C-01-00-00-3C-00-00-00-3C-00-00-00-D0-07-00-00-BC-02-00-00-58-02-00-00-BC-02-00-00-58-02-00-00-38-FF-FF-FF-9C-FF-FF-FF-84-03-00-00-BC-02-00-00-10-00-00-00-01-00-00-00-01-00-00-00-01-00-00-00-C0-45-04-00-DC-05-00-00-48-0D-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-D1
                    Console.WriteLine("JK02 - Response Type 1");
                    break;
                case 0x02:
                    // 55-AA-EB-90-02-42-00-0D-FF-0C-FF-0C-FF-0C-FF-0C-FF-0C-FF-0C-00-0D-FF-0C-00-0D-00-0D-FF-0C-00-0D-00-0D-00-0D-FF-0C-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-FF-FF-00-00-FF-0C-01-00-06-00-37-00-37-00-35-00-34-00-34-00-34-00-34-00-35-00-35-00-36-00-39-00-38-00-3B-00-3A-00-34-00-35-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-F1-CF-00-00-6F-07-02-00-3E-F6-FF-FF-06-01-09-01-F2-00-00-00-00-00-00-5D-CE-03-04-00-C0-45-04-00-20-00-00-00-38-CF-89-00-64-00-7A-02-4A-00-7C-01-01-01-6A-06-00-00-00-00-00-00-00-00-00-00-00-00-07-00-01-00-00-00-C1-03-00-00-0D-00-28-C5-3E-40-00-00-00-00-E2-04-00-00-00-00-00-01-00-05-00-00-77-4D-59-08-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-B4
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
