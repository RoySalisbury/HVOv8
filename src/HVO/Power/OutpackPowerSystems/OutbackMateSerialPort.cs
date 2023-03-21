using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.IO;

namespace HVO.Power.OutbackPowerSystems {
  public sealed class OutbackMateSerialPort : IDisposable {
    private bool _disposed = false;
    private object _syncLock = new object();

    public event EventHandler<OutbackMateRecordReceivedEventArgs> OnRecordReceived;
    public event EventHandler<OutbackMateCommunicationsErrorEventArgs> OnCommunicationsError;

    private string _portName = "COM1";
    private SerialPort _serialPort = null;
    private ManualResetEvent _serialPortStopReadingResetEvent = new ManualResetEvent(false);
    private ManualResetEvent _serialPortReadingStoppedResetEvent = new ManualResetEvent(false);
    private Thread _serailPortReadThread = null;

    public OutbackMateSerialPort(string portName) {
      this._portName = portName;
    }

    ~OutbackMateSerialPort() {
      this.Dispose(false);
    }

    #region IDisposable Members

    private void Dispose(bool disposing) {
      lock (this._syncLock) {
        if (!this._disposed) {
          // Dispose of managed resources.
          if (this._serialPort != null) {
            try
            {
              this._serialPort.Dispose();
            }
            finally
            {
              this._serialPort = null;
            }
          }

          this._disposed = true;
        }
      }
    }

    void IDisposable.Dispose() {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    public bool IsOpen {
      get {
        lock (this._syncLock) {
          if (this._disposed) {
            throw new ObjectDisposedException(this.GetType().Name);
          }

          return ((this._serialPort != null) && (this._serialPort.IsOpen));
        }
      }
    }

    public void Open() {
      if (!this.IsOpen) {
        this._serialPort = new SerialPort(this._portName, 19200, Parity.None, 8, StopBits.One);

        this._serialPort.DtrEnable = true;
        this._serialPort.RtsEnable = false;

        this._serialPort.Open();

        this._serailPortReadThread = new Thread(delegate() {
          this._serialPortStopReadingResetEvent.Reset();
          this._serialPortReadingStoppedResetEvent.Reset();

          using (ManualResetEvent communicationsErrorResetEvent = new ManualResetEvent(false)) {
            while (true) {
              try {
                string rawRecord = this._serialPort.ReadLine();
                if (!string.IsNullOrWhiteSpace(rawRecord)) {
                  EventHandler<OutbackMateRecordReceivedEventArgs> localOnRecordReceived = this.OnRecordReceived;
                  if (localOnRecordReceived != null) {
                    localOnRecordReceived.BeginInvoke(this, new OutbackMateRecordReceivedEventArgs(DateTimeOffset.Now, rawRecord), delegate(IAsyncResult asyncResult) {
                      localOnRecordReceived.EndInvoke(asyncResult);
                    }, null);
                  }
                }
              }
              catch (IOException ex) {
                // Usually caused becuase the port was closed out from under the read.  We will just exit the loop now
                //Console.WriteLine("COM ERROR (IO): {0}", ex.Message);

                EventHandler<OutbackMateCommunicationsErrorEventArgs> localOnCommunicationsError = this.OnCommunicationsError;
                if (localOnCommunicationsError != null) {
                  localOnCommunicationsError(this, new OutbackMateCommunicationsErrorEventArgs(ex));
                }
                this._serialPortStopReadingResetEvent.Set();
              }
              catch (TimeoutException) {
                Console.WriteLine("COM READ ERROR: TIMEOUT");
              }
              catch (Exception ex) {
                //Console.WriteLine("COM ERROR: {0}", ex.ToString());
                EventHandler<OutbackMateCommunicationsErrorEventArgs> localOnCommunicationsError = this.OnCommunicationsError;
                if (localOnCommunicationsError != null) {
                  localOnCommunicationsError(this, new OutbackMateCommunicationsErrorEventArgs(ex));
                }
                communicationsErrorResetEvent.Set();
              }

              switch (WaitHandle.WaitAny(new WaitHandle[] { communicationsErrorResetEvent, _serialPortStopReadingResetEvent }, 0)) {
                case WaitHandle.WaitTimeout: {
                  break;
                }
                case 0: { // communicationsErrorResetEvent
                  this._serialPortReadingStoppedResetEvent.Set();
                  try
                  {
                    this._serialPort.Dispose();
                  }
                  finally
                  {
                    this._serialPort = null;
                  }
                  return;
                }
                case 1: { // _serialPortStopReadingResetEvent
                  this._serialPortReadingStoppedResetEvent.Set();
                  return;
                }
              }
            }
          }
        });
        this._serailPortReadThread.Name = "SerialPortReadThread";
        this._serailPortReadThread.IsBackground = true;
        this._serailPortReadThread.Start();
      }
    
    }

    public void Close() {
      if (this.IsOpen) {

        // Signal the thread to stop and wait for it to do so. Only wait 2 seconds before killing the thread
        if (!WaitHandle.SignalAndWait(this._serialPortStopReadingResetEvent, this._serialPortReadingStoppedResetEvent, 2000, false)) {

          // The thread did not exit in the time alloted.. Try to kill it.
          this._serailPortReadThread.Abort();

          // Wait for the shutdown after the abort
          if (!this._serialPortReadingStoppedResetEvent.WaitOne(2000)) {
            // Wow .. still did not shot down.. this is one stuborn thread..
          }
        }

        this._serialPort.Close();
        this._serialPort = null;
      }
    }
  }
}
