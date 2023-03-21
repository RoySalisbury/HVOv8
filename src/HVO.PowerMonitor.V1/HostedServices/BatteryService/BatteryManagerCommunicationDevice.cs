namespace HVO.PowerMonitor.V1.HostedServices.BatteryService
{
    public abstract class BatteryManagerCommunicationDevice : IDisposable
    {
        private bool _disposed;

        protected BatteryManagerCommunicationDevice() { }

        public bool IsOpen { get; protected set; }
        private TimeSpan MaxPollingRate => TimeSpan.FromMilliseconds(250);

        public abstract void Open();

        public abstract void Close();

        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> SendRequest(ReadOnlyMemory<byte> request, bool replyExpected = true, int receiveTimeout = 750, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed(this.GetType().Name);

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
                await this._clientDevice.FlushAsync(cancellationToken);
                try
                {
                    // The underlying system is a HID device. The structure of this is designed so that specifc side pages are 
                    // sent and received. In this case it is 9 bytes.
                    var index = 0;
                    do
                    {
                        var packet = request.Slice(index, (request.Length - index) > 9 ? 9 : (request.Length - index));

                        // Send the packet and make sure to flush the buffers to the data is sent completely
                        await _clientDevice.WriteAsync(packet, cancellationToken);
                        await _clientDevice.FlushAsync(cancellationToken);

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
            }

            return (false, ReadOnlyMemory<byte>.Empty);
        }

        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> ReceivePacket(int receiveTimeout = 500, CancellationToken cancellationToken = default)
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
                        var b = await _clientDevice.ReadAsync(buffer, bytesRead, packetSize, cancellationToken).WithCancellation(timeout: receiveTimeout, cancellationToken);
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









        public abstract Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken = default);

        public abstract ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

        public abstract Task FlushAsync(CancellationToken cancellationToken = default);

        protected abstract void OnDispose();

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.OnDispose();
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
    }


}
