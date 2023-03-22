using HVO.DataModels.HualapaiValleyObservatory;
using HVO.WebSite.V8.DataContracts.Weather;
using System.Transactions;
using Microsoft.EntityFrameworkCore;

namespace HVO.WebSite.V8.Repository
{
    public sealed class WeatherRespository
    {
        private readonly HualapaiValleyObservatoryDbContext _dbContext;

        public WeatherRespository(HualapaiValleyObservatoryDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
        public async Task<LatestWeatherRecord> GetLatestWeatherRecordHighLow()
        {
            using (var transactionScope = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions() { IsolationLevel = IsolationLevel.ReadUncommitted }, TransactionScopeAsyncFlowOption.Enabled))
            {
                // Get the latest weather record that we have
                var latestRecord = await _dbContext.DavisVantageProConsoleRecords
                    .Select(x => new
                    {
                        x.RecordDateTime,
                        x.Barometer,
                        x.BarometerTrend,
                        x.InsideHumidity,
                        x.InsideTemperature,
                        x.OutsideDewpoint,
                        x.OutsideHeatIndex,
                        x.OutsideHumidity,
                        x.OutsideTemperature,
                        x.OutsideWindChill,
                        x.SolarRadiation,
                        x.UvIndex,
                        x.WindDirection,
                        x.WindSpeed,
                        Rain = new RainWeatherRecord()
                        {
                            DailyAmount = x.DailyRainAmount,
                            MonthlyAmount = x.MonthlyRainAmount,
                            RainRate = x.RainRate,
                            StormRain = x.StormRain,
                            StormStartDate = x.StormStartDate,
                            YearlyAmount = x.YearlyRainAmount
                        },
                        x.ConsoleBatteryVoltage,
                        x.TenMinuteWindSpeedAverage
                    })
                    .OrderByDescending(x => x.RecordDateTime)
                    .FirstOrDefaultAsync();

                if (latestRecord == null)
                {
                    // This should never happen unless the DB is empty.
                    return null;
                }

                // Get the High / Low for the full day of the latest weather record.
                var highLowSummary = await _dbContext.GetWeatherRecordHighLowSummary(latestRecord.RecordDateTime.Date, latestRecord.RecordDateTime);
                if (highLowSummary == null)
                {
                    // This shoud never happen
                    return null;
                }

                var result = new LatestWeatherRecord()
                {
                    MaxRecordDateTime = latestRecord.RecordDateTime,
                    Rain = latestRecord.Rain,
                    ConsoleVoltage = latestRecord.ConsoleBatteryVoltage,
                    TenMinuteWindSpeedAverage = latestRecord.TenMinuteWindSpeedAverage,

                    Barometer = new WeatherRecordItem<BarometerRecord, WeatherRecordDataPoint<decimal>>()
                    {
                        Latest = new BarometerRecord() { Value = latestRecord.Barometer, Trend = latestRecord.BarometerTrend },
                        High = new WeatherRecordDataPoint<decimal>() { DateTime = highLowSummary.BarometerHighDateTime, Value = highLowSummary.BarometerHigh },
                        Low = new WeatherRecordDataPoint<decimal>() { DateTime = highLowSummary.BarometerLowDateTime, Value = highLowSummary.BarometerLow },
                    },

                    InsideHumidity = new WeatherRecordItem<byte, WeatherRecordDataPoint<byte>>()
                    {
                        Latest = latestRecord.InsideHumidity,
                        High = new WeatherRecordDataPoint<byte>() { DateTime = highLowSummary.InsideHumidityHighDateTime, Value = highLowSummary.InsideHumidityHigh },
                        Low = new WeatherRecordDataPoint<byte> { DateTime = highLowSummary.InsideHumidityLowDateTime, Value = highLowSummary.InsideHumidityLow },
                    },

                    InsideTemperature = new WeatherRecordItem<decimal, WeatherRecordDataPoint<decimal>>()
                    {
                        Latest = latestRecord.InsideTemperature,
                        High = new WeatherRecordDataPoint<decimal>() { DateTime = highLowSummary.InsideTemperatureHighDateTime, Value = highLowSummary.InsideTemperatureHigh },
                        Low = new WeatherRecordDataPoint<decimal>() { DateTime = highLowSummary.InsideTemperatureLowDateTime, Value = highLowSummary.InsideTemperatureLow },
                    },

                    OutsideDewpoint = new WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>>()
                    {
                        Latest = latestRecord.OutsideDewpoint,
                        High = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideDewpointHighDateTime, Value = highLowSummary.OutsideDewpointHigh },
                        Low = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideDewpointLowDateTime, Value = highLowSummary.OutsideDewpointLow },
                    },

                    OutsideHeatIndex = new WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>>()
                    {
                        Latest = latestRecord.OutsideHeatIndex,
                        High = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideHeatIndexHighDateTime, Value = highLowSummary.OutsideHeatIndexHigh },
                        Low = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideHeatIndexLowDateTime, Value = highLowSummary.OutsideHeatIndexLow },
                    },

