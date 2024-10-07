using HVO.Hardware.RoofControllerV4;
using Microsoft.Extensions.Options;

namespace HVO.WebSite.RoofControlV4.HostedServices
{
    public class RoofControllerHost : BackgroundService
    {
        private readonly ILogger<RoofControllerHost> _logger;
        private readonly IRoofController _roofController;
        private readonly RoofControllerHostOptions _options;

        public RoofControllerHost(ILogger<RoofControllerHost> logger, IOptions<RoofControllerHostOptions> options, IRoofController roofController)
        {
            _logger = logger;
            _roofController = roofController;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => this._logger.LogTrace($"{nameof(RoofControllerHost)} background task is stopping."));
            try
            {
                this._logger.LogTrace($"{nameof(RoofControllerHost)} background task is starting.");
                while (stoppingToken.IsCancellationRequested == false)
                {
                    try
                    {
                        await ServiceHostLogic(stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        this._logger.LogInformation($"{nameof(RoofControllerHost)} background task cancelled.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "{serviceName} background task stopped unexpectedly. Restarting in {waitTime} seconds.", nameof(RoofControllerHost), this._options.RestartOnFailureWaitTime);
                        await Task.Delay(TimeSpan.FromSeconds(this._options.RestartOnFailureWaitTime), stoppingToken);
                    }
                }
            }
            finally
            {
                this._logger.LogTrace($"{nameof(RoofControllerHost)} background task has stopped.");
            }
        }

        private async Task ServiceHostLogic(CancellationToken cancellationToken)
        {
            while (await this._roofController.Initialize(cancellationToken) == false)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                this._logger.LogError($"GPIO initialization error. Restarting in {this._options.RestartOnFailureWaitTime} seconds unless cancelled.");
                await Task.Delay(TimeSpan.FromSeconds(this._options.RestartOnFailureWaitTime), cancellationToken);
            }

            await Task.Delay(-1, cancellationToken);

            //while (cancellationToken.IsCancellationRequested == false)
            //{
            //    // Just keep waiting for shutdown.
            //    await Task.Delay(1000, cancellationToken);
            //}

            this._logger.LogInformation($"{nameof(ServiceHostLogic)}: Complete");
        }
    }
}
