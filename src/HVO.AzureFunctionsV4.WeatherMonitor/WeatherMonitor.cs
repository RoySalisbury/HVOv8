using System;
using HVO.DataModels.HualapaiValleyObservatory;
using Microsoft.Extensions.Logging;

namespace HVO.AzureFunctionsV4.WeatherMonitor
{
    public partial class WeatherMonitor
    {
        private readonly ILogger<WeatherMonitor> _logger;
        private readonly HualapaiValleyObservatoryDbContext _dbContext;


        public WeatherMonitor(HualapaiValleyObservatoryDbContext dbContext, ILogger<WeatherMonitor> log)
        {
            this._dbContext = dbContext;
            this._logger = log;
        }
    }
}
