namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterGetDeviceProtocolIDResponse : InverterResponse
    {
        public InverterGetDeviceProtocolIDResponse() : base("QPI")
        {
        }

        protected override void InitializeFromPayload(ReadOnlySpan<byte> payload)
        {
            // The base 'abstract' version does nothing, but for completness/standardization we call it. At some point it may do something.
            base.InitializeFromPayload(payload);

            if (payload.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            ProtocolID = System.Text.Encoding.ASCII.GetString(payload);
        }

        public string ProtocolID { get; private set; }
    }
}