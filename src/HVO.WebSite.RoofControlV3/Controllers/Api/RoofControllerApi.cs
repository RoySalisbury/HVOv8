using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using HVO.Hardware.RoofControllerV3;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Device.I2c;
using Iot.Device.Mlx90614;

namespace HVO.WebSite.RoofControlV3.Controllers.Api
{
    [ApiController, ApiVersion("3.0"), Produces("application/json")]
    [Route("api/v{version:apiVersion}/RoofControl")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class RoofControllerApi : ControllerBase
    {
        private readonly ILogger<RoofController> _logger;
        private readonly IRoofController _roofController;

        public RoofControllerApi(ILogger<RoofController> logger, IRoofController roofController)
        {
            this._logger = logger;
            this._roofController = roofController;
        }

        [HttpGet, Route("Status", Name = "GetRoofStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoofControllerStatus>> GetRoofStatus(CancellationToken cancellationToken = default)
        {
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("Open", Name = "DoRoofOpen")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoofControllerStatus>> DoRoofOpen(CancellationToken cancellationToken = default)
        {
            this._roofController.Open();
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("Close", Name = "DoRoofClose")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoofControllerStatus>> DoRoofClose(CancellationToken cancellationToken = default)
        {
            this._roofController.Close();
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("Stop", Name = "DoRoofStop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RoofControllerStatus>> DoRoofStop(CancellationToken cancellationToken = default)
        {
            this._roofController.Stop();
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("Test", Name = "Test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Test(CancellationToken cancellationToken = default)
        {
            I2cConnectionSettings settings = new(1, 0x27 /*Mlx90614.DefaultI2cAddress*/);
            using I2cDevice i2cDevice = I2cDevice.Create(settings);

            using Mlx90614 sensor = new(i2cDevice);
            {
                Console.WriteLine($"Ambient: {sensor.ReadAmbientTemperature().DegreesFahrenheit} F");
                Console.WriteLine($"Object: {sensor.ReadObjectTemperature().DegreesFahrenheit} F");
            }


            return Ok(sensor.ReadAmbientTemperature().DegreesFahrenheit);
        }


    }
}
