using System.IO.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterSerialClient : IDisposable, IInverterClient
    {
        private readonly ILogger<IInverterClient> _logger;
        private readonly InverterClientOptions _options;

        private bool _disposed;

        private SerialPort _serialPort;

        internal InverterSerialClient(ILogger<IInverterClient> logger, InverterClientOptions options)
        {
            this._logger = logger;
            this._options = options;

            this._serialPort = new SerialPort(this._options.PortPath, 2400, Parity.None, 8, StopBits.One);            
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
                this._serialPort.Open();
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
                this._serialPort.Close();
                this.IsOpen = false;
            }
        }

        private void Dispose(bool disposing)
        {
            if (this._disposed == false)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this._serialPort.Dispose();
                    this._serialPort = null;
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
            var bytesRead = this._serialPort.Read(buffer, offset, count);
            return Task.FromResult<int>(bytesRead);
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            this._serialPort.Write(buffer.ToArray(), 0, buffer.Length);
            return ValueTask.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            this._serialPort.DiscardInBuffer();
            this._serialPort.DiscardOutBuffer();

            return Task.CompletedTask;
        }
    }
}