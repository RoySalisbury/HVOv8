using HVO.Hardware.RoofControllerV3;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HVO.WebSite.RoofControlV3.HostedServices
{
    public class RoofControlServiceHost : BackgroundService
    {
        private readonly ILogger<RoofControlServiceHost> _logger;
        private readonly IRoofController _roofController;
        private readonly RoofControllerHostOptions _roofControllerHostOptions;

        public RoofControlServiceHost(ILogger<RoofControlServiceHost> logger, IRoofController roofController, IOptions<RoofControllerHostOptions> roofControllerHostOptions)
        {
            this._logger = logger;
            this._roofController = roofController;
            this._roofControllerHostOptions = roofControllerHostOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.Register(() => this._logger.LogDebug($"{nameof(RoofControlServiceHost)} background task is stopping."));

            this._logger.LogDebug($"{nameof(RoofControlServiceHost)} background task is starting.");
            try
            {
                // Loop this until the service is requested to stop
                while (stoppingToken.IsCancellationRequested == false)
                {
                    var roofController = this._roofController as RoofController;
                    try
                    {
                        roofController.Initialize(stoppingToken);
                        try
                        {
                            await Task.Delay(-1, stoppingToken);
                        }
                        finally
                        {
                            // We ALWAYS want to error on the side of caution and STOP the motors.  This will call dispose, which will inturn call shutdown.
                            ((IDisposable)roofController).Dispose();
                            this._logger.LogDebug("RoofController instance disposed.");
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        this._logger.LogDebug($"{nameof(RoofControlServiceHost)} TaskCanceledException.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError($"{nameof(RoofControlServiceHost)} initialization error: {ex.Message}. Restarting in {this._roofControllerHostOptions.RestartOnFailureWaitTime} seconds unless cancelled.");
                        await Task.Delay(TimeSpan.FromSeconds(this._roofControllerHostOptions.RestartOnFailureWaitTime), stoppingToken);
                    }
                }
            }
            finally
            {
                this._logger.LogDebug($"{nameof(RoofControlServiceHost)} background task has stopped.");
            }
        }
    }
}
