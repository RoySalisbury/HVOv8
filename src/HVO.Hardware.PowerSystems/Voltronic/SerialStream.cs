

using HVO.Hardware.PowerSystems.Voltronic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HVO.Hardware.PowerSystems
{
    public interface IInverterCommunications : IDisposable
    {
        private static readonly ushort[] CrcTable = { 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7, 0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef };

        bool IsOpen { get; }

        void Open(CancellationToken cancellationToken = default);
        void Close(CancellationToken cancellationToken = default);

        Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);
        ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
        Task FlushAsync(CancellationToken cancellationToken = default);

        ReadOnlyMemory<byte> GenerateGetRequest(string commandCode, bool includeCrc = true);

        byte[] CalculateCrc(Span<byte> data, ushort seed = 0)
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
    }


    public sealed class HiDrawStream : IDisposable, IInverterCommunications
    {
        private readonly ILogger<IInverterCommunications> _logger;
        private readonly InverterClientOptions _options;
        private Stream _deviceStream;

        private bool _disposed;

        internal HiDrawStream(ILogger<IInverterCommunications> logger, InverterClientOptions options) 
        {
            this._logger = logger;
            this._options = new InverterClientOptions();
        }

        public bool IsOpen { get; private set; }
        

        public void Open(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                _deviceStream = new FileStream(_options.PortPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
                this.IsOpen = true;
            }
        }

        public void Close(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == true)
            {
                _deviceStream.Close();
                this.IsOpen = false;
            }
        }


        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~HiDrawStream()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            return this._deviceStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            return this._deviceStream.WriteAsync(buffer, cancellationToken);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            return this._deviceStream.FlushAsync(cancellationToken);
        }

        public ReadOnlyMemory<byte> GenerateGetRequest(string commandCode, bool includeCrc = true)
        {
            // Get the command bytes
            var commandBytes = System.Text.Encoding.ASCII.GetBytes(commandCode);

            // Generate the request
            List<byte> request = new List<byte>() { 0 };
            request.AddRange(commandBytes);

            if (includeCrc)
            {
                // Get the CRC for this payload
                var crc = ((IInverterCommunications)this).CalculateCrc(commandBytes, 0);
                request.AddRange(crc);
            }

            request.Add(0x0D);

            //Console.WriteLine($"Command: {commandCode}, Data: {BitConverterExtras.BytesToHexString(request.ToArray())}");
            return request.ToArray();
        }
    }


    public sealed class SerialStream : IDisposable, IInverterCommunications
    {
        private bool disposedValue;

        public bool IsOpen => throw new NotImplementedException();

        public SerialStream () {}

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SerialStream()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Open(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Close(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ReadOnlyMemory<byte> GenerateGetRequest(string commandCode, bool includeCrc = true)
        {
            throw new NotImplementedException();
        }
    }


}