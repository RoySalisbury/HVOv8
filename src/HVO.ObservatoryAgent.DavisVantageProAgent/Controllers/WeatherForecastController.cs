using HVO.ObservatoryAgent.DavisVantageProAgent.NotificationServices;
using HVO.Weather.DavisVantagePro;
using Microsoft.AspNetCore.Mvc;

namespace HVO.ObservatoryAgent.DavisVantageProAgent.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly ILogger<WeatherController> _logger;
        private readonly IWeatherUpdateNotificationService _weatherUpdateNotificationService;

        public WeatherController(ILogger<WeatherController> logger, IWeatherUpdateNotificationService weatherUpdateNotificationService)
        {
            _logger = logger;
            this._weatherUpdateNotificationService = weatherUpdateNotificationService;
        }

        [HttpGet, Route("CurrentWeather", Name = nameof(GetCurrentWeather))]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public ActionResult<DavisVantageProConsoleRecord> GetCurrentWeather()
        {
            return Ok(this._weatherUpdateNotificationService.LatestRecord);
        }
    }
}