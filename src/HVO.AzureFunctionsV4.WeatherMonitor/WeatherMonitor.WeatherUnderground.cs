using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HVO.AzureFunctionsV4.WeatherMonitor
{
    public partial class WeatherMonitor
    {
        private static DateTimeOffset lastProcessedWeatherUndergroundRecord = DateTime.MinValue;

        [Function("ProcessWeatherUnderground")]
        public async Task ProcessWeatherUnderground([TimerTrigger("*/30 * * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {

            var record = this._dbContext.DavisVantageProConsoleRecords
                .Select(x => new
                {
                    Id = x.Id,
                    RecordDateTime = x.RecordDateTime,
                    InsideTemperature = x.InsideTemperature,
                    InsideHumidity = x.InsideHumidity,
                    WindSpeed = x.WindSpeed,
                    WindDirection = x.WindDirection,
                    OutsideTemperature = x.OutsideTemperature,
                    OutsideHumidity = x.OutsideHumidity,
                    OutsideDewpoint = x.OutsideDewpoint,
                    DailyRainAmount = x.DailyRainAmount,
                    Barometer = x.Barometer,
                    SolarRadiation = x.SolarRadiation,

                    RainAmountLast60Minutes = (x.YearlyRainAmount - this._dbContext.DavisVantageProConsoleRecords
                         .Where(y => y.RecordDateTime >= y.RecordDateTime.AddMinutes(-60) && y.RecordDateTime <= x.RecordDateTime)
                         .OrderBy(y => y.RecordDateTime)
                         .Select(y => y.YearlyRainAmount)
                         .FirstOrDefault()
                    ),

                    OneMinuteMax = this._dbContext.DavisVantageProConsoleRecords
                         .Where(y => y.RecordDateTime >= x.RecordDateTime.AddMinutes(-1) && y.RecordDateTime <= x.RecordDateTime)
                         .OrderByDescending(y => y.WindSpeed)
                         .ThenByDescending(y => y.RecordDateTime)
                         .Select(y => new { WindSpeed = y.WindSpeed, WindDirection = y.WindDirection })
                         .FirstOrDefault(),

                    TenMinuteMax = this._dbContext.DavisVantageProConsoleRecords
                         .Where(y => y.RecordDateTime >= x.RecordDateTime.AddMinutes(-10) && y.RecordDateTime <= x.RecordDateTime)
                         .OrderByDescending(y => y.WindSpeed)
                         .ThenByDescending(y => y.RecordDateTime)
                         .Select(y => new { WindSpeed = y.WindSpeed, WindDirection = y.WindDirection })
                         .FirstOrDefault(),
                })
                .Where(x => x.RecordDateTime > lastProcessedWeatherUndergroundRecord)
                .OrderByDescending(x => x.RecordDateTime)
                .FirstOrDefault();

            if (record == null)
            {
                // No new records .. we are done for now.
                this._logger.LogInformation("[WEATHER]  [WUNDERGROUND]  [{0:yyyy-MM-dd HH:mm:ss}] - NO RECORD FOUND]", DateTime.Now);
                return;
            }

            try
            {
                //// Only process this record if it is within the last 60 seconds 
                if (record.RecordDateTime.ToUniversalTime() <= DateTimeOffset.Now.ToUniversalTime().Subtract(TimeSpan.FromSeconds(60)))
                {
                    this._logger.LogInformation("[WEATHER]  [WUNDERGROUND]  [{0:yyyy-MM-dd HH:mm:ss}] - OUT OF BOUNDS]", DateTime.Now);
                    return;
                }

                var rainAmountLast60Minutes = (record.RainAmountLast60Minutes < 0) ? 0 : record.RainAmountLast60Minutes;

                // Send this record the the server
                // http://weatherstation.wunderground.com/weatherstation/updateweatherstation.php

                // Required values
                var stationId = "KAZKINGM12";
                var password = "chester";
                var action = "updateraw";
                var updateDateTime = record.RecordDateTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");

                // Non weather info
                var softwareType = "Custom"; // softwaretype     - [text] ie: WeatherLink, VWS, WeatherDisplay

                // Indoor values - should always be available
                var indoorTempature = record.InsideTemperature.ToString(); // indoortempf      - [F indoor temperature]
                var indoorHumidity = record.InsideHumidity.ToString(); // indoorhumidity   - [% indoor humidity : 0-100]

                // Outdoor values - not available if console not receiving from ISS
                var windSpeed = record.WindSpeed.ToString(); // windspeedmph     - [mph instantaneous wind speed]
                var windDirection = record.WindDirection.ToString(); // winddir          - [0-360 instantaneous wind direction]
                var windSpeedTwoMinAverage = ""; // windspdmph_avg2m - [mph 2 minute average wind speed mph]
                var windDirectionTwoMinAverage = ""; // winddir_avg2m    - [0-360 2 minute average wind direction]
                var windGustSpeed = record.OneMinuteMax.WindSpeed.ToString(); // windgustmph      - [mph current wind gust, using software specific time period]
                var windGustDirection = record.OneMinuteMax.WindDirection.ToString(); // windgustdir      - [0-360 using software specific time period]
                var winGustTenMin = record.TenMinuteMax.WindSpeed.ToString(); // windgustmph_10m  - [mph past 10 minutes wind gust mph ]
                var winGustDirectionTenMin = record.TenMinuteMax.WindDirection.ToString(); // windgustdir_10m  - [0-360 past 10 minutes wind gust direction]
                var outdoorHumidity = record.OutsideHumidity.ToString(); // humidity         - [% outdoor humidity 0-100%]
                var outdoorDewPoint = record.OutsideDewpoint.ToString(); // dewptf           - [F outdoor dewpoint F]
                var outsideTempature = record.OutsideTemperature.ToString(); // tempf            - [F outdoor temperature] * for extra outdoor sensors use temp2f, temp3f, and so on
                var rainLast60Min = rainAmountLast60Minutes.ToString(); // rainin           - [rain inches over the past hour -- the accumulated rainfall in the past 60 min]
                var rainToday = record.DailyRainAmount.ToString(); // dailyrainin      - [rain inches so far today in local time]
                var barometer = record.Barometer.ToString(); // baromin          - [barometric pressure inches]
                var weather = ""; // weather          - [text] -- metar style (+RA)
                var clouds = ""; // clouds           - [text] -- SKC, FEW, SCT, BKN, OVC
                var soilTempature = ""; // soiltempf        - [F soil temperature] * for sensors 2,3,4 use soiltemp2f, soiltemp3f, and soiltemp4f
                var soilMoisture = ""; // soilmoisture     - [%] * for sensors 2,3,4 use soilmoisture2, soilmoisture3, and soilmoisture4
                var leafWetness = ""; // leafwetness      - [%] * for sensor 2 use leafwetness2
                var solarRadiation = record.SolarRadiation.ToString(); // solarradiation   - [W/m^2]
                var UV = ""; // UV               - [index]
                var visibility = ""; // visibility       - [nm visibility]

                using (var client = new HttpClient() { BaseAddress = new Uri("http://weatherstation.wunderground.com") })
                {
                    var template = $"/weatherstation/updateweatherstation.php?ID={stationId}&PASSWORD={password}&action={action}&dateutc={updateDateTime}&softwaretype={softwareType}&indoortempf={indoorTempature}&indoorhumidity={indoorHumidity}&windspeedmph={windSpeed}&winddir={windDirection}&windspdmph_avg2m={windSpeedTwoMinAverage}&winddir_avg2m={windDirectionTwoMinAverage}&windgustmph={windGustSpeed}&windgustdir={windGustDirection}&windgustmph_10m={winGustTenMin}&windgustdir_10m={winGustDirectionTenMin}&humidity={outdoorHumidity}&dewptf={outdoorDewPoint}&tempf={outsideTempature}&rainin={rainLast60Min}&dailyrainin={rainToday}&baromin={barometer}&weather={weather}&clouds={clouds}&soiltempf={soilTempature}&soilmoisture={soilMoisture}&leafwetness={leafWetness}&solarradiation={solarRadiation}&UV={UV}&visibility={visibility}";

                    var response = await client.GetAsync(template);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        if (result.StartsWith("success"))
                        {
                            this._logger.LogInformation("[WEATHER]  [UNDERGROUND] [{0:yyyy-MM-dd HH:mm:ss}] - PROCESSED - {1:yyyy-MM-dd HH:mm:ss}", DateTime.Now, record.RecordDateTime);
                        }
                        else
                        {
                            this._logger.LogInformation("[WEATHER]  [UNDERGROUND] [{0:yyyy-MM-dd HH:mm:ss}] - FAIL - {1:yyyy-MM-dd HH:mm:ss} - {2}", DateTime.Now, record.RecordDateTime, result);
                        }
                    }
                }
            }
            finally
            {
                // This is now our last processed record.
                lastProcessedWeatherUndergroundRecord = record.RecordDateTime;
            }

        }
    }
}
