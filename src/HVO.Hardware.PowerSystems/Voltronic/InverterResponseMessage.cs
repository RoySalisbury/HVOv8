namespace HVO.Hardware.PowerSystems.Voltronic
{
    public abstract class InverterResponseMessage : InverterMessage
    {
        protected InverterResponseMessage(string command) : base(command)
        {
        }

        protected virtual void InitializeFromPayload(ReadOnlySpan<byte> payload) { }

        public static InverterResponseMessage CreateInstance(InverterRequestMessage request, ReadOnlySpan<byte> response)
        {
            if (request == null)
            {
                return null;
            }

            // The response must have something .. the CRC at least
            if ((response.IsEmpty) || (response.Length < 2))
            {
                return null;
            }

            // Get just the payload (no CRC)
            var payload = response.Slice(0, response.Length - 2);

            // All responses are of this type
            InverterResponseMessage result = null;

            // validate the CRC.
            if (ValidateCrc(payload, response.Slice(response.Length - 2)) == false)
            {
                //throw new ArgumentException("CRC validation of the repsonse does not match expected calculation.");
                return null;
            }

            // It is now up to this 'factory' to determine the Response 'type' and Initialize the instance from the respone data.
            switch (request)
            {
                case InverterGetDeviceProtocolIDRequest r:
                    result = new InverterGetDeviceProtocolIDResponse();
                    break;
                case InverterGetSerialNumberRequest r:
                    result = new InverterGetSerialNumberResponse();
                    break;
                default:
                    break;
            }

            // Initialize the instance form the payload data
            result?.InitializeFromPayload(payload);

            return result;
        }
    }
}