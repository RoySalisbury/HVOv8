using System;
using System.Diagnostics.Contracts;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterGetSerialNumberResponse : InverterResponseMessage
    {
        public InverterGetSerialNumberResponse() : base("QID")
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

            SerialNumber = System.Text.Encoding.ASCII.GetString(payload);
        }

        public string SerialNumber { get; private set; }

    }

}