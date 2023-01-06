namespace HVO.PowerMonitor.V1.HostedServices.BatteryService
{
    public abstract class BatteryManagerService : IDisposable
    {
        private bool _disposed;
        private BatteryManagerCommunicationDevice _communicationsDevice;

        protected readonly CancellationTokenSource _internalCancellationTokenSource;

        protected BatteryManagerService() 
        {
            this._internalCancellationTokenSource = new CancellationTokenSource();
        }

        public Guid ServiceId { get; init; }

        public virtual void Initialize()
        {
            // Reset the cancellationToken that this instance uses internally. No one outside this instance will ever have access to it.
            this._internalCancellationTokenSource.TryReset();
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this._internalCancellationTokenSource.Token);

            await Task.Delay(-1, linkedTokenSource.Token);
        }

        public void Stop()
        {
            this._internalCancellationTokenSource.Cancel();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    this.Stop();

                    //this._communicationsDevice?.Dispose();
                }

                _disposed = true;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class JBDBatteryManagerService : BatteryManagerService
    {
        public JBDBatteryManagerService() { }
    }

    public sealed class JKBatteryManagerService : BatteryManagerService
    {
        public JKBatteryManagerService() { }
    }
}
