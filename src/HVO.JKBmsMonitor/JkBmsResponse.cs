namespace HVO.JKBmsMonitor
{
    public abstract class JkBmsResponse
    {
        protected JkBmsResponse(ReadOnlyMemory<byte> data) 
        {
            this.Payload = data;

            this.InitializeFromPayload();
        }

        public ReadOnlyMemory<byte> Payload { get; private set; }

        protected abstract void InitializeFromPayload();
    }
}
