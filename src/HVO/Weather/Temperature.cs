using HVO.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace HVO.Weather
{
    [DataContract]
    public readonly struct Temperature
    {
        private const double AbsoluteTemperatureC = 273.15; // absolute temperature in Celcius        
        private const double AbsoluteTemperatureF = 459.67; // absolute temperature in Fahrenheit
        private const double KelvinFahrenheitMultiplier = 9.0d / 5.0d;

        public Temperature(double kelvin)
        {
            Kelvin = kelvin;
        }

        public static Temperature FromFahrenheit(double temperature)
        {
            return new Temperature((temperature + AbsoluteTemperatureF) / KelvinFahrenheitMultiplier);
        }

        public static Temperature FromFahrenheit(decimal temperature)
        {
            return FromFahrenheit((double)temperature);
        }

        public static Temperature FromCelsius(double temperature)
        {
            return new Temperature(temperature + AbsoluteTemperatureC);
        }

        public static Temperature FromCelsius(decimal temperature)
        {
            return FromCelsius((double)temperature);
        }

        public static Temperature FromKelvin(double temperature)
        {
            return new Temperature(temperature);
        }

        public static Temperature FromKelvin(decimal temperature)
        {
            return FromKelvin((double)temperature);
        }

        public static Temperature FromRankine(double temperature)
        {
            return new Temperature(temperature / KelvinFahrenheitMultiplier);
        }

        public static Temperature FromRankine(decimal temperature)
        {
            return FromRankine((double)temperature);
        }

        [DataMember, DoubleFormat("N2")]
        public double Fahrenheit => (Kelvin * KelvinFahrenheitMultiplier) - AbsoluteTemperatureF;

        [IgnoreDataMember, DoubleFormat("N2")]
        public double Celsius => Kelvin - AbsoluteTemperatureC;

        [IgnoreDataMember, DoubleFormat("N2")]
        public double Kelvin { get; }

        [IgnoreDataMember, DoubleFormat("N2")]
        public double Rankine => (Kelvin * KelvinFahrenheitMultiplier);
    }
}
