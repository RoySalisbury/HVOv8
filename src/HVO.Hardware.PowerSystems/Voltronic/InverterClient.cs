using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using HVO.Hardware.PowerSystems;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public class InverterClient : IDisposable
    {
        private readonly ILogger<IInverterCommunications> _logger;
        private readonly InverterClientOptions _options;

        private readonly Stopwatch _lastPoll = Stopwatch.StartNew();
        private IInverterCommunications _deviceStream;

        public InverterClient() 
        {
            this._options = new InverterClientOptions();
            this._deviceStream = new HiDrawStream(null, this._options);
        }

        public InverterClient(ILogger<IInverterCommunications> logger, IOptions<InverterClientOptions> options)
        {
            this._logger = logger;
            this._options = options.Value;
            this._deviceStream = new HiDrawStream(this._logger, this._options);
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

            this._deviceStream.Open(); 
        }

        public void Close()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            _deviceStream?.Close();
        }

        public async Task<bool> Test()
        {

            var request = GenerateGetRequest("DAT2212232206", includeCrc: false);
            Console.WriteLine($"Request: {BitConverterExtras.BytesToHexString(request.ToArray())}");

            var response = await SendRequest(request, replyExpected: true);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: DAT\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
            }

            await this.QT();

            return false;
        }


        public async Task<(bool IsSuccess, string ProtocolID)> QPI(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QPI");
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QPI\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");

                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> QID(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QID");
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QID\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");

                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string SerialNumber)> QSID(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QSID");
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QSID\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");

                // The first 2 bytes are the lenght of the serial number
                var s1 = System.Text.Encoding.ASCII.GetString(response.Data.Span);
                return (true, s1);
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QVFW"); // 00 51 56 46 57 62 99 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QVFW\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW2(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QVFW2"); //00 51 56 46 57 32 C3 F5 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QVFW2\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> QVFW3(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QVFW3"); // 00 51 56 46 57 33 D3 D4 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QVFW3\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");

                return (true, Version.Parse("0.0").ToString());
            }

            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, string Version)> VERFW(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("VERFW"); // 00 56 45 52 46 57 B7 B8 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: VERFW\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");

                return (true, Version.Parse("0.0").ToString());
            } else
            {
                Console.WriteLine($"FAIL: VERFW");
            }

            await Task.Yield();
            return (false, string.Empty);
        }

        public async Task<(bool IsSuccess, object Model)> QPIRI(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QPIRI"); // 00 51 50 49 52 49 F8 54 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QPIRI\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QFLAG(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QFLAG"); // 00 51 46 4C 41 47 98 74 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QFLAG\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPIGS(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QPIGS"); // 00 51 50 49 47 53 B7 A9 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QPIGS\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMOD(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QMOD"); // 00 51 4D 4F 44 49 C1 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QMOD\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPIWS(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QPIWS"); // 00 51 50 49 57 53 B4 DA 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QPIWS\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QDI(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QDI"); // 00 51 44 49 71 1B 0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QDI\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMCHGCR(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QMCHGCR"); // 0x00, 0x51, 0x4D, 0x43, 0x48, 0x47, 0x43, 0x52, 0xD8, 0x55, 0x0D
            //var request = new byte[] { 0x51, 0x4D, 0x43, 0x48, 0x47, 0x43, 0x52, 0xD8, 0x55, 0x0D };
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QMCHGCR\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMUCHGCR(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QMUCHGCR"); // 0x00, 0x51, 0x4D, 0x55, 0x43, 0x48, 0x47, 0x43, 0x52, 0x26, 0x34, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QMUCHGCR\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QOPPT(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QOPPT"); // 00, 0x51, 0x4F, 0x50, 0x50, 0x54, 0x4F, 0x11, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QOPPT\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QCHPT(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QCHPT"); // 00, 0x51, 0x43, 0x48, 0x50, 0x54, 0xEA, 0xE1, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QCHPT\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QT(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QT"); // 00, 0x51, 0x54, 0x27, 0xFF, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QT\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMN(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QMN"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QMN\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QGMN(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QGMN"); // 00, 0x51, 0x47, 0x4D, 0x4E, 0x49, 0x29, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QGMN\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBEQI(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QBEQI"); // 00, 0x51, 0x42, 0x45, 0x51, 0x49, 0x2E, 0xA9, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QBEQI\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QET(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QET"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QET\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEY(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QEY{date.Value.Year:0000}";
            var request = GenerateGetRequest(command);

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: {command}\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEM(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QEM{date.Value.Year:0000}{date.Value.Month:00}";
            var request = GenerateGetRequest(command);

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: {command}\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QED(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QED{date.Value.Year:0000}{date.Value.Month:00}{date.Value.Day:00}";
            var request = GenerateGetRequest(command);

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: {command}\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLT(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QLT"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QLT\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLY(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QLY{date.Value.Year:0000}";
            var request = GenerateGetRequest(command);

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: {command}\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLM(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QLM{date.Value.Year:0000}{date.Value.Month:00}";
            var request = GenerateGetRequest(command);

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: {command}\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLD(DateTime? date = null, CancellationToken cancellationToken = default)
        {
            date ??= DateTime.Now;

            var command = $"QLD{date.Value.Year:0000}{date.Value.Month:00}{date.Value.Day:00}";
            var request = GenerateGetRequest(command);

            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: {command}\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLED(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QLED"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QLED\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> Q1(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("Q1"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: Q1\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBOOT(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QBOOT"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QBOOT\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QOPM(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QOPM"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QOPM\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPGS(byte index = 0, CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest($"QPGS{index}"); 
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QPGS{index}\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QBV(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QBV"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QBV\t\tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }






        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> SendRequest(ReadOnlyMemory<byte> request, bool replyExpected = true, int receiveTimeout = 750, CancellationToken cancellationToken = default)
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
                await this._deviceStream.FlushAsync(cancellationToken);
                try
                {
                    // The underlying system is a HID device. The structure of this is designed so that specifc side pages are 
                    // sent and received. In this case it is 9 bytes.
                    var index = 0;
                    do
                    {
                        var packet = request.Slice(index, (request.Length - index) > 9 ? 9 : (request.Length - index));

                        // Send the packet and make sure to flush the buffers to the data is sent completely
                        await _deviceStream.WriteAsync(packet, cancellationToken);
                        await _deviceStream.FlushAsync(cancellationToken);

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
                        var b = await _deviceStream.ReadAsync(buffer, bytesRead, packetSize, cancellationToken).WithCancellation(timeout: receiveTimeout, cancellationToken);
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

        private ReadOnlyMemory<byte> GenerateGetRequest(string commandCode, bool includeCrc = true)
        {
            return this._deviceStream.GenerateGetRequest(commandCode, includeCrc);
        }

        private static bool ValidateCrc(ReadOnlyMemory<byte> message, ReadOnlyMemory<byte> payloadCrc)
        {
            return true;
        }
    }



}