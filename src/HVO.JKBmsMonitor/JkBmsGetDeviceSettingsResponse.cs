namespace HVO.JKBmsMonitor
{
    public class JkBmsGetDeviceSettingsResponse : JkBmsResponse
    {
        public JkBmsGetDeviceSettingsResponse(ReadOnlyMemory<byte> data) : base(data)
        {
            this.InitializeFromPayload();
        }

        protected override void InitializeFromPayload()
        {
            var payload = this.Payload.Span;
        }
    }
}
