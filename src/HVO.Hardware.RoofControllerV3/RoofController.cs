using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HVO.Hardware.RoofControllerV3
{
    public class RoofController : IRoofController, IDisposable
    {
        // Just makes it easier to understand whats happening.
        private PinValue RelayOn = PinValue.High;
        private PinValue RelayOff = PinValue.Low;

        private readonly ILogger<RoofController> _logger;
        private readonly RoofControllerOptions _roofControllerOptions;
        private readonly object _syncLock = new object();
        private bool _disposed;
        private Stopwatch _closeLimitDelay = new Stopwatch();
        private Stopwatch _openLimitDelay = new Stopwatch();

        private GpioController _gpioController;

        public RoofController(ILogger<RoofController> logger, IOptions<RoofControllerOptions> roofControllerOptions)
        {
            this._logger = logger;
            this._roofControllerOptions = roofControllerOptions.Value;
            this._gpioController = new GpioController();
        }

        public void Initialize(CancellationToken cancellationToken) 
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(RoofController));
            }

            lock (this._syncLock)
            {
                if (this.IsInitialized)
                {
                    throw new Exception("Already Initialized");
                }

                // Setup the cancellation token registration so we know when things are shutting down as soon as possible and can call STOP.
                cancellationToken.Register(() => this.Stop());

                // Setup the GPIO controller
                try
                {
                    this._gpioController.OpenPin(this._roofControllerOptions.RoofOpenedLimitSwitchPin, PinMode.InputPullDown);
                    this._gpioController.RegisterCallbackForPinValueChangedEvent(this._roofControllerOptions.RoofOpenedLimitSwitchPin, PinEventTypes.Falling | PinEventTypes.Rising, RoofOpenedLimitSwitchCallback);

                    this._gpioController.OpenPin(this._roofControllerOptions.RoofClosedLimitSwitchPin, PinMode.InputPullDown);
                    this._gpioController.RegisterCallbackForPinValueChangedEvent(this._roofControllerOptions.RoofClosedLimitSwitchPin, PinEventTypes.Falling | PinEventTypes.Rising, RoofClosedLimitSwitchCallback);

                    this._gpioController.OpenPin(this._roofControllerOptions.LimitSwitch3, PinMode.InputPullDown);
                    this._gpioController.RegisterCallbackForPinValueChangedEvent(this._roofControllerOptions.LimitSwitch3, PinEventTypes.Falling | PinEventTypes.Rising, LimitSwitch3Callback);

                    this._gpioController.OpenPin(this._roofControllerOptions.OpenRoofButtonPin, PinMode.InputPullUp);
                    this._gpioController.RegisterCallbackForPinValueChangedEvent(this._roofControllerOptions.OpenRoofButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, OpenRoofButtonCallback);

                    this._gpioController.OpenPin(this._roofControllerOptions.CloseRoofButtonPin, PinMode.InputPullUp);
                    this._gpioController.RegisterCallbackForPinValueChangedEvent(this._roofControllerOptions.CloseRoofButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, CloseRoofButtonCallback);

                    this._gpioController.OpenPin(this._roofControllerOptions.StopRoofButtonPin, PinMode.InputPullUp);
                    this._gpioController.RegisterCallbackForPinValueChangedEvent(this._roofControllerOptions.StopRoofButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, StopRoofButtonCallback);

                    this._gpioController.OpenPin(this._roofControllerOptions.OpenRoofRelayPin, PinMode.Output, RelayOff);
                    this._gpioController.OpenPin(this._roofControllerOptions.StopRoofRelayPin, PinMode.Output, RelayOn);
                    this._gpioController.OpenPin(this._roofControllerOptions.CloseRoofRelayPin, PinMode.Output, RelayOff);
                    this._gpioController.OpenPin(this._roofControllerOptions.KeypadEnableRelayPin, PinMode.Output, RelayOn);
                }
                catch 
                {
                    this._gpioController.Dispose();
                    this._gpioController = new GpioController();

                    throw;
                }

                this.IsInitialized = true;

                // Always reset to a known safe state on initialization.
                this.Stop();
            }
        }

        private void StopRoofButtonCallback(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Pin #: {0}, ChangeType: {1}", e.PinNumber, e.ChangeType);

            if (this._disposed)
            {
                return;
            }

            lock (this._syncLock)
            {
                if (this.IsInitialized == false)
                {
                    return;
                }

                switch (e.ChangeType)
                {
                    case PinEventTypes.None:
                        break;
                    case PinEventTypes.Rising:
                        this._logger.LogInformation($"StopRoofButtonCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop");
                        this.Stop();
                        break;
                    case PinEventTypes.Falling:
                        this._logger.LogInformation($"StopRoofButtonCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop");
                        this.Stop();
                        break;
                    default:
                        break;
                }
            }

        }

        private void CloseRoofButtonCallback(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Pin #: {0}, ChangeType: {1}", e.PinNumber, e.ChangeType);

            if (this._disposed)
            {
                return;
            }

            lock (this._syncLock)
            {
                if (this.IsInitialized == false)
                {
                    return;
                }

                switch (e.ChangeType)
                {
                    case PinEventTypes.None:
                        break;
                    case PinEventTypes.Rising:
                        this._logger.LogInformation($"CloseRoofButtonCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop");
                        this.Stop();
                        break;
                    case PinEventTypes.Falling:
                        this._logger.LogInformation($"CloseRoofButtonCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoClose");
                        this._closeLimitDelay.Reset();
                        this.Close();
                        break;
                    default:
                        break;
                }
            }
        }

        private void OpenRoofButtonCallback(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Pin #: {0}, ChangeType: {1}", e.PinNumber, e.ChangeType);

            if (this._disposed)
            {
                return;
            }

            lock (this._syncLock)
            {
                if (this.IsInitialized == false)
                {
                    return;
                }

                switch (e.ChangeType)
                {
                    case PinEventTypes.None:
                        break;
                    case PinEventTypes.Rising:
                        this._logger.LogInformation($"OpenRoofButtonCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop");
                        this.Stop();
                        break;
                    case PinEventTypes.Falling:
                        this._logger.LogInformation($"OpenRoofButtonCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoOpen");
                        this._openLimitDelay.Reset();
                        this.Open();
                        break;
                    default:
                        break;
                }
            }
        }

        private void RoofOpenedLimitSwitchCallback(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Pin #: {0}, ChangeType: {1}", e.PinNumber, e.ChangeType);

            switch (e.ChangeType)
            {
                case PinEventTypes.None:
                    break;
                case PinEventTypes.Rising:
                    this._logger.LogInformation($"OpenLimitSwitchCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  N/A");
                    break;
                case PinEventTypes.Falling:
                    //if (this._closeLimitDelay.ElapsedMilliseconds > 1000)
                    //{
                        this._logger.LogInformation($"OpenLimitSwitchCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop");
                        this.Stop();
                    //} else
                    // {
                    //     this._logger.LogInformation($"OpenLimitSwitchCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop DELAYED  -  {this._closeLimitDelay.ElapsedMilliseconds}");
                    //}
                    break;
                default:
                    break;
            }
        }

        private void RoofClosedLimitSwitchCallback(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Pin #: {0}, ChangeType: {1}", e.PinNumber, e.ChangeType);

            switch (e.ChangeType)
            {
                case PinEventTypes.None:
                    break;
                case PinEventTypes.Rising:
                    this._logger.LogInformation($"RoofClosedLimitSwitchCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  N/A");
                    break;
                case PinEventTypes.Falling:
                    //if (this._openLimitDelay.ElapsedMilliseconds > 1000)
                    //{
                        this._logger.LogInformation($"RoofClosedLimitSwitchCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop");
                        this.Stop();
                    //}
                    //else
                    //{
                    //    this._logger.LogInformation($"RoofClosedLimitSwitchCallback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop DELAYED  -  {this._openLimitDelay.ElapsedMilliseconds}");
                    //}
                    break;
                default:
                    break;
            }
        }

        private void LimitSwitch3Callback(object sender, PinValueChangedEventArgs e)
        {
            Console.WriteLine("Pin #: {0}, ChangeType: {1}", e.PinNumber, e.ChangeType);

            switch (e.ChangeType)
            {
                case PinEventTypes.None:
                    break;
                case PinEventTypes.Rising:
                    this._logger.LogInformation($"LimitSwitch3Callback - {DateTime.Now:O}  -  {e.ChangeType}  -  N/A");
                    break;
                case PinEventTypes.Falling:
                    this._logger.LogInformation($"LimitSwitch3Callback - {DateTime.Now:O}  -  {e.ChangeType}  -  DoStop");
                    break;
                default:
                    break;
            }
        }


        public bool IsInitialized { get; private set; } = false;

        public RoofControllerStatus Status { get; private set; } = RoofControllerStatus.NotInitialized;

        public void Stop() 
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(RoofController));
            }

            this.InternalStop();
        }

        private void InternalStop()
        {
            lock (this._syncLock)
            {
                if (this.IsInitialized == false)
                {
                    throw new Exception("Device not initialized");
                }

                // What was the original status? Used to determine the last position of the roof when not open or closed.
                var originalStatus = this.Status;

                this._gpioController.Write(this._roofControllerOptions.StopRoofRelayPin, RelayOn);
                this.Status = RoofControllerStatus.Stopped;

                this._gpioController.Write(this._roofControllerOptions.OpenRoofRelayPin, RelayOff);
                this._gpioController.Write(this._roofControllerOptions.CloseRoofRelayPin, RelayOff);

                // Re-enable the keypad
                this._gpioController.Write(this._roofControllerOptions.KeypadEnableRelayPin, RelayOn);

                // Determine the state of the system
                if (this._gpioController.Read(this._roofControllerOptions.RoofClosedLimitSwitchPin) == PinValue.Low)
                {
                    this.Status = RoofControllerStatus.Closed;
                }
                else if (this._gpioController.Read(this._roofControllerOptions.RoofOpenedLimitSwitchPin) == PinValue.Low)
                {
                    this.Status = RoofControllerStatus.Open;
                }
                else
                {
                    // Neither of the limit switchs has been set, so what was the stqte before we stopped the motors?
                    if ((originalStatus == RoofControllerStatus.Closing) || (originalStatus == RoofControllerStatus.Opening))
                    {
                        this.Status = originalStatus;
                    }
                    else
                    {
                        this.Status = RoofControllerStatus.Open;
                    }
                }

                this._logger.LogInformation($"====Stop - {DateTime.Now:O}. Current Status: {this.Status}");
            }

        }

        public void Open() 
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(RoofController));
            }

            lock (this._syncLock)
            {
                if (this.IsInitialized == false)
                {
                    throw new Exception("Device not initialized");
                }

                if (this._gpioController.Read(this._roofControllerOptions.OpenRoofRelayPin) == RelayOff)
                {
                    // Stop any movement of the roof
                    this.Stop();

                    // Are we at the roof limits for this direction?
                    if (this._gpioController.Read(this._roofControllerOptions.RoofOpenedLimitSwitchPin) == PinValue.Low)
                    {
                        this.Status = RoofControllerStatus.Open;
                        return;
                    }

                    // Do a small delay here just so we know the roof has stopped
                    System.Threading.Thread.Sleep(100);

                    // Disable the keypad control while we are moving (STOP still works).
                    this._gpioController.Write(this._roofControllerOptions.KeypadEnableRelayPin, RelayOff);

                    // Turn off the stop command
                    this._gpioController.Write(this._roofControllerOptions.StopRoofRelayPin, RelayOff);

                    // Start the motor
                    this._gpioController.Write(this._roofControllerOptions.OpenRoofRelayPin, RelayOn);
                    this.Status = RoofControllerStatus.Opening;
                }

                this._logger.LogInformation($"====Open - {DateTime.Now:O}. Current Status: {this.Status}");
            }
        }

        public void Close() 
        {
            if (this._disposed)
            {
                throw new ObjectDisposedException(nameof(RoofController));
            }

            lock (this._syncLock)
            {
                if (this.IsInitialized == false)
                {
                    throw new Exception("Device not initialized");
                }

                if (this._gpioController.Read(this._roofControllerOptions.CloseRoofRelayPin) == RelayOff)
                {
                    // Stop any movement of the roof
                    this.Stop();

                    // Are we at the roof limits for this direction?
                    if (this._gpioController.Read(this._roofControllerOptions.RoofClosedLimitSwitchPin) == PinValue.Low)
                    {
                        this.Status = RoofControllerStatus.Closed;
                        return;
                    }

                    // Do a small delay here just so we know the roof has stopped
                    System.Threading.Thread.Sleep(100);

                    // Disable the keypad control while we are moving (STOP still works).
                    this._gpioController.Write(this._roofControllerOptions.KeypadEnableRelayPin, RelayOff);

                    // Turn off the stop command
                    this._gpioController.Write(this._roofControllerOptions.StopRoofRelayPin, RelayOff);

                    // Start the motor
                    this._gpioController.Write(this._roofControllerOptions.CloseRoofRelayPin, RelayOn);
                    this.Status = RoofControllerStatus.Closing;
                }

                this._logger.LogInformation($"====Close - {DateTime.Now:O}. Current Status: {this.Status}");
            }
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
                            this.InternalStop();

                            this._gpioController.UnregisterCallbackForPinValueChangedEvent(this._roofControllerOptions.RoofOpenedLimitSwitchPin, RoofOpenedLimitSwitchCallback);
                            this._gpioController.UnregisterCallbackForPinValueChangedEvent(this._roofControllerOptions.RoofClosedLimitSwitchPin, RoofClosedLimitSwitchCallback);

                            this._gpioController.UnregisterCallbackForPinValueChangedEvent(this._roofControllerOptions.OpenRoofButtonPin, OpenRoofButtonCallback);
                            this._gpioController.UnregisterCallbackForPinValueChangedEvent(this._roofControllerOptions.CloseRoofButtonPin, CloseRoofButtonCallback);
                            this._gpioController.UnregisterCallbackForPinValueChangedEvent(this._roofControllerOptions.StopRoofButtonPin, StopRoofButtonCallback);
                        }
                    }
                    catch { }
                    finally
                    {
                        this._gpioController?.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
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