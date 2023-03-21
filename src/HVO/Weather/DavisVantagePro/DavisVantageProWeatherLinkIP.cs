using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HVO.Threading.Tasks;
using System.Linq;

namespace HVO.Weather.DavisVantagePro
{
    public sealed class DavisVantageProWeatherLinkIP : IDisposable
    {
        private bool _disposed = false;
        private IPEndPoint _ipEndPoint = null;
        private object _syncLock = new object();

        private ManualResetEvent _monitorStopRequestResetEvent = new ManualResetEvent(false);
        private ManualResetEvent _monitorStoppedResetEvent = new ManualResetEvent(false);

        public event EventHandler<DavisVantageProConsoleRecordReceivedEventArgs> OnConsoleRecordReceived;

        public DavisVantageProWeatherLinkIP(IPAddress ipAddress, int port = 22222)
        {
            this._ipEndPoint = new IPEndPoint(ipAddress, port);
        }

        ~DavisVantageProWeatherLinkIP()
        {
            this.Dispose(false);
        }

        private CancellationToken _cancellationToken = new CancellationToken();

        public void StartMonitor()
        {
            var task = Task.Factory.StartNew(() =>
            {
                try
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        tcpClient.Connect(this._ipEndPoint.Address, this._ipEndPoint.Port);

                        var networkStream = tcpClient.GetStream();
                        networkStream.ReadTimeout = 5000;
                        networkStream.WriteTimeout = 5000;

                        using (StreamReader streamReader = new StreamReader(networkStream))
                        using (StreamWriter streamWriter = new StreamWriter(networkStream) { AutoFlush = true })
                        {
                            if (_SendConsoleWakeupCommand(streamWriter, streamReader))
                            {
                                if (_GetConsoleDataRecord(networkStream, streamWriter, streamReader, out var latestConsoleRecord))
                                {
                                    this.OnConsoleRecordReceived?.Invoke(this, new DavisVantageProConsoleRecordReceivedEventArgs(latestConsoleRecord.Key, latestConsoleRecord.Value));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }, this._cancellationToken, TaskCreationOptions.None, TaskScheduler.Default);
            task.ContinueWith(t => 
            {
                Console.WriteLine("Restarting TcpClient after failure");
                Thread.Sleep(10000);
                this.StartMonitor();
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        public async Task StartMonitorAsync(CancellationToken stoppingToken)
        {
            using (var tcpClient = new TcpClient() { ReceiveTimeout = 1500, SendTimeout = 1500 })
            {
                await tcpClient.ConnectAsync(this._ipEndPoint.Address, this._ipEndPoint.Port);
                var networkStream = tcpClient.GetStream();

                using (var streamReader = new StreamReader(networkStream))
                using (var streamWriter = new StreamWriter(networkStream) { AutoFlush = true })
                {
                    while (stoppingToken.IsCancellationRequested == false)
                    {
                        // We need to send the console wakeup command as the very first thing.  If not, then we can't get other data.
                        if (await _SendConsoleWakeupCommandAsync(streamReader, streamWriter, retryAttempts: 3))
                        {
                            // Now that we are awake, we should send a command to tell the console to send the packets every 2 seconds.
                            while (stoppingToken.IsCancellationRequested == false)
                            {
                                Action<DateTimeOffset, byte[]> callback = (recordDateTime, data) =>
                                {
                                    this.OnConsoleRecordReceived(this, new DavisVantageProConsoleRecordReceivedEventArgs(recordDateTime, data));
                                };

                                if (await _GetConsoleDataRecordAsync(streamReader, streamWriter, callback, stoppingToken) == false)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task<bool> _SendConsoleWakeupCommandAsync(StreamReader streamReader, StreamWriter streamWriter, int retryAttempts = 3)
        {
            var i = 0;
            while (retryAttempts > i++)
            {
                await streamWriter.WriteAsync('\n');
                var result = await streamReader.ReadLineAsync();

                if (result != null)
                {
                    streamReader.DiscardBufferedData();
                    return true;
                }

                await Task.Delay(1200);
            }
            return false;
        } 

        private static async Task<bool> _GetConsoleDataRecordAsync(StreamReader streamReader, StreamWriter streamWriter, Action<DateTimeOffset, byte[]> action, CancellationToken cancellationToken, byte packetsRequested = byte.MaxValue )
        {
            // We request x LOOP packets. This will give us a record every 2 seconds.
            await streamWriter.WriteAsync($"LOOP {packetsRequested}\n");

            // We should first get a response of an ACK or NAK from the LOOP command.  
            int ackRetryCount = 0;
            do
            {
                var byteRead = 0;
                try
                {
                    byteRead = streamReader.BaseStream.ReadByte();
                }
                catch (IOException ex) when ((ex.InnerException is SocketException socketException) && (socketException.SocketErrorCode == SocketError.TimedOut))
                {
                    Console.WriteLine($"Socket Timeout Exception during 'ReadByte' after sending the LOOP command. Retry Count: {ackRetryCount}");
                    continue;
                }

                if (byteRead == 0x06)
                {
                    // We could use a hint to know when we have read the total packets that the LOOP command would give us. 
                    // Timeouts will also come into play, but this gives us a hint.
                    for (int i = 0; i < packetsRequested; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }

                        var commandResponse = new byte[99];
                        int bytesRead = 0;
                        int retryCount = 0;

                        // Read data until we have a complete response. This should all be a single response, but network conditions could slow it down.
                        while ((bytesRead < commandResponse.Length) && (cancellationToken.IsCancellationRequested == false))
                        {
                            try
                            {
                                bytesRead += streamReader.BaseStream.Read(commandResponse, bytesRead, commandResponse.Length - bytesRead);
                            }
                            catch (IOException ex) when ((ex.InnerException is SocketException socketException) && (socketException.SocketErrorCode == SocketError.TimedOut))
                            {
                                // Retry ....
                                if (retryCount++ > 3)
                                {
                                    // Not enough data was read.  We could be at the end of the LOOP packets for this round.
                                    return false;
                                }
                            }
                        }

                        if ((i == 0) && (packetsRequested > 1))
                        {
                            // The first record MIGHT be an old record.  So lets throw this one away.
                            continue;
                        }

                        if (DavisVantageProConsoleRecord.ValidatePacktCrc(commandResponse))
                        {
                            var r = DavisVantageProConsoleRecord.Create(commandResponse, DateTimeOffset.Now, false);
                            action?.Invoke(r.RecordDateTime, r.RawDataRecord);
                        }
                        else
                        {
                            Console.WriteLine("BAD CRC for weather data record");
                        }
                    }

                    return true;
                }

                if (byteRead == 0x21)
                {
                    // NAK
                    Console.WriteLine("Received a NAK for the LOOP command.");
                    return false;
                }

                Console.WriteLine($"Bad ACK: {byteRead}");
            } while (ackRetryCount++ < 3);

            return false;
        }


        #region WeatherLink Commands

        private bool _SendConsoleWakeupCommand(TcpClient tcpClient)
        {
            lock (this._syncLock)
            {
                int retryCount = 0;
                int maxRetries = 3;

                try
                {
                    var networkStream = tcpClient.GetStream();

                    // Write the wake up command (a NewLine character '\n' )
                    networkStream.WriteByte(10);

                    // Wait for the command to be processed
                    Thread.Sleep(500);

                    while (!networkStream.DataAvailable && retryCount < maxRetries)
                    {
                        networkStream.WriteByte(10);
                        Thread.Sleep(500);

                        retryCount += 1;
                    }

                    if (retryCount < maxRetries)
                    {
                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        //private DateTime _GetConsoleDateTime(StreamReader streamReader, StreamWriter streamWriter)
        //{
        //  lock (this._syncLock)
        //  {
        //    // Always start every command with a wakeup signal to the console.  This makes sure that we are ready to send the command. It will check that
        //    // the instance is still valid (not disposed) and that the serial port is currently open and available.
        //    this._SendConsoleWakeupCommand(streamReader, streamWriter);

        //    // This record has a CRC value, so we will validate it and if it is not correct, retry the operation up to 3 times.
        //    int retryCount = 0;
        //    do
        //    {
        //      this._serialPort.Write(string.Format("GETTIME{0}", '\n'));

        //      // Read the data sent in the response. The first character is the ACK.
        //      this._serialPort.ReadTo(string.Format("{0}", (char)0x06));

        //      // Now that we have the ACK, get the data packet.  This is 99 bytes of data.
        //      byte[] commandResponse = new byte[8];
        //      int readCount = this._serialPort.Read(commandResponse, 0, commandResponse.Length);
        //      if (readCount != commandResponse.Length)
        //      {
        //        throw new Exception("Invalid/Unexpected data recieved from console.");
        //      }

        //      using (HualapaiValleyObservatory.Security.Cryptography.Crc16 crc16 = new HualapaiValleyObservatory.Security.Cryptography.Crc16())
        //      {
        //        ushort calculatedCrcValue = BitConverter.ToUInt16(crc16.ComputeHash(commandResponse, 0, 6), 0);
        //        ushort originalCrcValue = BitConverter.ToUInt16(commandResponse, 6);

        //        if (calculatedCrcValue == originalCrcValue)
        //        {
        //          return new DateTime(commandResponse[5], commandResponse[4], commandResponse[3], commandResponse[2], commandResponse[1], commandResponse[0]);
        //        }
        //        else if (++retryCount < 3)
        //        {
        //          continue;
        //        }

        //        throw new Exception(string.Format("CRC calculation failed. Data packet corrupt after 3 download attempts. [Original: {0}  -  Calculated: {1}", originalCrcValue, calculatedCrcValue));
        //      }
        //    } while (true);
        //  }
        //}

        #endregion

        #region IDisposable Members
        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {

            }
            this._disposed = true;
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        private static bool _SendConsoleWakeupCommand(StreamWriter streamWriter, StreamReader streamReader)
        {
            try
            {
                // Send the wakeup command to the console
                //networkStream.WriteByte(10);
                streamWriter.Write("\n");

                // Read the response.
                string response = streamReader.ReadLine();
                return true;
            }
            catch (Exception)
            {
                //Console.WriteLine("_SendConsoleWakeupCommand - Error: {0}", ex.Message);
                return false;
            }
        }

        private static bool _GetConsoleDataRecord(NetworkStream networkStream, StreamWriter streamwriter, StreamReader streamReader, out KeyValuePair<DateTimeOffset, byte[]> latestConsoleRecord)
        {
            // This record has a CRC value, so we will validate it and if it is not correct, retry the operation up to 3 times.
            int retryCount = 0;
            {
                do
                {
                    try
                    {
                        // Send the command to get the current data record.  We signal that we only want ONE record.
                        //string commandData = string.Format("LOOP 1{0}", '\n');
                        //networkStream.Write(ASCIIEncoding.ASCII.GetBytes(commandData), 0, commandData.Length);
                        streamwriter.Write(string.Format("LOOP 1{0}", '\n'));

                        // Read the data sent in the response. The first character is the ACK.
                        bool ackReceived = false;
                        int ackRetry = 0;
                        while (!ackReceived && (ackRetry < 3))
                        {
                            ackReceived = (networkStream.ReadByte() == 6);
                            if (ackReceived)
                            {
                                byte[] commandResponse = new byte[99];

                                int currentReadCount, totalReadCount = 0;
                                while (totalReadCount < commandResponse.Length)
                                {
                                    currentReadCount = networkStream.Read(commandResponse, totalReadCount, commandResponse.Length - totalReadCount);
                                    totalReadCount += currentReadCount;

                                    if (currentReadCount != commandResponse.Length)
                                    {
                                        Console.WriteLine("_GetLatestConsoleRecord - Not Enough Data - Reading More");
                                        continue;
                                    }

                                    using (var crc16 = new Security.Cryptography.Crc16())
                                    {
                                        ushort calculatedCrcValue = BitConverter.ToUInt16(crc16.ComputeHash(commandResponse, 0, 97), 0);
                                        ushort originalCrcValue = BitConverter.ToUInt16(commandResponse, 97);

                                        if (calculatedCrcValue == originalCrcValue)
                                        {
                                            latestConsoleRecord = new KeyValuePair<DateTimeOffset, byte[]>(DateTimeOffset.Now, commandResponse);
                                            return true;
                                        }
                                        else if (++retryCount < 3)
                                        {
                                            Console.WriteLine("_GetLatestConsoleRecord - CRC Error - Retry #{0}", retryCount);
                                            continue;
                                        }

                                        //throw new Exception(string.Format("CRC calculation failed. Data packet corrupt after 3 download attempts. [Original: {0}  -  Calculated: {1}", originalCrcValue, calculatedCrcValue));
                                        latestConsoleRecord = new KeyValuePair<DateTimeOffset, byte[]>();
                                        return false;
                                    }
                                }
                                break;
                            }

                            ackRetry += 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("_GetConsoleDataRecord - Error: {0}", ex.Message);
                        latestConsoleRecord = new KeyValuePair<DateTimeOffset, byte[]>();
                        return false;
                    }
                } while (true);
            }
        }

    }
}
