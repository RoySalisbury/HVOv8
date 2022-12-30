using Microsoft.Extensions.Options;

namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public sealed partial class InverterServiceProcessor : IInverterServiceProcessor, IDisposable
    {
        private readonly ILogger<IInverterServiceProcessor> _logger;
        private readonly InverterServiceProcessorOptions _options;
        private InverterCommunicationsClient _communicationsClient;
        private bool _disposed;
        private CancellationToken _cancellationToken;

        public InverterServiceProcessor(ILogger<IInverterServiceProcessor> logger, IOptions<InverterServiceProcessorOptions> options)
        {
            this._logger = logger;
            this._options = options.Value;
        }

        public void InitializeInstance(InverterCommunicationsClient communicationsClient, CancellationToken cancellationToken)
        {
            this._cancellationToken = cancellationToken;

            this._communicationsClient = communicationsClient;
            this._communicationsClient.Open(cancellationToken);
        }

        public Task StartProcessing()
        {
            return Task.Delay(-1, this._cancellationToken);
        }

        public void StopProcessing()
        {
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Use the 'using' pattern to call dipose on the interface method
                    using (this._communicationsClient) { }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


}
