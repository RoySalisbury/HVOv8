using HVO.DataModels.HualapaiValleyObservatory;
using HVO.Weather.DavisVantagePro;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace HVO.AzureFunctionsV4.WeatherMonitor
{
    public partial class WeatherMonitor
    {
        [Function("ProcessWebSiteWeatherRecord")]
        public void ProcessWebSiteWeatherRecord([ServiceBusTrigger("weatherrecords", "weatherrecords.website", Connection = "ServiceBusConnection")] DavisVantageProConsoleRecordReceivedEventArgs serviceBusMessage)
        {
            try
            {
                var consoleRecord = Weather.DavisVantagePro.DavisVantageProConsoleRecord.Create(serviceBusMessage.ConsoleRecord, serviceBusMessage.RecordDateTime);
                if (consoleRecord == null)
                {
                    this._logger.LogError("Unable to parse the weather console record.");
                    return;
                }

                var recordExists = this._dbContext.DavisVantageProConsoleRecords.Any(x => x.RecordDateTime == consoleRecord.RecordDateTime);
                if (recordExists)
                {
                    this._logger.LogWarning("Unable to parse the weather console record.");
                    return;
                }

                var item = new DavisVantageProConsoleRecordNew()
                {
                    Barometer = (decimal)consoleRecord.Barometer,
                    BarometerTrend = (short)consoleRecord.BarometerTrend,
                    ConsoleBatteryVoltage = (decimal?)consoleRecord.ConsoleBatteryVoltage,
                    DailyEtamount = (decimal?)consoleRecord.DailyETAmount,
                    DailyRainAmount = (decimal?)consoleRecord.DailyRainAmount,
                    ForcastIcons = (short)consoleRecord.ForcastIcons,
                    InsideHumidity = consoleRecord.InsideHumidity,
                    InsideTemperature = (decimal)consoleRecord.InsideTemperature.Fahrenheit,
                    MonthlyEtamount = (decimal?)consoleRecord.MonthlyETAmount,
                    MonthlyRainAmount = (decimal?)consoleRecord.MonthlyRainAmount,
                    OutsideDewpoint = (consoleRecord.OutsideDewpoint == null) ? null : (decimal?)consoleRecord.OutsideDewpoint.Fahrenheit,
                    OutsideHeatIndex = (consoleRecord.OutsideHeatIndex == null) ? null : (decimal?)consoleRecord.OutsideHeatIndex.Fahrenheit,
                    OutsideHumidity = (consoleRecord.OutsideHumidity == null) ? null : (byte?)consoleRecord.OutsideHumidity,
                    OutsideTemperature = (consoleRecord.OutsideTemperature == null) ? null : (decimal?)consoleRecord.OutsideTemperature.Fahrenheit,
                    OutsideWindChill = (consoleRecord.OutsideWindChill == null) ? null : (decimal?)consoleRecord.OutsideWindChill.Fahrenheit,
                    RainRate = (consoleRecord.RainRate == null) ? null : (decimal?)consoleRecord.RainRate,
                    RecordDateTime = consoleRecord.RecordDateTime,
                    SolarRadiation = (consoleRecord.SolarRadiation == null) ? null : (short?)consoleRecord.SolarRadiation,
                    StormRain = (consoleRecord.StormRain == null) ? null : (decimal?)consoleRecord.StormRain,
                    StormStartDate = consoleRecord.StormStartDate,
                    SunriseTime = consoleRecord.SunriseTime,
                    SunsetTime = consoleRecord.SunriseTime,
                    TenMinuteWindSpeedAverage = consoleRecord.TenMinuteWindSpeedAverage,
                    UvIndex = consoleRecord.UVIndex,
                    WindDirection = (consoleRecord.WindDirection == null) ? null : (short?)consoleRecord.WindDirection,
                    WindSpeed = consoleRecord.WindSpeed,
                    YearlyEtamount = (decimal?)consoleRecord.YearlyETAmount,
                    YearlyRainAmount = (decimal?)consoleRecord.YearlyRainAmount
                };

                this._dbContext.Add(item);
                this._dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
