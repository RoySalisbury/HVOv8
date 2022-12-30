using Microsoft.Extensions.Options;

namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public sealed class InverterHidrawCommunicationsClient : InverterCommunicationsClient
    {
        private FileStream _fileStream;

        public InverterHidrawCommunicationsClient(ILogger<InverterCommunicationsClient> logger, InverterClientOptions options) : base(logger, options)
        {
        }

        public override void Close(CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == true)
            {
                this._fileStream.Close();
                this.IsOpen = false;
            }
        }

        public override void Open(CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                this._fileStream = new FileStream(this._options.PortPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
                this.IsOpen = true;
            }
        }

        protected override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            return this._fileStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        protected override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            return this._fileStream.WriteAsync(buffer, cancellationToken: cancellationToken);
        }
       
        protected override Task FlushAsync(CancellationToken cancellationToken = default)
        {
            base.ThrowIfDisposed(GetType().FullName);

            if (this.IsOpen == false)
            {
                throw new InvalidOperationException("Device not open");
            }

            this._fileStream.FlushAsync(cancellationToken);
            return Task.CompletedTask;
        }

        public override ReadOnlyMemory<byte> GenerateGetRequest(string commandCode, bool includeCrc = true)
        {
            // Get the command bytes
            var commandBytes = System.Text.Encoding.ASCII.GetBytes(commandCode);

            // Generate the request
            List<byte> request = new List<byte>() { 0 };
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
            this._fileStream.Dispose();
        }
    }


}
