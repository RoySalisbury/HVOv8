using System;
using System.Diagnostics.Contracts;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterGetSerialNumberResponse : InverterResponseMessage
    {
        public InverterGetSerialNumberResponse() : base("QID")
        {
        }

        protected override void InitializeFromPayload(Span<byte> payload)
        {
            // The base 'abstract' version does nothing, but for completness/standardization we call it. At some point it may do something.
            base.InitializeFromPayload(payload);

            // The payload consists of just the data and NOT the header or CRC.
            if (payload == null || payload.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            SerialNumber = System.Text.Encoding.ASCII.GetString(payload);
        }

        public string SerialNumber { get; private set; }

    }
}