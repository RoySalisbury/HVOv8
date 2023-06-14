using System.Buffers.Binary;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

namespace HVO.Test.BMS
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using (BMS.JbdBmsSocketClient _client = new BMS.JbdBmsSocketClient())
            {
                _client.Open(new IPEndPoint(IPAddress.Parse("192.168.0.8"), 4002));

                //Console.BufferHeight = 50;
                //Console.BufferWidth = 120;

                Console.Clear();
                Console.SetCursorPosition(0, 0);

                var result = await _client.GetBmsInformation();

                Console.ReadLine();


                //var count = 10;
                //while (count-- != 0)
                //{
                //    Console.SetCursorPosition(0, 0);

                //    if (_client.SendRequest<GetBasicJbdBmsInfoResponse>(new GetBasicJbdBmsInfoRequest(), out var bmsInfo))
                //    {
                //        Console.WriteLine($"CommandCode: {bmsInfo.CommandCode}, Response: {BitConverter.ToString(bmsInfo.Payload.ToArray())}");
                //        Console.WriteLine($"     Total Voltage   : {bmsInfo.TotalVoltage} V");
                //        Console.WriteLine($"     Total Current   : {bmsInfo.Current} A");
                //        Console.WriteLine($"     Total Watts     : {bmsInfo.TotalVoltage * bmsInfo.Current} W");

                //        Console.WriteLine($"     Capacity        : {bmsInfo.ResidualCapacity} of {bmsInfo.NominalCapacity} mAh   --  {bmsInfo.PercentageOfResidualCapacity}%");
                //        Console.WriteLine($"     CycleLife       : {bmsInfo.CycleLife}");
                //        //Console.WriteLine($"     # of Cells      : {bmsInfo.CellBlockNumber}");

                //        for (int i = 0; i < bmsInfo.TemperatureSensors.Length; i++)
                //        {
                //            Console.WriteLine($"     Temp #{i + 1}   : {bmsInfo.TemperatureSensors[i]}");
                //        }
                //        Console.WriteLine();
                //    }

                //    //if (_client.SendRequest<GetCellInfoResponse>(new GetCellInfoRequest(), out var cellInfo))
                //    //{
                //    //    Console.WriteLine($"CommandCode: {cellInfo.CommandCode}");
                //    //    for (int i = 0; i < cellInfo.CellVoltage.Length; i++)
                //    //    {
                //    //        Console.WriteLine($"     Cell #{i + 1}: {cellInfo.CellVoltage[i]}");
                //    //    }
                //    //    Console.WriteLine();
                //    //}

                //    //if (_client.SendRequest<GetBoardInfoResponse>(new GetBoardInfoRequest(), out var boardInfo))
                //    //{
                //    //    Console.WriteLine($"CommandCode: {boardInfo.CommandCode}, Response: {boardInfo.Name}");
                //    //    Console.WriteLine();
                //    //}

                //    Console.WriteLine("=================================================================");

                //    System.Threading.Thread.Sleep(2000);
                //}

            }
        }
    }

    public class JbdBmsSocketClient : IDisposable
    {
        private readonly TcpClient _tcpClient = new TcpClient();
        private readonly Stopwatch _lastPoll = Stopwatch.StartNew();

        private bool _disposed;

        public JbdBmsSocketClient()
        {
        }

        private TimeSpan MaxPollingRate => TimeSpan.FromMilliseconds(250);

        public bool IsOpen { get; private set; }

        public void Open(IPEndPoint endpoint) 
        {
            // Make sure we are not disposed of 
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                this._tcpClient.Connect(endpoint);
                this.IsOpen = true;
            }
        }

        public void Close() 
        {
            // Make sure we are not disposed of 
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (this.IsOpen == true)
            {
                this._tcpClient.Close();
                this.IsOpen = false;
            }
        }

        public async Task<(bool IsSuccess, dynamic Model)> GetBmsInformation(CancellationToken cancellationToken = default)
        {
            var request = GenerateRequest(0xA5, 0x03, payload: ReadOnlySpan<byte>.Empty);
            var response = await this.SendRequestAsync(request, cancellationToken: cancellationToken);
            if (response.IsSuccess)
            {
                var validation = ValidateResponse(0x03, response.Data.Span);
                if (validation.ResultCode == 0)
                {
                    // We do this so we can convert the ReadOnlyMemory<T> to a ReadOnlySpan<T> just once and not on each call.
                    // This is basically just like creating a private method, but we keep it local to THIS method.
                    Func<ReadOnlyMemory<byte>, dynamic> CreateModel = (data) =>
                    {
                        var payload = data.Span;
                        var model = new
                        {
                            TotalVoltage = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(0))) / 100.0,
                            Current = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(payload.Slice(2))) * 0.01,

                            ResidualCapacity = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(4))) * 10,
                            NominalCapacity = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(6))) * 10,

                            CycleLife = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(8))),
                            ProductDate = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(10))),

                            BalanceStatus = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(12))),
                            BalanceStatusHigh = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(14))),

                            ProtectionStatus = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(16))),

                            Version = payload[18],
                            PercentageOfResidualCapacity = payload[19],
                            FETControlStatus = payload[20],
                            CellBlockNumber = payload[21],
                            TemperatureSensors = new double[payload[22]],
                        };

                        for (int i = 0; i < model.TemperatureSensors.Length; i++)
                        {
                            model.TemperatureSensors[i] = (BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(23 + (i * 2)))) - 2731) / 10.0;
                        }

                        return model;
                    };

                    return (true, CreateModel(validation.Payload));

                }
            }

            return (false, null);
        }

        private static ReadOnlyMemory<byte> GenerateRequest(byte statusByte, byte commandCode, ReadOnlySpan<byte> payload, bool includeCrc = true)
        {
            // Generate the request
            List<byte> request = new List<byte>() { 0xDD, statusByte, commandCode, (byte)payload.Length };
            request.AddRange(payload.ToArray());
            if (includeCrc)
            {
                // Get the CRC for this payload
                var crc = CalculateCrc(payload, seedValue: commandCode);
                request.AddRange(crc);
            }

            request.Add(0x77);
            return request.ToArray();
        }

        private static byte[] CalculateCrc(ReadOnlySpan<byte> data, byte seedValue = 0)
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

        private static bool ValidateCrc(ReadOnlySpan<byte> data, ReadOnlySpan<byte> originalCrc, byte seedValue = 0)
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

        private static (int ResultCode, ReadOnlyMemory<byte> Payload) ValidateResponse(byte commandCode, ReadOnlySpan<byte> response)
        {
            // All responses should start with 0xDD
            if ((response.Length > 0) && (response[0] != 0xDD))
            {
                return (-1, null);
            }

            // The response should have the same request command code
            if ((response.Length > 1) && (response[1] != commandCode))
            {
                return (-2, null);
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
                    return (-3, null);
                }

                return (0, payload.ToArray());
            }

            return (-4, null);
        }


        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> SendRequestAsync(ReadOnlyMemory<byte> request, int receiveTimeout = 500, CancellationToken cancellationToken = default)
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

                var networkStream = this._tcpClient.GetStream();
                try
                {
                    await networkStream.FlushAsync(cancellationToken);

                    var index = 0;
                    do
                    {
                        var packet = request.Slice(index, (request.Length - index));

                        // Send the packet and make sure to flush the buffers to the data is sent completely
                        await networkStream.WriteAsync(packet, cancellationToken);
                        await networkStream.FlushAsync(cancellationToken);

                        // Update the starting index for the next slice.
                        index += packet.Length;
                    } while (index < request.Length);

                    return await ReceiveResponseAsync(receiveTimeout: receiveTimeout, cancellationToken);
                }
                catch (Exception)
                {
                }
                finally
                {
                    _lastPoll.Restart();
                }
            }

            return (false, ReadOnlyMemory<byte>.Empty);
        }

        private async Task<(bool IsSuccess, ReadOnlyMemory<byte> Data)> ReceiveResponseAsync(int receiveTimeout = 500, CancellationToken cancellationToken = default)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            try
            {
                // This is the maximum packet size that we would ever expect.
                var buffer = new byte[1024];
                int bytesRead = 0;

                var networkStream = this._tcpClient.GetStream();
                do
                {
                    try
                    {
                        var b = await networkStream.ReadAsync(buffer, bytesRead, (buffer.Length - bytesRead), cancellationToken).WithCancellation(timeout: receiveTimeout, cancellationToken);
                        if (b == 0)
                        {
                            break;
                        }
                        bytesRead += b;
                    }
                    catch (TimeoutException)
                    {
                        // Just in case we timeout too soon. Try again.
                        if (networkStream.DataAvailable)
                        {
                            continue;
                        }
                        return (false, ReadOnlyMemory<byte>.Empty);
                        //break;
                    }
                    catch (OperationCanceledException)
                    {
                        return (false, ReadOnlyMemory<byte>.Empty);
                    }

                } while (buffer.Any(x => x == 0x77) == false); // The packet termination character is 0x77 ... Once we get here, there is no more data to read.

                return (true, new ReadOnlyMemory<byte>(buffer, 0, bytesRead));
            }
            catch
            {
                return (false, ReadOnlyMemory<byte>.Empty);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this._tcpClient.Dispose();
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
    }





    public abstract class JbdBmsPacket
    {
        protected JbdBmsPacket(byte statusBit, byte commandCode)
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

    public abstract class JbdBmsPacketRequest : JbdBmsPacket
    {
        protected JbdBmsPacketRequest(byte commandCode) : base(0xA5, commandCode)
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

    public sealed class GetBasicJbdBmsInfoRequest : JbdBmsPacketRequest
    {
        public GetBasicJbdBmsInfoRequest() : base(0x03)
        {
        }
    }


    public abstract class JbdBmsPacketResponse : JbdBmsPacket
    {
        protected JbdBmsPacketResponse(byte command) : base(0, command)
        {
        }

        protected virtual void InitializeFromPayload(Span<byte> payload)
        {
            this.Payload = payload.ToArray();
        }

        public ReadOnlyMemory<byte> Payload { get; private set; }

        public static JbdBmsPacketResponse CreateInstance(JbdBmsPacketRequest request, Span<byte> response)
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
                JbdBmsPacketResponse result = null;

                switch (request)
                {
                    case GetBasicJbdBmsInfoRequest r:
                        result = new GetBasicJbdBmsInfoResponse();
                        break;
                    case GetCellInfoRequest r:
                        result = new GetCellInfoResponse();
                        break;
                    case GetBoardInfoRequest r:
                        result = new GetBoardInfoResponse();
                        break;
                    default:
                        break;
                }

                // Initialize the instance form the payload data
                result?.InitializeFromPayload(payload);
                return result;
            }



            return null;

            // DD  A5  00  1B  04  00  00  






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


    public sealed class GetBasicJbdBmsInfoResponse : JbdBmsPacketResponse
    {
        public GetBasicJbdBmsInfoResponse() : base(0x03)
        {
        }

        protected override void InitializeFromPayload(Span<byte> payload)
        {
            base.InitializeFromPayload(payload);

            if ((payload == null) || (payload.Length < 2))
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            this.TotalVoltage = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(0))) / 100.0;
            this.Current = BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(payload.Slice(2))) * 0.01;

            this.ResidualCapacity = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(4))) * 10;
            this.NominalCapacity = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(6))) * 10;

            this.CycleLife = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(8)));
            this.ProductDate = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(10)));

            this.BalanceStatus = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(12)));
            this.BalanceStatusHigh = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(14)));

            this.ProtectionStatus = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(16)));

            this.Version = payload[18];
            this.PercentageOfResidualCapacity = payload[19];
            this.FETControlStatus = payload[20];
            this.CellBlockNumber = payload[21];

            this.TemperatureSensors = new double[payload[22]];
            for (int i = 0; i < this.TemperatureSensors.Length; i++)
            {
                this.TemperatureSensors[i] = (BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(23 + (i * 2)))) - 2731) / 10.0;
            }
        }


        public double TotalVoltage { get; set; }
        public double Current { get; set; }
        public int ResidualCapacity { get; set; }
        public int NominalCapacity { get; set; }
        public ushort CycleLife { get; set; }
        public ushort ProductDate { get; set; }
        public ushort BalanceStatus { get; set; }
        public ushort BalanceStatusHigh { get; set; }
        public ushort ProtectionStatus { get; set; }
        public byte Version { get; set; }
        public byte PercentageOfResidualCapacity { get; set; }
        public byte FETControlStatus { get; set; }
        public byte CellBlockNumber { get; set; }

        public double[] TemperatureSensors { get; set; }
    }

    public sealed class GetCellInfoRequest : JbdBmsPacketRequest
    {
        public GetCellInfoRequest() : base(0x04)
        {
        }


    }

    public sealed class GetCellInfoResponse : JbdBmsPacketResponse
    {
        public GetCellInfoResponse() : base(0x04)
        {
        }

        protected override void InitializeFromPayload(Span<byte> payload)
        {
            base.InitializeFromPayload(payload);

            if ((payload == null) || (payload.Length < 2))
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            this.CellVoltage = new double[payload.Length / 2];
            for (int i = 0; i < CellVoltage.Length; i++)
            {
                this.CellVoltage[i] = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt16(payload.Slice(i * 2))) / 1000.0;
            }
        }


        public double[] CellVoltage { get; set; }
    }


    public sealed class GetBoardInfoRequest : JbdBmsPacketRequest
    {
        public GetBoardInfoRequest() : base(0x05)
        {
        }


    }

    public sealed class GetBoardInfoResponse : JbdBmsPacketResponse
    {
        public GetBoardInfoResponse() : base(0x05)
        {
        }

        protected override void InitializeFromPayload(Span<byte> payload)
        {
            base.InitializeFromPayload(payload);

            if ((payload == null) || (payload.Length < 2))
            {
                throw new ArgumentOutOfRangeException(nameof(payload));
            }

            this.Name = ASCIIEncoding.ASCII.GetString(payload);
        }

        public string Name { get; private set; }
    }


    public sealed class JDBClient : IDisposable
    {
        private SerialPort _serialPort;
        private readonly object _syncLock = new object();
        private readonly System.Diagnostics.Stopwatch _lastPoll = System.Diagnostics.Stopwatch.StartNew();

        public JDBClient()
        {
            this._serialPort = new SerialPort()
            {
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                ReadTimeout = 1000,
                WriteTimeout = 1000
            };
        }

        public bool IsOpen
        {
            get
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }

                return _serialPort.IsOpen;
            }
        }

        public string PortName { get; private set; }

        public void Open(string portName)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this._serialPort.IsOpen == false)
            {
                this._serialPort.PortName = portName;
                this._serialPort.Open();

                this.ClearReadWriteBuffers();
            }
        }

        public void Close()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }

            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }


        public void ClearReadWriteBuffers()
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            // Discard anything in the output buffer that has yet be be sent
            this._serialPort.DiscardOutBuffer();

            // Read all existing data in the buffer .. Clear it all out.
            this._serialPort.ReadExisting();
            this._serialPort.DiscardInBuffer();
        }


        public bool SendRequest(JbdBmsPacketRequest request, out JbdBmsPacketResponse response, Func<JbdBmsPacketResponse, byte, bool> responseValidation = null, byte retryCount = 3, CancellationToken cancellationToken = default)
        {
            for (byte i = 1; i <= retryCount; i++)
            {
                if (SendRequest(request.ToBytes(), out var responseBytes, cancellationToken: cancellationToken))
                {
                    try
                    {
                        response = JbdBmsPacketResponse.CreateInstance(request, responseBytes);
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

        public bool SendRequest<T>(JbdBmsPacketRequest request, out T response, byte retryCount = 3, CancellationToken cancellationToken = default)
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
        private bool SendRequest(Span<byte> request, out Span<byte> responsePacket, bool replyExpected = true, CancellationToken cancellationToken = default)
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

                // Before we can poll we need to make sure the minimum polling interval has elapsed. Because Task.Delay may not wait the EXACT value, we loop it until its 0 or less.
                //while (_lastPoll.ElapsedMilliseconds < MaxSasPollingRate.TotalMilliseconds)
                //{
                //    var waitTime = MaxSasPollingRate.TotalMilliseconds - _lastPoll.ElapsedMilliseconds;
                //    if (waitTime < 0)
                //    {
                //        break;
                //    }
                //    this._logger.LogWarning("Delaying for proper polling rate: {waitTime}", waitTime);
                //    Task.Delay(TimeSpan.FromMilliseconds(waitTime > 0 ? waitTime : 0), cancellationToken).Wait();
                //}

                // Discard any data in the buffers from any previous read/write
                ClearReadWriteBuffers();
                try
                {
                    if (SendPacket(request, out var errorCode, cancellationToken: cancellationToken))
                    {
                        // We dont expect or even try to read a response for Long Poll's of type 'G'.
                        if (request[0] == 0)
                        {
                            responsePacket = null;
                            return true;
                        }

                        if (replyExpected == false)
                        {
                            responsePacket = Array.Empty<byte>();
                            return true;
                        }

                        //if (ReceivePacket(out var initialResponsePacket, out errorCode, _options.MaxFirstByteReadDelay, _options.MaxInterByteReadDelay, cancellationToken: cancellationToken))
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
                this._serialPort.Write(buffer, 0, buffer.Length);

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
                byte headerLength = 4;
                var buffer = new byte[256];

                // Normally, the first packet may take a bit of extra time to response. So our read timeout is just for that 
                // first character.
                //this._serialPort.ReadTimeout = (int)(firstByteReadTimeout);
                int bytesRead = this._serialPort.Read(buffer, 0, headerLength);

                while (bytesRead < headerLength)
                {
                    try
                    {
                        var b = this._serialPort.Read(buffer, bytesRead, (headerLength - bytesRead));
                        if (b == 0)
                        {
                            break;
                        }
                        bytesRead += b;
                    }
                    catch (TimeoutException) { break; }
                }


                // Did we get the header?
                if (bytesRead == headerLength)
                {
                    // How many data bytes in the packet
                    var dataLength = System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(buffer, 2));

                    // How many TOTAL bytes are in this packet?
                    var packetLength = headerLength + dataLength + 3;

                    while (bytesRead < packetLength) // Don't overflow our buffer...
                    {
                        // Continue trying to read bytes until we timeout (or nothing is left to read)
                        try
                        {
                            var b = this._serialPort.Read(buffer, bytesRead, (packetLength - bytesRead));
                            if (b == 0)
                            {
                                break;
                            }

                            bytesRead += b;
                        }
                        catch (TimeoutException) { break; }
                    }

                    response = new Span<byte>(buffer, 0, bytesRead);
                    return true;
                }
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
                    _serialPort?.Dispose();
                    _serialPort = null;
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


    public sealed class JDBSocketClient : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;

        private readonly object _syncLock = new object();
        private readonly System.Diagnostics.Stopwatch _lastPoll = System.Diagnostics.Stopwatch.StartNew();

        public JDBSocketClient()
        {
            this._tcpClient = new TcpClient();
        }

        public bool IsOpen
        {
            get; private set;
        }

        public string PortName { get; private set; } = "192.168.0.117:4002";

        public void Open(string portName)
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }

            if (this.IsOpen == false)
            {
                this._tcpClient.Connect("192.168.0.117", 4002);
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


        public bool SendRequest(JbdBmsPacketRequest request, out JbdBmsPacketResponse response, Func<JbdBmsPacketResponse, byte, bool> responseValidation = null, byte retryCount = 3, CancellationToken cancellationToken = default)
        {
            for (byte i = 1; i <= retryCount; i++)
            {
                if (SendRequest(request.ToBytes(), out var responseBytes, cancellationToken: cancellationToken))
                {
                    try
                    {
                        response = JbdBmsPacketResponse.CreateInstance(request, responseBytes);
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

        public bool SendRequest<T>(JbdBmsPacketRequest request, out T response, byte retryCount = 3, CancellationToken cancellationToken = default)
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
        private bool SendRequest(Span<byte> request, out Span<byte> responsePacket, bool replyExpected = true, CancellationToken cancellationToken = default)
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
                var buffer = new byte[1024];
                int bytesRead = 0;

                while ((bytesRead < buffer.Length) && (this._networkStream.DataAvailable))
                {
                    try
                    {
                        this._networkStream.ReadTimeout = 500;
                        var b = this._networkStream.Read(buffer, bytesRead, (buffer.Length - bytesRead));
                        if (b == 0)
                        {
                            break;
                        }
                        bytesRead += b;
                    }
                    catch (TimeoutException) { break; }
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






}