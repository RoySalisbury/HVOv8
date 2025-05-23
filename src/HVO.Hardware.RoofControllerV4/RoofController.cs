using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Device.Gpio;
using System.Runtime.CompilerServices;

namespace HVO.Hardware.RoofControllerV4
{
    public class RoofController : IRoofController, IDisposable
    {
        private RoofControllerStatus _status = RoofControllerStatus.NotInitialized;

        // Just makes it easier to understand whats happening.
        private PinValue RelayOn = PinValue.High;
        private PinValue RelayOff = PinValue.Low;

        private readonly ILogger<RoofController> _logger;
        private readonly RoofControllerOptions _options;

        private readonly object _syncLock = new object();
        private bool _disposed;

        public RoofController(ILogger<RoofController> logger, IOptions<RoofControllerOptions> options)
        {
            this._logger = logger;
            this._options = options.Value;
        }

        public bool IsInitialized { get; set; } = false;

        public RoofControllerStatus Status { 
            get { return this._status; }
            private set 
            {
                if (this._status == value)
                {
                    return;
                }

                this._status = value;
                this.OnPropertyChanged();
            }
        }

        public async Task<bool> Initialize(CancellationToken cancellationToken)
        {
            if (this.IsInitialized)
            {
                return true;
            }

            this._logger.LogInformation("Initializing GPIO.");
            await Task.Delay(5000, cancellationToken);

            this.IsInitialized = true;
            return true;
        }

        public void Close()
        {
            this.Status = RoofControllerStatus.Closed;
        }

        public void Open()
        {
            this.Status = RoofControllerStatus.Open;
        }

        public void Stop()
        {
            this.Status = RoofControllerStatus.Stopped;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = default)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (this.IsInitialized)
                        {
                        }
                    }
                    catch { }
                    finally
                    {
                    }
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
}
