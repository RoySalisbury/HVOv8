using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace HVO.Hardware.PowerSystems.Voltronic
{
    public class InverterClient : IDisposable
    {
        private static readonly ushort[] CrcTable = { 0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50a5, 0x60c6, 0x70e7, 0x8108, 0x9129, 0xa14a, 0xb16b, 0xc18c, 0xd1ad, 0xe1ce, 0xf1ef };
        private readonly ILogger<InverterClient> _logger;
        private readonly InverterClientOptions _options;

        private readonly Stopwatch _lastPoll = Stopwatch.StartNew();
        private Stream _deviceStream;

        public InverterClient() 
        {
            this._options = new InverterClientOptions();
        }

        public InverterClient(ILogger<InverterClient> logger, IOptions<InverterClientOptions> options)
        {
            this._logger = logger;
            this._options = options.Value;
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
                _deviceStream = new FileStream(_options.PortPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
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


        public async Task<(bool IsSuccess, string ProtocolID)> QPI(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QPI");
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QPI \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

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
                Console.WriteLine($"Request: QID \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

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
                Console.WriteLine($"Request: QSID \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

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
                Console.WriteLine($"Request: QVFW \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

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
                Console.WriteLine($"Request: QVFW2 \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

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
                Console.WriteLine($"Request: QVFW3 \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

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
                Console.WriteLine($"Request: VERFW \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");

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
                Console.WriteLine($"Request: QPIRI \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QFLAG \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QPIGS \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QMOD \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QPIWS \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QDI \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QMCHGCR(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QMCHGCR"); // 0x00, 0x51, 0x4D, 0x43, 0x48, 0x47, 0x43, 0x52, 0xD8, 0x55, 0x0D
            var response = await SendRequest(request, replyExpected: true, receiveTimeout: 2000, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QMCHGCR \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QMUCHGCR \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QOPPT \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QCHPT \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QT \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QMN \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QGMN \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QBEQI \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QET \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEY(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QEY"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QEY \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QEM(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QEM"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QEM \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QED(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QED"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QED \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QLT \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLY(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QLY"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QLY \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLM(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QLM"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QLM \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QLD(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QLD"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QLD \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QLED \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: Q1 \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QBOOT \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QOPM \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
                return (true, null);
            }

            return (false, null);
        }

        public async Task<(bool IsSuccess, object Model)> QPGS(CancellationToken cancellationToken = default)
        {
            var request = GenerateGetRequest("QPGS"); // 00, 0x51, 0x4D, 0x4E, 0xBB, 0x64, 0x0D
            var response = await SendRequest(request, replyExpected: true, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                Console.WriteLine($"Request: QPGS \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                Console.WriteLine($"Request: QBV \tReply: {System.Text.Encoding.ASCII.GetString(response.Data.ToArray())}\t   -   HEX: {BitConverterExtras.BytesToHexString(response.Data.ToArray())}");
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
                ClearReadWriteBuffers(cancellationToken);
                try
                {
                    await _deviceStream.WriteAsync(request, cancellationToken);
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
                        var b = await _deviceStream.ReadAsync(buffer, bytesRead, buffer.Length - bytesRead, cancellationToken).WithCancellation(timeout: receiveTimeout, cancellationToken);
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

        private async void ClearReadWriteBuffers(CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            // Discard anything in the input and output buffer that has yet be be sent
            await _deviceStream.FlushAsync(cancellationToken);
        }



        private static ReadOnlyMemory<byte> GenerateGetRequest(string commandCode)
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

            //Console.WriteLine($"Command: {commandCode}, Data: {BitConverterExtras.BytesToHexString(request.ToArray())}");
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