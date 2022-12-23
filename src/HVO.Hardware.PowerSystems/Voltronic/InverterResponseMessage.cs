namespace HVO.Hardware.PowerSystems.Voltronic
{
    public abstract class InverterResponseMessage : InverterMessage
    {
        protected InverterResponseMessage(string command) : base(command)
        {
        }

        protected virtual void InitializeFromPayload(Span<byte> payload) { }

        public static InverterResponseMessage CreateInstance(InverterRequestMessage request, ReadOnlyMemory<byte> response)
        {
            if (request == null)
            {
                return null;
            }

            // The response must have at least the hidReportId byte 
            if (response.IsEmpty)
            {
                return null;
            }

            // It is now up to this 'factory' to determine the Response 'type' and Initialize the instance from the respone data.
            return null;
        }
    }
}