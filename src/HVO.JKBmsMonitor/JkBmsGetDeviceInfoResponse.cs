namespace HVO.JKBmsMonitor
{
    public class JkBmsGetDeviceInfoResponse : JkBmsResponse
    {
        public JkBmsGetDeviceInfoResponse() : base() { }

        protected override void InitializeFromPayload(ReadOnlySpan<byte> payload)
        {
            base.InitializeFromPayload(payload);
        }
    }



}
