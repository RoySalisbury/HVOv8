using System;
using System.Runtime.Serialization;

namespace HVO.Weather
{
    [DataContract]
    public sealed class BarometricPressure
    {
        private BarometricPressure() { }

        public static BarometricPressure FromInchesHg(double value)
        {
            return new BarometricPressure()
            {
                InchesHg = (double)value
            };
        }

        public static BarometricPressure FromInchesHg(decimal value)
        {
            return new BarometricPressure()
            {
                InchesHg = (double)value
            };
        }

        public static BarometricPressure FromMillibars(double value)
        {
            return new BarometricPressure()
            {
                Millibars = value,
            };
        }

        public static BarometricPressure FromMillibars(decimal value)
        {
            return new BarometricPressure()
            {
                Millibars = (double)value,
            };
        }

        public static BarometricPressure FromPascals(double value)
        {
            return new BarometricPressure()
            {
                Millibars = (double)value * 0.01,
            };
        }

        [DataMember]
        public double InchesHg
        {
            get;
            private set;
        }

        [IgnoreDataMember]
        public double Millibars
        {
            get
            {
                return InchesHg * 33.8637526;
            }
            private set
            {
                InchesHg = value / 33.8637526;
            }
        }

        public static BarometricPressure AltimeterFromAbsoluteMb(double absoluteMillibars, double elevationMeters)
        {
            double part1 = absoluteMillibars - 0.3;
            double part2 = (1.0 + (0.0000842288 * (elevationMeters / Math.Pow(part1, 0.190284))));

            double altimeter_setting_pressure_mb = part1 * Math.Pow(part2, 5.2553026);
            return BarometricPressure.FromInchesHg(altimeter_setting_pressure_mb * 0.02953);
        }
    }
}
