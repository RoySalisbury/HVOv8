namespace HVO.JKBmsMonitor
{
    public abstract class JkBmsResponse
    {
        protected JkBmsResponse(int protocolVersion, ReadOnlyMemory<byte> data) 
        {
            this.ProtocolVersion = protocolVersion;
            this.Payload = data;

            this.InitializeFromPayload();
        }

        protected int ProtocolVersion { get; set; }

        public ReadOnlyMemory<byte> Payload { get; private set; }

        protected abstract void InitializeFromPayload();
    }
}
