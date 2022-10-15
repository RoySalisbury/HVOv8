using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HVO.Hardware.RoofControllerV3
{
    public class RoofController : IRoofController, IDisposable
    {
        // Just makes it easier to understand whats happening.
        private const bool RelayOn = true;   // PinValue.High; 
        private const bool RelayOff = false; // PinValue.Low

        private readonly ILogger<RoofController> _logger;
        private readonly RoofControllerOptions _roofControllerOptions;
        private readonly object _syncLock = new object();
        private bool _disposed;

        public RoofController(ILogger<RoofController> logger, IOptions<RoofControllerOptions> roofControllerOptions)
        {
            this._logger = logger;
            this._roofControllerOptions = roofControllerOptions.Value;
        }

        public void Initialize(CancellationToken cancellationToken) 
        {
            if (this.IsInitialized)
            {
                this.Status = RoofControllerStatus.NotInitialized;
                throw new Exception("Already Initialized");
            }

            // Setup the cancellation token registration so we know when things are shutting down as soon as possible and can call STOP.
            cancellationToken.Register(() => this.Stop());

            this.IsInitialized = true;
            this.Status = RoofControllerStatus.Unknown;
        }

        private void Shutdown() 
        {
            if (this.IsInitialized)
            {
                this.IsInitialized = false;
                this.Status = RoofControllerStatus.NotInitialized;
            }
        }

        public bool IsInitialized { get; private set; } = false;

        public RoofControllerStatus Status { get; private set; } = RoofControllerStatus.NotInitialized;

        public void Stop() 
        {
            lock (this._syncLock)
            {

            }
        }

        public void Open() 
        {
            lock (this._syncLock)
            {

            }
        }

        public void Close() 
        {
            lock (this._syncLock)
            {

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.Shutdown();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RoofController()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}