                    OutsideHumidity = new WeatherRecordItem<byte?, WeatherRecordDataPoint<byte?>>()
                    {
                        Latest = latestRecord.OutsideHumidity,
                        High = new WeatherRecordDataPoint<byte?>() { DateTime = highLowSummary.OutsideHumidityHighDateTime, Value = highLowSummary.OutsideHumidityHigh },
                        Low = new WeatherRecordDataPoint<byte?>() { DateTime = highLowSummary.OutsideHumidityLowDateTime, Value = highLowSummary.OutsideHumidityLow },
                    },

                    OutsideTemperature = new WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>>()
                    {
                        Latest = latestRecord.OutsideTemperature,
                        High = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideTemperatureHighDateTime, Value = highLowSummary.OutsideTemperatureHigh },
                        Low = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideTemperatureLowDateTime, Value = highLowSummary.OutsideTemperatureLow },
                    },

                    OutsideWindChill = new WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>>()
                    {
                        Latest = latestRecord.OutsideWindChill,
                        High = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideWindChillHighDateTime, Value = highLowSummary.OutsideWindChillHigh },
                        Low = new WeatherRecordDataPoint<decimal?>() { DateTime = highLowSummary.OutsideWindChillLowDateTime, Value = highLowSummary.OutsideWindChillLow },
                    },

                    SolarRadiation = new WeatherRecordItem<short?, WeatherRecordDataPoint<short?>>()
                    {
                        Latest = latestRecord.SolarRadiation,
                        High = highLowSummary.SolarRadiationHigh == null ? null : new WeatherRecordDataPoint<short?>() { DateTime = highLowSummary.SolarRadiationHighDateTime, Value = highLowSummary.SolarRadiationHigh },
                    },

                    UVIndex = new WeatherRecordItem<byte?, WeatherRecordDataPoint<byte?>>()
                    {
                        Latest = latestRecord.UvIndex,
                        High = highLowSummary.UVIndexHigh == null ? null : new WeatherRecordDataPoint<byte?>() { DateTime = highLowSummary.UVIndexHighDateTime, Value = highLowSummary.UVIndexHigh },
                    },

                    WindSpeed = new WeatherRecordItem<WindWeatherRecord, WindWeatherRecordDataPoint>()
                    {
                        Latest = new WindWeatherRecord() { Value = latestRecord.WindSpeed, Direction = latestRecord.WindDirection },
                        High = new WindWeatherRecordDataPoint() { DateTime = highLowSummary.WindSpeedHighDateTime, Direction = highLowSummary.WindSpeedHighDirection, Value = highLowSummary.WindSpeedHigh },
                        Low = new WindWeatherRecordDataPoint() { DateTime = highLowSummary.WindSpeedLowDateTime, Direction = highLowSummary.WindSpeedLowDirection, Value = highLowSummary.WindSpeedLow },
                    },
                };

                transactionScope.Complete();
                return result;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
        public async Task<dynamic> GetLatestWeatherRecord()
        {
            var latestRecord = await _dbContext.DavisVantageProConsoleRecords
                                .Select(x => new 
                                {
                                    x.RecordDateTime,
                                    x.Barometer,
                                    x.BarometerTrend,
                                    x.InsideHumidity,
                                    x.InsideTemperature,
                                    x.OutsideDewpoint,
                                    x.OutsideHeatIndex,
                                    x.OutsideHumidity,
                                    x.OutsideTemperature,
                                    x.OutsideWindChill,
                                    x.SolarRadiation,
                                    x.UvIndex,
                                    x.WindDirection,
                                    x.WindSpeed,
                                    Rain = new RainWeatherRecord()
                                    {
                                        DailyAmount = x.DailyRainAmount,
                                        MonthlyAmount = x.MonthlyRainAmount,
                                        RainRate = x.RainRate,
                                        StormRain = x.StormRain,
                                        StormStartDate = x.StormStartDate,
                                        YearlyAmount = x.YearlyRainAmount
                                    },
                                    x.ConsoleBatteryVoltage,
                                    x.TenMinuteWindSpeedAverage
                                })
                                .OrderByDescending(x => x.RecordDateTime)
                                .FirstOrDefaultAsync();

            if (latestRecord == null)
            {
                // This should never happen unless the DB is empty.
                return null;
            }

            return latestRecord;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "<Pending>")]
        public async Task<dynamic> GetDavisVantageProOneMinuteAverage(DateTimeOffset startDateTime, DateTimeOffset endDateTime)
        {
            // All times need to be in AZ time (-420) .. so we need to crate a new DateTimeOffset for each one. 
            startDateTime = new DateTimeOffset(startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, startDateTime.Minute, 0, Program.ObservatoryTimeZone.BaseUtcOffset);
            endDateTime = new DateTimeOffset(endDateTime.Year, endDateTime.Month, endDateTime.Day, endDateTime.Hour, endDateTime.Minute, 59, 999, Program.ObservatoryTimeZone.BaseUtcOffset);

            return await _dbContext.GetDavisVantageProOneMinuteAverage(startDateTime, endDateTime);
        }
    }
}
