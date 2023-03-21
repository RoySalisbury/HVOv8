using HVO.JsonConverters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace HVO.Weather
{
    [DataContract]
    public sealed class Temperature
    {
        private const double AbsoluteTemperatureC = 273.15; // absolute temperature in Celcius        
        private const double AbsoluteTemperatureF = 459.67; // absolute temperature in Fahrenheit
        private const double KelvinFahrenheitMultiplier = 9.0d / 5.0d;

        private Temperature() { }

        public static Temperature FromFahrenheit(double temperature)
        {
            return new Temperature() { Fahrenheit = temperature };
        }

        public static Temperature FromFahrenheit(decimal temperature)
        {
            return new Temperature() { Fahrenheit = (double)temperature };
        }

        public static Temperature FromCelsius(double temperature)
        {
            return new Temperature() { Celsius = temperature };
        }

        public static Temperature FromCelsius(decimal temperature)
        {
            return new Temperature() { Celsius = (double)temperature };
        }

        public static Temperature FromKelvin(double temperature)
        {
            return new Temperature() { Kelvin = temperature };
        }

        public static Temperature FromKelvin(decimal temperature)
        {
            return new Temperature() { Kelvin = (double)temperature };
        }

        public static Temperature FromRankine(double temperature)
        {
            return new Temperature() { Rankine = temperature };
        }

        public static Temperature FromRankine(decimal temperature)
        {
            return new Temperature() { Rankine = (double)temperature };
        }

        [DataMember, DoubleFormat("N2")]
        public double Fahrenheit
        {
            get
            {
                return (this.Kelvin * KelvinFahrenheitMultiplier) - AbsoluteTemperatureF;
            }
            private set
            {
                this.Kelvin = (value + AbsoluteTemperatureF) / KelvinFahrenheitMultiplier;
            }
        }

        [IgnoreDataMember, DoubleFormat("N2")]
        public double Celsius
        {
            get
            {
                return this.Kelvin - AbsoluteTemperatureC;
            }
            private set
            {
                this.Kelvin = value + AbsoluteTemperatureC;
            }
        }

        [IgnoreDataMember, DoubleFormat("N2")]
        public double Kelvin
        {
            get;
            private set;
        }

        [IgnoreDataMember, DoubleFormat("N2")]
        public double Rankine
        {
            get
            {
                return (this.Kelvin * KelvinFahrenheitMultiplier);
            }
            private set
            {
                this.Kelvin = (value / KelvinFahrenheitMultiplier);
            }
        }
    }
}
