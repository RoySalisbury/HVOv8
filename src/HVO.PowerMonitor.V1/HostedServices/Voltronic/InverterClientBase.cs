namespace HVO.PowerMonitor.V1.HostedServices.Voltronic
{
    public abstract class InverterClientBase : IDisposable, IInverterClient
    {
        private static readonly ushort[] CrcTable = { 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7, 0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef };

        public bool IsOpen { get; protected set; }

        public abstract void Close(CancellationToken cancellationToken = default);
        public abstract Task FlushAsync(CancellationToken cancellationToken = default);
        public abstract void Open(CancellationToken cancellationToken = default);
        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        public abstract ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

        protected static byte[] CalculateCrc(Span<byte> data, ushort seed = 0)
        {
            ushort crc = seed;
            foreach (byte b in data)
            {
                int da = (byte)(crc >> 8) >> 4;
                crc <<= 4;
                crc ^= CrcTable[da ^ b >> 4];
                da = (byte)(crc >> 8) >> 4;
                crc <<= 4;
                crc ^= CrcTable[da ^ b & 0x0F];
            }

            byte crcLow = (byte)crc;
            byte crcHigh = (byte)(crc >> 8);
            if (crcLow is 0x28 or 0x0d or 0x0a)
            {
                crcLow++;
            }

            if (crcHigh is 0x28 or 0x0d or 0x0a)
            {
                crcHigh++;
            }

            //crc = (ushort)(crcHigh << 8);
            //crc += crcLow;

            return new byte[] { crcHigh, crcLow };
        }

        protected volatile bool _disposed = false;
        protected abstract void Dispose(bool disposing);
        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
