using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HVO.WebSite.V8.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherApiController : ControllerBase
    {
        [HttpGet, Route(nameof(GetWeatherData), Name = nameof(GetWeatherData))]
        public ActionResult GetWeatherData()
        {
            return Ok("OK");
        }
    }
}
