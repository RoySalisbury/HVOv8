using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public sealed class InverterCommunicationsClient : IDisposable
    {
        private readonly ILogger<InverterCommunicationsClient> _logger;
        private readonly InverterCommunicationsClientOptions _options;

        private readonly System.Diagnostics.Stopwatch _lastPoll = System.Diagnostics.Stopwatch.StartNew();
        private FileStream _deviceStream;

        public InverterCommunicationsClient()
        {
            this._options = new InverterCommunicationsClientOptions();
        }

        public InverterCommunicationsClient(ILogger<InverterCommunicationsClient> logger, IOptions<InverterCommunicationsClientOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        private TimeSpan MaxPollingRate => TimeSpan.FromMilliseconds(_options.MaxPollingRateMs);

        public void Open()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_deviceStream == null)
            {
                //_deviceStream = new FileStream(_options.PortPath, FileMode.Open, FileAccess.Read | FileAccess.Write, FileShare.ReadWrite, 4096, true);
                _deviceStream = File.Open(_options.PortPath, FileMode.Open);
            }
        }

        public void Close()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_deviceStream != null)
            {
                _deviceStream.Close();
                _deviceStream = null;
            }
        }

        public async Task<(bool IsSuccess, T ResponseMessage)> SendRequest<T>(InverterRequest request, byte retryCount = 1, CancellationToken cancellationToken = default)
        {
            var sendResponse = await SendRequest(request, (x, i) => x is T, retryCount, cancellationToken);
            if (sendResponse.IsSuccess)
            {
                var responseMessage = (T)Convert.ChangeType(sendResponse.ResponseMessage, typeof(T), CultureInfo.CurrentCulture);
                return (true, responseMessage);
            }

            return (false, default);
        }

        public async Task<(bool IsSuccess, InverterResponse ResponseMessage)> SendRequest(InverterRequest request, Func<InverterResponse, byte, bool> responseValidation = null, byte retryCount = 1, CancellationToken cancellationToken = default)
        {
            for (byte retryNumber = 1; retryNumber <= retryCount; retryNumber++)
            {
                var sendResponse = await SendRequest(request.ToBytes(), cancellationToken: cancellationToken);
                if (sendResponse.IsSuccess)
                {
                    try
                    {
                        var responseMessage = InverterResponse.CreateInstance(request, sendResponse.Data.Span);
                        if (responseValidation == null && responseMessage == null)
                        {
                            // This usually means that the CreateInstance factory needs updated with this response type
                            // _logger.LogWarning("Retry - Unknown Response: {retryNumber}  -  Request: {requestBytes}   -   Response: {responseBytes}",
                            //     retryNumber, BitConverter.ToString(request.ToBytes()), BitConverterExtras.BytesToHexString(sendResponse.Data.ToArray()));

                            continue;
                        }

                        // Do we have a valid reponse (optional)
                        if (responseValidation != null && responseValidation(responseMessage, retryNumber) == false)
                        {
                            // _logger.LogWarning("Retry - Response validation failed: {retryNumber}  -  Request: {requestBytes}   -   Response: {responseBytes}",
                            //     retryNumber, BitConverter.ToString(request.ToBytes()), BitConverterExtras.BytesToHexString(sendResponse.Data.ToArray()));

                            continue; // Next retry
                        }

                        // Accept the response
                        return (true, responseMessage);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Usually this is due to a packet size mismatch (bad read).
                        continue;
                    }
                }

//                _logger.LogWarning("Retry - No Response: {retryNumber} - {request}", retryNumber, request.GetType());
            }

            return (false, null);
        }

        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> SendRequest(ReadOnlyMemory<byte> request, bool replyExpected = true, CancellationToken cancellationToken = default)
        {
            // Make sure we are not disposed of 
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            // First thing we have to do is acquire a lock on the instance so only one thread can control the communications at a time.
            using (SemaphoreSlim semiphoreLock = new SemaphoreSlim(1))
            {
                await semiphoreLock.WaitAsync(cancellationToken);
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
                ClearReadWriteBuffers(cancellationToken);
                try
                {
                    if (await SendPacket(request, cancellationToken))
                    {
                        if (replyExpected == false)
                        {
                            return (true, ReadOnlyMemory<byte>.Empty);
                        }

                        return await ReceivePacket(cancellationToken);
                    }
                }
                catch (Exception)
                {
                    // TODO: We need to setup the correct error codes
                }
                finally
                {
                    _lastPoll.Restart();
                }
            }

            return (false, ReadOnlyMemory<byte>.Empty);
        }

        private async Task<bool> SendPacket(ReadOnlyMemory<byte> request, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            try
            {
                await _deviceStream.WriteAsync(request, cancellationToken);
                return true;
            }
            catch (TimeoutException)
            {
            }
            catch (Exception)
            {
            }

            return false;
        }

        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> ReceivePacket(CancellationToken cancellationToken = default)
        {
            try
            {
                var buffer = new byte[1024];

                int bytesRead = 0;
                do
                {
                    // Continue trying to read bytes until we timeout (or nothing is left to read), or the termination character (0x0D) appears
                    // BUG: ... what if the 0X0D is valid WITHIN the packet and not just at the end
                    try
                    {
                        // Being an HID request "behind the sceens", this is actually done in blocks of 8 bytes at a time.
                        var b = await _deviceStream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, cancellationToken);
                        if (b == 0)
                        {
                            break;
                        }

                        bytesRead += b;
                    }
                    catch (TimeoutException) 
                    { 
                        break; 
                    }
                } while (buffer.Any(x => x == 0x0D) == false);

                // BUG: If the buffer contains a valid 0x0D before the termination, then this breaks.
                var response = buffer
                    .Take(bytesRead)
                    .TakeWhile(x => x != 0x0D)
                    .ToArray()
                    .AsMemory();

                return (true, response);
            }
            catch (TimeoutException)
            {
            }
            catch (Exception)
            {
            }

            return (false, ReadOnlyMemory<byte>.Empty);
        }

        private async void ClearReadWriteBuffers(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            // Discard anything in the input and output buffer that has yet be be sent
            await _deviceStream.FlushAsync(cancellationToken);
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _deviceStream?.Dispose();
                    _deviceStream = null;
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}