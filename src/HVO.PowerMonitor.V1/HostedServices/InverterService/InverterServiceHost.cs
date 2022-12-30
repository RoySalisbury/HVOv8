namespace HVO.PowerMonitor.V1.HostedServices.InverterService
{
    public sealed class InverterServiceHost : BackgroundService
    {
        private readonly ILogger<InverterServiceHost> _logger;
        private readonly InverterServiceProcessor _inverterServiceProcessor;
        private readonly IServiceScope _serviceScope;

        public InverterServiceHost(ILogger<InverterServiceHost> logger, IInverterServiceProcessor inverterServiceProcessor, IServiceScopeFactory serviceScopeFactory)
        {
            this._logger = logger;
            this._inverterServiceProcessor = inverterServiceProcessor as InverterServiceProcessor;
            this._serviceScope = serviceScopeFactory.CreateScope();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                using (var communicationsClient = this._serviceScope.ServiceProvider.GetRequiredService<InverterCommunicationsClient>())
                {
                    try
                    {
                        this._inverterServiceProcessor.InitializeInstance(communicationsClient, stoppingToken);
                        await this._inverterServiceProcessor.StartProcessing();
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Sometimes we get a TaskCancelledException as the AggregateException.InnerException ... Eveything is await'd
                        if ((ex is AggregateException) && (ex.InnerException is TaskCanceledException))
                        {
                            break;
                        }

                        if (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(5000), stoppingToken).ConfigureAwait(true);
                    }
                }
            }
        }
    }
}
