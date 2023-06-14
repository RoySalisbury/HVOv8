using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public sealed class JKBmsSocketClient : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;

        private readonly object _syncLock = new object();
        private readonly System.Diagnostics.Stopwatch _lastPoll = System.Diagnostics.Stopwatch.StartNew();

        public JKBmsSocketClient()
        {
            this._tcpClient = new TcpClient();
        }

        public bool IsOpen
        {
            get; private set;
        }

        public string PortName { get; private set; } = "192.168.0.8:28001";

        public void Open(string portName)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                this._tcpClient.Connect("192.168.0.8", 28001);
                this._networkStream = this._tcpClient.GetStream();

                this.IsOpen = true;
            }
        }

        public void Close()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen)
            {
                this._tcpClient.Close();
                this._networkStream.Dispose();

                this.IsOpen = false;
            }
        }


        public void ClearReadWriteBuffers()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.IsOpen)
            {
                // Discard anything in the output buffer that has yet be be sent
                //this._streamWriter.Flush();
                //this._streamReader.Flush();
            }
        }


        public bool SendRequest(JKBmsPacketRequest request, out JKBmsPacketResponse response, Func<JKBmsPacketResponse, byte, bool> responseValidation = null, byte retryCount = 3, CancellationToken cancellationToken = default)
        {
            for (byte i = 1; i <= retryCount; i++)
            {
                if (SendRequest(request.ToBytes(), out var responseBytes, cancellationToken: cancellationToken))
                {
                    try
                    {
                        response = JKBmsPacketResponse.CreateInstance(request, responseBytes);
                        if ((responseValidation == null) && (response == null))
                        {
                            // This usually means that the SasPacketResponse.CreateInstance factory needs updated with this response type
                            //_logger.LogWarning("Retry - Unknown Response: {retryNumber}  -  Request: {requestBytes}   -   Response: {responseBytes}",
                            //    i, BitConverter.ToString(request.ToBytes()), BitConverterExtras.BytesToHexString(responseBytes));

                            continue;
                        }


                        // Do we have a valid reponse (optional)
                        if ((responseValidation != null) && (responseValidation(response, i) == false))
                        {
                            //_logger.LogWarning("Retry - Response validation failed: {retryNumber}  -  Request: {requestBytes}   -   Response: {responseBytes}",
                            //    i, BitConverter.ToString(request.ToBytes()), BitConverterExtras.BytesToHexString(responseBytes));

                            continue; // Next retry
                        }

                        // Accept the response
                        return true;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // Usually this is due to a packet size mismatch (bad read).
                        continue;
                    }
                }

                //_logger.LogWarning("Retry - No Response: {retryNumber} - {request}", i, request.GetType());
            }

            response = null;
            return false;
        }

        public bool SendRequest<T>(JKBmsPacketRequest request, out T response, byte retryCount = 3, CancellationToken cancellationToken = default)
        {
            if (SendRequest(request, out var r, (x, i) => (x is T), retryCount, cancellationToken))
            {
                response = (T)Convert.ChangeType(r, typeof(T), CultureInfo.CurrentCulture);
                return true;
            }

            response = default;
            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public bool SendRequest(Span<byte> request, out Span<byte> responsePacket, bool replyExpected = true, CancellationToken cancellationToken = default)
        {
            // Make sure we are not disposed of 
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            responsePacket = null;

            // First thing we have to do is acquire a lock on the instance so only one thread can control the communicates at a time.
            if (Monitor.TryEnter(_syncLock))
            {
                // Discard any data in the buffers from any previous read/write
                ClearReadWriteBuffers();
                try
                {
                    if (SendPacket(request, out var errorCode, cancellationToken: cancellationToken))
                    {
                        if (ReceivePacket(out var initialResponsePacket, out errorCode, 1000, 1000, cancellationToken: cancellationToken))
                        {
                            responsePacket = initialResponsePacket;
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    // TODO: We need to setup the correct error codes
                }
                finally
                {
                    _lastPoll.Restart();
                    Monitor.Exit(_syncLock);
                }
            }

            return false;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Diagnostics tracing only.")]
        private bool SendPacket(Span<byte> request, out ulong errorCode, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            //_logger.LogTrace("TX: {data}", BitConverterExtras.BytesToHexString(request));
            errorCode = 0;
            try
            {
                var buffer = request.ToArray();
                this._networkStream.Write(buffer, 0, buffer.Length);

                return true;
            }
            catch (TimeoutException)
            {
                errorCode = ulong.MaxValue;
            }
            catch (Exception)
            {
                errorCode = ulong.MaxValue;
            }

            return false;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Diagnostics tracing only.")]
        private bool ReceivePacket(out Span<byte> response, out ulong errorCode, uint firstByteReadTimeout = 1000, uint interByteReadTimeout = 1000, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            // This is the maximum packet site that we would ever expect.
            response = Span<byte>.Empty;
            errorCode = 0;

            try
            {
                var buffer = new byte[320*10];
                int bytesRead = 0;

                this._networkStream.ReadTimeout = 1000;
                while ((bytesRead < buffer.Length)/* && (this._networkStream.DataAvailable)*/)
                {
                    try
                    {
                        var b = this._networkStream.Read(buffer, bytesRead, (buffer.Length - bytesRead));
                        this._networkStream.ReadTimeout = 500;
                        if (b == 0)
                        {
                            break;
                        }
                        bytesRead += b;
                    }
                    catch (IOException ex) when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.TimedOut) 
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        break;
                    }
                    //catch (TimeoutException) { /*break;*/ }
                }

                response = new Span<byte>(buffer, 0, bytesRead);
                return true;
            }
            catch (TimeoutException)
            {
                errorCode = ulong.MaxValue;
            }
            catch (Exception)
            {
                errorCode = ulong.MaxValue;
            }

            return false;
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this._tcpClient?.Dispose();
                    this._tcpClient = null;
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

    public abstract class JKBmsPacket
    {
        protected JKBmsPacket(byte statusBit, byte commandCode)
        {
            this.StatusBit = statusBit;
            this.CommandCode = commandCode;
        }

        public byte StatusBit { get; protected set; }

        public byte CommandCode { get; protected set; }

        protected static byte[] CalculateCrc(Span<byte> data, byte seedValue = 0)
        {
            ushort crc = seedValue;
            for (int i = 0; i < data.Length; i++)
            {
                crc += data[i];
            }

            crc += (ushort)data.Length;
            crc ^= 0xFFFF;
            crc += 1;

            return BitConverter.GetBytes(System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(crc));
        }
    }

    public abstract class JKBmsPacketRequest : JKBmsPacket
    {
        protected JKBmsPacketRequest(byte commandCode) : base(0xA5, commandCode)
        {
        }

        public virtual byte[] ToBytes()
        {
            // The specific instance will be responsible for overriding this method and providing us the data as bytes.
            var payload = this.PayloadBytes();

            // The request packets want to use the command code as part of the CRC calculation.
            var crc = CalculateCrc(payload, seedValue: this.CommandCode);

            var header = new byte[] { this.StatusBit, this.CommandCode, (byte)payload.Length };

            var packetData = new byte[header.Length + payload.Length + crc.Length];

            Array.Copy(header, 0, packetData, 0, header.Length);
            Array.Copy(payload, 0, packetData, header.Length, payload.Length);
            Array.Copy(crc, 0, packetData, packetData.Length - crc.Length, crc.Length);

            var data = new byte[1 + packetData.Length + 1];
            data[0] = 0xDD;
            data[data.Length - 1] = 0x77;
            Array.Copy(packetData, 0, data, 1, packetData.Length);

            return data;
        }

        protected internal virtual byte[] PayloadBytes()
        {
            return Array.Empty<byte>();
        }
    }

    public abstract class JKBmsPacketResponse : JKBmsPacket
    {
        protected JKBmsPacketResponse(byte command) : base(0, command)
        {
        }

        protected virtual void InitializeFromPayload(Span<byte> payload)
        {
            this.Payload = payload.ToArray();
        }

        public ReadOnlyMemory<byte> Payload { get; private set; }

        public static JKBmsPacketResponse CreateInstance(JKBmsPacketRequest request, Span<byte> response)
        {
            if (request == null)
            {
                return null;
            }

            // The response must have at least the address byte and a command/exception code
            if (((response == null) || (response.Length == 0)))
            {
                return null;
            }

            // All responses should start with 0xDD
            if ((response.Length > 0) && (response[0] != 0xDD))
            {
                return null;
            }

            // The response should have the same request command code
            if ((response.Length > 1) && (response[1] != request.CommandCode))
            {
                return null;
            }

            // For anything else, the response must be at least 7 bytes long (start bit + command code + data length + CRC + stop bit)
            if ((response.Length > 6))
            {
                // The next two bytes are the length of the packet data
                var payloadDataLength = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(response.Slice(2, 2)));
                var payload = response.Slice(4, payloadDataLength);
                var originalCrc = response.Slice(4 + payload.Length, 2);

                // Validate the CRC. 
                if (ValidateCrc(payload, originalCrc) == false)
                {
                    //throw new ArgumentException("CRC validation of the repsonse does not match expected calculation.");
                    return null;
                }

                // All responses are of this type
                JKBmsPacketResponse result = null;

                //switch (request)
                //{
                //    case GetBasicJbdBmsInfoRequest r:
                //        result = new GetBasicJbdBmsInfoResponse();
                //        break;
                //    case GetCellInfoRequest r:
                //        result = new GetCellInfoResponse();
                //        break;
                //    case GetBoardInfoRequest r:
                //        result = new GetBoardInfoResponse();
                //        break;
                //    default:
                //        break;
                //}

                // Initialize the instance form the payload data
                result?.InitializeFromPayload(payload);
                return result;
            }

            return null;
        }

        private static bool ValidateCrc(Span<byte> data, Span<byte> originalCrc, byte seedValue = 0)
        {
            var calculatedCrc = CalculateCrc(data, seedValue);

            if ((calculatedCrc?.Length == 2) && (originalCrc.Length == 2))
            {
                if ((calculatedCrc[0] == originalCrc[0]) && (calculatedCrc[1] == originalCrc[1]))
                {
                    return true;
                }
            }

            return false;
        }
    }


}
