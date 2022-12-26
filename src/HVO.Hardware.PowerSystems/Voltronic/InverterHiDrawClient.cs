using Microsoft.Extensions.Logging;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterHiDrawClient : IDisposable, IInverterClient
    {
        private readonly ILogger<IInverterClient> _logger;
        private readonly InverterClientOptions _options;
        private Stream _deviceStream;

        private bool _disposed;

        internal InverterHiDrawClient(ILogger<IInverterClient> logger, InverterClientOptions options) 
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
                    this._deviceStream?.Dispose();
                    this._deviceStream = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this._disposed = true;
            }
        }

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
    }

}