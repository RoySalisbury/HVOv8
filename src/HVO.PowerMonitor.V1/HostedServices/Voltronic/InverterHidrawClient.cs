namespace HVO.PowerMonitor.V1.HostedServices.Voltronic
{
    public sealed class InverterHidrawClient : InverterClientBase
    {
        private readonly ILogger<IInverterClient> _logger;
        private readonly InverterClientOptions _options;

        private FileStream _fileStream;

        public InverterHidrawClient(ILogger<IInverterClient> logger, InverterClientOptions options)
        {
            this._logger = logger;
            this._options = options;
        }

        public override void Open(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                this._fileStream = new FileStream(_options.PortPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
                this.IsOpen = true;
            }
        }

        public override void Close(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == true)
            {
                this._fileStream.Close();
                this.IsOpen = false;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (this._disposed == false)
            {
                if (disposing)
                {
                    this._fileStream.Dispose();
                }

                this._disposed = true;
            }
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            return this._fileStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            return this._fileStream.WriteAsync(buffer, cancellationToken: cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            this._fileStream.FlushAsync(cancellationToken);
            return Task.CompletedTask;
        }
    }
}
