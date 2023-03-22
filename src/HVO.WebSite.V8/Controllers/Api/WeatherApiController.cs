using HVO.WebSite.V8.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HVO.WebSite.V8.Controllers.Api
{
    [ApiController]
    [Route("api/weather")]
    public class WeatherApiController : ControllerBase
    {
        private readonly WeatherRespository _weatherRepository;

        public WeatherApiController(WeatherRespository weatherRepository)
        {
            this._weatherRepository = weatherRepository;
        }

        [HttpGet("GetLatestWeatherRecordHighLow")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
        public async Task<IActionResult> GetLatestWeatherRecordHighLow()
        {
            var result = await this._weatherRepository.GetLatestWeatherRecordHighLow();
            return Ok(result);
        }

        [HttpGet("GetLatestWeatherRecord")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
        public async Task<IActionResult> GetLatestWeatherRecord()
        {
            var result = await this._weatherRepository.GetLatestWeatherRecord();
            return Ok(result);
        }

        [HttpGet("GetDavisVantageProOneMinuteAverage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
        public async Task<IActionResult> GetDavisVantageProOneMinuteAverage(DateTimeOffset startDateTime, DateTimeOffset endDateTime)
        {
            var result = await this._weatherRepository.GetDavisVantageProOneMinuteAverage(startDateTime, endDateTime);
            return Ok(result);
        }
    }
}
