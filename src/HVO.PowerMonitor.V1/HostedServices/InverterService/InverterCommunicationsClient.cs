using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public abstract class InverterCommunicationsClient : IDisposable
    {
        private static readonly ushort[] CrcTable = { 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7, 0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef };

        protected readonly ILogger<InverterCommunicationsClient> _logger;
        protected readonly InverterClientOptions _options;
        protected bool _disposed;
        private readonly Stopwatch _lastPoll = Stopwatch.StartNew();

        protected InverterCommunicationsClient(ILogger<InverterCommunicationsClient> logger, InverterClientOptions options)
        {
            this._logger = logger;
            this._options = options;
        }

        public static InverterCommunicationsClient Create(ILogger<InverterCommunicationsClient> logger, InverterClientOptions options)
        {
            // This is a factory method ... create the right instance here.
            switch (options.PortType)
            {
                case PortDeviceType.Serial:
                    return new InverterSerialCommunicationsClient(logger, options);
                case PortDeviceType.Hidraw:
                    return new InverterHidrawCommunicationsClient(logger, options);
                case PortDeviceType.USB:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private TimeSpan MaxPollingRate => TimeSpan.FromMilliseconds(this._options.MaxPollingRateMs);

        public virtual bool IsOpen
        {
            get;
            protected set;
        }

        public abstract void Open(CancellationToken cancellationToken = default);
        public abstract void Close(CancellationToken cancellationToken = default);

        protected abstract Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

        protected abstract ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

        protected abstract Task FlushAsync(CancellationToken cancellationToken = default);

        protected abstract void OnDispose(bool disposing);

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.OnDispose(disposing);
                }

                _disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed(string callerName)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(callerName);
            }
        }


        public virtual async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> SendRequest(ReadOnlyMemory<byte> request, bool replyExpected = true, int receiveTimeout = 750, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed(GetType().FullName);

            // First thing we have to do is acquire a lock on the instance so only one thread can control the communications at a time.
            using (SemaphoreSlim semiphoreLock = new SemaphoreSlim(1))
            {
                if (await semiphoreLock.WaitAsync(5000, cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return (false, ReadOnlyMemory<byte>.Empty);
                    }

                    // Before we can poll we need to make sure the minimum polling interval has elapsed. Because Task.Delay may not wait the EXACT value, we loop it until its 0 or less.
                    while (_lastPoll.ElapsedMilliseconds < MaxPollingRate.TotalMilliseconds)
                    {
                        var waitTime = MaxPollingRate.TotalMilliseconds - _lastPoll.ElapsedMilliseconds;
                        if (waitTime < 0)
                        {
                            break;
                        }
                        //_logger.LogWarning("Delaying for correct polling rate: {waitTime}", waitTime);
                        Task.Delay(TimeSpan.FromMilliseconds(waitTime > 0 ? waitTime : 0), cancellationToken).Wait();
                    }

                    // Discard any data in the buffers from any previous read/write
                    await this.FlushAsync(cancellationToken);
                    try
                    {
                        // The underlying system is a HID device. The structure of this is designed so that specifc side pages are 
                        // sent and received. In this case it is 9 bytes.
                        var index = 0;
                        do
                        {
                            var packet = request.Slice(index, (request.Length - index) > 9 ? 9 : (request.Length - index));

                            // Send the packet and make sure to flush the buffers to the data is sent completely
                            await WriteAsync(packet, cancellationToken);
                            await FlushAsync(cancellationToken);

                            // Update the starting index for the next slice.
                            index += packet.Length;
                        } while (index < request.Length);

                        //await _deviceStream.WriteAsync(request, cancellationToken);
                        if (replyExpected == false)
                        {
                            return (true, ReadOnlyMemory<byte>.Empty);
                        }

                        return await ReceivePacket(receiveTimeout: receiveTimeout, cancellationToken);
                    }
                    catch (Exception)
                    {
                        // TODO: We need to setup the correct error codes
                    }
                    finally
                    {
                        _lastPoll.Restart();
                    }
                } else
                {
                    // Timeout waiting for semiphore lock
                    return (false, ReadOnlyMemory<byte>.Empty);
                }
            }

            return (false, ReadOnlyMemory<byte>.Empty);
        }

        protected virtual async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> ReceivePacket(int receiveTimeout = 500, CancellationToken cancellationToken = default)
        {
            try
            {
                var packetSize = 8;
                var buffer = new byte[packetSize * 32];

                int bytesRead = 0;
                do
                {
                    // Continue trying to read bytes until we timeout (or nothing is left to read), or the termination character (0x0D) appears
                    try
                    {
                        // HACK: Because the FileStream provides no way to "cancel" the request, we use a special cancellation logic to actualy
                        //       cancel the operation. This does however still leave the FileStream in a "Read" state that cant be stopped. We
                        //       use the exception thrown to close and reopen the stream.  Is this just a LINUX thing?
                        //var b = await _deviceStream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, cancellationToken).WithCancellation(timeout: receiveTimeout, cancellationToken);
                        var b = await ReadAsync(buffer, bytesRead, packetSize, cancellationToken).WithCancellation(timeout: receiveTimeout, cancellationToken);
                        if (b == 0)
                        {
                            break;
                        }

                        bytesRead += b;
                    }
                    catch (OperationCanceledException)
                    {
                        // This is a BIG hack ... cancelling a read does not work and leaves the device locked.
                        this.Close();
                        this.Open();

                        // This seems to clear out the issue of the stream getting locked. We should get back the original request.
                        var request = new byte[] { 0x00, 0x0D }; // <null><cr>
                        await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);

                        bytesRead = 0;
                        break;
                    }
                    catch (TimeoutException)
                    {
                        // This is a BIG hack ... cancelling a read does not work and leaves the device locked.
                        this.Close();
                        this.Open();

                        // This seems to clear out the issue of the stream getting locked. We should get back the original request.
                        var request = new byte[] { 0x00, 0x0D }; // <null><cr>
                        return await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);

                        //bytesRead = 0;
                        //break;
                    }
                } while (buffer.Any(x => x == 0x0D) == false);

                var response = buffer
                    .Take(bytesRead)
                    .TakeWhile(x => x != 0x0D)
                    .ToArray()
                    .AsMemory();

                // Validate the CRC
                if ((response.Length > 2) && ValidateCrc(response.Slice(0, response.Length - 2), response.Slice(response.Length - 2)))
                {
                    // Return the validated data (NO CRC)
                    return (true, response.Slice(0, response.Length - 2));
                }

                // Return the entire payload for inspection
                return (false, response);
            }
            catch (TimeoutException)
            {
            }
            catch (Exception)
            {
            }

            return (false, ReadOnlyMemory<byte>.Empty);
        }

        public abstract ReadOnlyMemory<byte> GenerateGetRequest(string commandCode, bool includeCrc = true);

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

        private static bool ValidateCrc(ReadOnlyMemory<byte> message, ReadOnlyMemory<byte> payloadCrc)
        {
            return true;
        }
    }
}
