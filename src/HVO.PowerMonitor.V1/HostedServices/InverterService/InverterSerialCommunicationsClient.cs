using Microsoft.Extensions.Options;
using System.IO.Ports;

namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public sealed class InverterSerialCommunicationsClient : InverterCommunicationsClient
    {
        private SerialPort _serialPort;

        public InverterSerialCommunicationsClient(ILogger<InverterCommunicationsClient> logger, InverterClientOptions options) : base(logger, options)
        {
            this._serialPort = new SerialPort(this._options.PortPath, 2400, Parity.None, 8, StopBits.One);
        }

        public override void Close(CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == true)
            {
                this._serialPort.Close();
                this.IsOpen = this._serialPort.IsOpen;
            }
        }

        public override void Open(CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                this._serialPort.Open();
                this.IsOpen = this._serialPort.IsOpen;
            }
        }

        protected override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            var bytesRead = this._serialPort.Read(buffer, offset, count);
            return Task.FromResult<int>(bytesRead);
        }

        protected override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            this._serialPort.Write(buffer.ToArray(), 0, buffer.Length);
            return ValueTask.CompletedTask;
        }

        protected override Task FlushAsync(CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            this._serialPort.DiscardInBuffer();
            this._serialPort.DiscardOutBuffer();

            return Task.CompletedTask;
        }

        public override ReadOnlyMemory<byte> GenerateGetRequest(string commandCode, bool includeCrc = true)
        {
            // Get the command bytes
            var commandBytes = System.Text.Encoding.ASCII.GetBytes(commandCode);

            // Generate the request
            List<byte> request = new List<byte>();
            request.AddRange(commandBytes);

            if (includeCrc)
            {
                // Get the CRC for this payload
                var crc = CalculateCrc(commandBytes, 0);
                request.AddRange(crc);
            }

            request.Add(0x0D);

            //Console.WriteLine($"Command: {commandCode}, Data: {BitConverterExtras.BytesToHexString(request.ToArray())}");
            return request.ToArray();
        }


        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                this._serialPort.Dispose();
                this._serialPort = null;
            }
        }
    }
}
