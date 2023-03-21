using HVO.Weather;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace HVO.AzureFunctionsV4.WeatherMonitor
{
    public partial class WeatherMonitor
    {
        private static DateTimeOffset lastProcessedCitizensWeatherRecord = DateTime.MinValue;
        private static string CitizensWeatherUserId = "DW4515";
        private static int CitizensWeatherPassword = -1;
        private static int StationElevationFeet = 2962;

        private static Latitude _latitude = new Latitude(35, 33, 36.1836, CompassPoint.N);
        private static Longitude _longitude = new Longitude(113, 54, 34.1424, CompassPoint.W);

        [Function("ProcessCitizensWeather")]
        public void ProcessCitizensWeather([TimerTrigger("15 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer)
        {
            var elevationM = StationElevationFeet * 0.3048;

            var record = this._dbContext.DavisVantageProConsoleRecords
                .Select(x => new
                {
                    Id = x.Id,
                    RecordDateTime = x.RecordDateTime,
                    OutsideTemperature = x.OutsideTemperature,
                    OutsideHumidity = x.OutsideHumidity,
                    HourlyRainAmount = (int?)null,
                    DailyRainAmount = x.DailyRainAmount,
                    Barometer = x.Barometer,
                    SolarRadiation = x.SolarRadiation,

                    OneMinuteAvg = this._dbContext.DavisVantageProConsoleRecords
                         .Where(y => y.RecordDateTime >= x.RecordDateTime.AddMinutes(-1) && y.RecordDateTime <= x.RecordDateTime)
                         .GroupBy(y => "1")
                         .Select(y => new { WindSpeed = y.Average(z => z.WindSpeed), WindDirection = y.Average(z => z.WindDirection) })
                         .FirstOrDefault(),

                    FiveMinuteMaxWindSpeed = this._dbContext.DavisVantageProConsoleRecords
                         .Where(y => y.RecordDateTime >= x.RecordDateTime.AddMinutes(-5) && y.RecordDateTime <= x.RecordDateTime)
                         .OrderByDescending(y => y.WindSpeed)
                         .ThenByDescending(y => y.RecordDateTime)
                         .Select(y => y.WindSpeed)
                         .FirstOrDefault(),
                })
                .Where(x => x.RecordDateTime > lastProcessedCitizensWeatherRecord)
                .OrderByDescending(x => x.RecordDateTime)
                .FirstOrDefault();

            if (record == null)
            {
                // No new records .. we are done for now.
                this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - NO RECORD FOUND]", DateTime.Now);
                return;
            }

            try
            {
                //// Only process this record if it is within the last 60 seconds 
                if (record.RecordDateTime.ToUniversalTime() <= DateTimeOffset.Now.ToUniversalTime().Subtract(TimeSpan.FromSeconds(60)))
                {
                    this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - OUT OF BOUNDS]", DateTime.Now);
                    return;
                }

                double barometer = BarometricPressure.FromInchesHg(record.Barometer).Millibars;
                Temperature temperature = Temperature.FromFahrenheit(record.OutsideTemperature.Value);

                double reductionRatio = WxUtils.PressureReductionRatio(barometer, elevationM, temperature, temperature, record.OutsideHumidity.Value, WxUtils.SLPAlgorithm.paDavisVP);
                double madisBarometer = WxUtils.StationToAltimeter(barometer / reductionRatio, elevationM);

                // Create the record to send to the CWOP server
                string cwopDataRecord = string.Format("{0}>APRS,TCPXX*:@{1}z{2}/{3}_{4}/{5}g{6}t{7}r{8}P{9}{10}h{11}b{12}{13}",
                  CitizensWeatherUserId,
                  record.RecordDateTime.ToUniversalTime().ToString("ddHHmm"),
                  string.Format("{0:00}{1:00.00}{2}", _latitude.Degrees, TimeSpan.FromSeconds((_latitude.Minutes * 60) + _latitude.Seconds).TotalMinutes, _latitude.Direction),
                  string.Format("{0:000}{1:00.00}{2}", _longitude.Degrees, TimeSpan.FromSeconds((_longitude.Minutes * 60) + _longitude.Seconds).TotalMinutes, _longitude.Direction),

                  record.OneMinuteAvg == null ? "..." : string.Format("{0:000}", record.OneMinuteAvg.WindDirection),
                  record.OneMinuteAvg == null ? "..." : string.Format("{0:000}", record.OneMinuteAvg.WindSpeed),

                  record.FiveMinuteMaxWindSpeed == null ? "..." : string.Format("{0:000}", record.FiveMinuteMaxWindSpeed),
                  record.OutsideTemperature == null ? "..." : (record.OutsideTemperature < 0 ? string.Format("{0:00}", record.OutsideTemperature) : string.Format("{0:000}", record.OutsideTemperature)),
                  record.HourlyRainAmount == null ? "..." : string.Format("{0:000}", record.HourlyRainAmount * 100),
                  record.DailyRainAmount == null ? "..." : string.Format("{0:000}", record.DailyRainAmount * 100),
                  record.SolarRadiation == null ? "" : (record.SolarRadiation < 1000 ? string.Format("L{0:000}", record.SolarRadiation) : string.Format("l{0:000}", record.SolarRadiation - 1000)),
                  record.OutsideHumidity == null ? ".." : (record.OutsideHumidity == 0 ? "01" : record.OutsideHumidity >= 100 ? "00" : string.Format("{0:00}", record.OutsideHumidity)),
                  string.Format("{0:00000}", Math.Truncate(madisBarometer * 10)),
                  "DVs");

                this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - START PROCESSING", DateTime.Now);

                // Open a connection to the CWOP server
                using (TcpClient tcpClient = new TcpClient("cwop.aprs.net", 14580))
                {
                    using (NetworkStream networkStream = tcpClient.GetStream())
                    {
                        using (StreamReader sr = new StreamReader(networkStream))
                        {
                            using (StreamWriter sw = new StreamWriter(networkStream))
                            {
                                // Login to the server
                                try
                                {
                                    sw.WriteLine(string.Format("user {0} {1} vers {2}", CitizensWeatherUserId, CitizensWeatherPassword, "1.0.0.0"));
                                    string loginResult = sr.ReadLine();
                                    this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - LOGIN - {1}", DateTime.Now, loginResult);

                                    try
                                    {
                                        // Send the data packet
                                        sw.WriteLine(cwopDataRecord);
                                        string sendResult = sr.ReadLine();

                                        this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - RECORD SENT - {1}", DateTime.Now, sendResult);
                                        this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - SUCCESS - {1}", DateTime.Now, cwopDataRecord);
                                    }
                                    catch (Exception ex)
                                    {
                                        this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - SEND ERROR - {1}", DateTime.Now, ex.Message);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - LOGIN ERROR - {1}", DateTime.Now, ex.Message);
                                }
                            }
                        }
                    }
                }
                this._logger.LogInformation("[WEATHER]  [CWOP]          [{0:yyyy-MM-dd HH:mm:ss}] - END PROCESSING", DateTime.Now);
            }
            finally
            {
                // This is now our last processed record.
                lastProcessedCitizensWeatherRecord = record.RecordDateTime;
            }

        }
    }
}
