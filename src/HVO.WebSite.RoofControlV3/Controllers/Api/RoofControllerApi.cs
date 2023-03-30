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
using Iot.Device.Mcp9808;

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

        [HttpGet, Route("Status", Name = nameof(GetRoofStatus))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RoofControllerStatus> GetRoofStatus()
        {
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("Open", Name = nameof(DoRoofOpen))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RoofControllerStatus> DoRoofOpen()
        {
            this._roofController.Open();
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("Close", Name = nameof(DoRoofClose))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RoofControllerStatus> DoRoofClose()
        {
            this._roofController.Close();
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("Stop", Name = nameof(DoRoofStop))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<RoofControllerStatus> DoRoofStop()
        {
            this._roofController.Stop();
            return Ok(this._roofController.Status);
        }

        [HttpGet, Route("AmbientTemperature", Name = nameof(GetAmbientTemperature))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult GetAmbientTemperature()
        {
            I2cConnectionSettings settings = new(1, 0x18 /*Mcp9808.DefaultI2cAddress*/);
            using I2cDevice i2cDevice = I2cDevice.Create(settings);

            using Mcp9808 sensor = new(i2cDevice);
            {
                Console.WriteLine($"Ambient: {sensor.Temperature.DegreesFahrenheit} F");
            }

            return Ok(sensor.Temperature.DegreesFahrenheit);
        }
    }
}
