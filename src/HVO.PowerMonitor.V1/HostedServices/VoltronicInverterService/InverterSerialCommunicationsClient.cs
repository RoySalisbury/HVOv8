using System.IO.Ports;

namespace HVO.PowerMonitor.V1.HostedServices.VoltronicInverterService
{
    internal sealed class InverterSerialCommunicationsClient : IDisposable
    {
        public SerialPort _serialPort;
        private bool _disposed;

        public InverterSerialCommunicationsClient(VoltronicInverterOptions options)
        {
            this._serialPort = new SerialPort(options.PortPath, 2400, Parity.None, 8, StopBits.One);
        }

        public bool IsOpen => this._serialPort.IsOpen;

        public void Close(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed(GetType().FullName);

            if (this._serialPort.IsOpen == true)
            {
                this._serialPort.Close();
            }
        }

        public void Open(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                this._serialPort.Open();
            }
        }

        public Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            var bytesRead = this._serialPort.Read(buffer, offset, count);
            return Task.FromResult<int>(bytesRead);
        }

        public ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            this._serialPort.Write(buffer.ToArray(), 0, buffer.Length);
            return ValueTask.CompletedTask;
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            this._serialPort.DiscardInBuffer();
            this._serialPort.DiscardOutBuffer();

            return Task.CompletedTask;
        }

        private void ThrowIfDisposed(string callerName)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(callerName);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._serialPort.Dispose();
                    this._serialPort = null;
                }

                this._disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
