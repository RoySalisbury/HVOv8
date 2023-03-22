using System;
using System.Collections.Generic;
using System.Text;

namespace HVO.DataContracts.Weather
{
    public sealed class LatestWeatherRecord
    {
        public DateTimeOffset MaxRecordDateTime { get; set; }

        public decimal? ConsoleVoltage { get; set; }
        public byte? TenMinuteWindSpeedAverage { get; set; }
        public RainWeatherRecord Rain { get; set; }

        public WeatherRecordItem<BarometerRecord, WeatherRecordDataPoint<decimal>> Barometer { get; set; }

        public WeatherRecordItem<decimal, WeatherRecordDataPoint<decimal>> InsideTemperature { get; set; }
        public WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>> OutsideTemperature { get; set; }
        public WeatherRecordItem<byte, WeatherRecordDataPoint<byte>> InsideHumidity { get; set; }
        public WeatherRecordItem<byte?, WeatherRecordDataPoint<byte?>> OutsideHumidity { get; set; }

        public WeatherRecordItem<WindWeatherRecord, WindWeatherRecordDataPoint> WindSpeed { get; set; }
        public WeatherRecordItem<short?, WeatherRecordDataPoint<short?>> SolarRadiation { get; set; }
        public WeatherRecordItem<byte?, WeatherRecordDataPoint<byte?>> UVIndex { get; set; }
        public WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>> OutsideHeatIndex { get; set; }
        public WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>> OutsideWindChill { get; set; }
        public WeatherRecordItem<decimal?, WeatherRecordDataPoint<decimal?>> OutsideDewpoint { get; set; }
    }

    public class WeatherRecordItem<TValue>
    {
        public TValue High { get; set; }
        public TValue Low { get; set; }
    }

    public class WeatherRecordItem<TValue, THighLow> : WeatherRecordItem<THighLow>
    {
        public TValue Latest { get; set; }
    }

    public class WeatherRecordDataPoint<TValue>
    {
        public TValue Value { get; set; }
        public DateTimeOffset? DateTime { get; set; }
    }

    public class WindWeatherRecordDataPoint : WindWeatherRecordDataPoint<byte?, short?> { }
    public class WindWeatherRecordDataPoint<TValue, TDirection> : WeatherRecordDataPoint<TValue>
    {
        public TDirection Direction { get; set; }
    }

    public class WindWeatherRecord : WindWeatherRecord<byte?, short?> { }
    public class WindWeatherRecord<TValue, TDirection>
    {
        public TValue Value { get; set; }
        public TDirection Direction { get; set; }
    }

    public class BarometerRecord : BarometerRecord<decimal, short> { }
    public class BarometerRecord<TValue, TTrend>
    {
        public TValue Value { get; set; }
        public TTrend Trend { get; set; }
    }

    public class RainWeatherRecord
    {
        public decimal? RainRate { get; set; }
        public decimal? StormRain { get; set; }
        public DateTimeOffset? StormStartDate { get; set; }
        public decimal? DailyAmount { get; set; }
        public decimal? MonthlyAmount { get; set; }
        public decimal? YearlyAmount { get; set; }
    }
}
