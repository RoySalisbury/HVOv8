using System.Diagnostics;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public class InverterClient : IDisposable
    {
        private static readonly ushort[] CrcTable = { 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7, 0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef };
        private readonly InverterCommunicationsClientOptions _options;
        private readonly Stopwatch _lastPoll = Stopwatch.StartNew();

        private Stream _deviceStream;

        public InverterClient() 
        {
            this._options = new InverterCommunicationsClientOptions();
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

        private TimeSpan MaxPollingRate => TimeSpan.FromMilliseconds(_options.MaxPollingRateMs);

        public void Open()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_deviceStream == null)
            {
                _deviceStream = new FileStream(_options.PortPath, FileMode.Open, FileAccess.Read | FileAccess.Write, FileShare.ReadWrite, 4096, true);
                //_deviceStream = File.Open(_options.PortPath, FileMode.Open);
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


        public async Task<(bool IsSuccess, string ProtocolID)> GetDeviceProtocolID(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QPI");
            var request = new byte[] { 0x00, 0x51, 0x50, 0x49, 0xBE, 0xAC, 0x0D }; // <null>QPI<crc><cr>
            
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));

                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> GetDeviceSerialNumber(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QID");
            var request = new byte[] { 0x00, 0x51, 0x49, 0x44, 0xD6, 0xEA, 0x0D }; // <null>QID<crc><cr>
            
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));

                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> GetDeviceSerialNumberEx(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QSID");
            var request = new byte[] { 0x00, 0x51, 0x53, 0x49, 0x44, 0xBB, 0x05, 0x0D }; // <null>QSID<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));

                // The first 2 bytes are the lenght of the serial number
                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> GetMainCPUFirmwareVersion(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QVFW"); // 00 51 56 46 57 62 99 0D
            var request = new byte[] { 0x00, 0x51, 0x56, 0x46, 0x57, 0x62, 0x99, 0x0D }; // <null>QVFW<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Reply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> GetAnotherCPUFirmwareVersion(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QVFW2"); //00 51 56 46 57 32 C3 F5 0D
            var request = new byte[] { 0x00, 0x51, 0x56, 0x46, 0x57, 0x32, 0xC3, 0xF5, 0x0D }; // <null>QVFW2<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Reply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> GetRemotePanelCPUFirmwareVersion(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QVFW3"); // 00 51 56 46 57 33 D3 D4 0D
            var request = new byte[] { 0x00, 0x51, 0x56, 0x46, 0x57, 0x33, 0xD3, 0xD4, 0x0D }; // <null>QVFW3<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Reply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> GetBLECPUFirmwareVersion(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("VERFW"); // 00 56 45 52 46 57 3A F8 25 0D
            var request = new byte[] { 0x00, 0x56, 0x45, 0x52, 0x46, 0x57, 0x3A, 0xF8, 0x25, 0x0D }; // <null>VERFW<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
               Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));
                Console.WriteLine(System.Text.Encoding.ASCII.GetString(response.Data.ToArray()));

                return (true, Version.Parse("0.0").ToString());
            }

            await Task.Yield();
            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, object Model)> GetDeviceRatingInformation(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QPIRI"); // 00 51 50 49 52 49 F8 54 0D
            var request = new byte[] { 0x00, 0x51, 0x50, 0x49, 0x52, 0x49, 0xF8, 0x54, 0x0D }; // <null>QPIRI<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> GetDeviceFlagStatus(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QFLAG"); // 00 51 46 4C 41 47 98 74 0D
            var request = new byte[] { 0x00, 0x51, 0x46, 0x4C, 0x41, 0x47, 0x98, 0x74, 0x0D }; // <null>QFLAG<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> GetDeviceGeneralStatusParameters(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QPIGS"); // 00 51 50 49 47 53 B7 A9 0D
            var request = new byte[] { 0x00, 0x51, 0x50, 0x49, 0x47, 0x53, 0xB7, 0xA9, 0x0D }; // <null>QPIGS<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> GetDeviceMode(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QMOD"); // 00 51 4D 4F 44 49 C1 0D
            var request = new byte[] { 0x00, 0x51, 0x4D, 0x4F, 0x44, 0x49, 0xC1, 0x0D }; // <null>QMOD<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> GetDeviceWarningStatus(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QPIWS"); // 00 51 50 49 57 53 B4 DA 0D
            var request = new byte[] { 0x00, 0x51, 0x50, 0x49, 0x57, 0x53, 0xB4, 0xDA, 0x0D }; // <null>QPIWS<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> GetDefaultSettingInformation(CancellationToken cancellationToken = default)
        {
            //var request = GenerateStaticPayloadRequest("QDI"); // 00 51 44 49 71 1B 0D
            var request = new byte[] { 0x00, 0x51, 0x44, 0x49, 0x71, 0x1B, 0x0D }; // <null>QDI<crc><cr>

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine(BitConverterExtras.BytesToHexString(response.Data.ToArray()));
                return (true, null);
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
                    await _deviceStream.WriteAsync(request, cancellationToken);
                    if (replyExpected == false)
                    {
                        return (true, ReadOnlyMemory<byte>.Empty);
                    }

                    return await ReceivePacket(cancellationToken);
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

        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> ReceivePacket(CancellationToken cancellationToken = default)
        {
            try
            {
                var buffer = new byte[1024];

                int bytesRead = 0;
                do
                {
                    // Continue trying to read bytes until we timeout (or nothing is left to read), or the termination character (0x0D) appears
                    try
                    {
                        // NOTE: Being an HID request "behind the sceens", this is actually done in blocks of 8 bytes at a time.
                        // HACK: Because the FileStream provides no way to "cancel" the request, we use a special cancellation logic to actualy
                        //       cancel the operation. This does however still leave the FileStream in a "Read" state that cant be stopped. We
                        //       use the exception thrown to close and reopen the stream.  Is this just a LINUX thing?
                        var b = await _deviceStream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, cancellationToken).WithCancellation(timeout: 750, cancellationToken);
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

                        bytesRead = 0;
                        break;
                    }
                    catch (TimeoutException)
                    {
                        // This is a BIG hack ... cancelling a read does not work and leaves the device locked.
                        this.Close();
                        this.Open();

                        bytesRead = 0;
                        break;
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

        private async void ClearReadWriteBuffers(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            // Discard anything in the input and output buffer that has yet be be sent
            await _deviceStream.FlushAsync(cancellationToken);
        }



        private static ReadOnlyMemory<byte> GenerateStaticPayloadRequest(string commandCode)
        {
            // Get the command bytes
            var commandBytes = System.Text.Encoding.ASCII.GetBytes(commandCode);

            // Get the CRC for this payload
            var crc = CalculateCrc(commandBytes, 0);

            // Generate the request
            List<byte> request = new List<byte>() { 0 };
            request.AddRange(commandBytes);
            request.AddRange(crc);
            request.Add(0x0D);

            Console.WriteLine($"Command: {commandCode}, Data: {BitConverterExtras.BytesToHexString(request.ToArray())}");
            return request.ToArray();
        }

        private static byte[] CalculateCrc(Span<byte> data, ushort seed = 0)
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