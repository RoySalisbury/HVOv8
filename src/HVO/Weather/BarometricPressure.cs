using System;
using System.Runtime.Serialization;

namespace HVO.Weather
{
    [DataContract]
    public readonly struct BarometricPressure
    {
        public BarometricPressure(double inchesHg)
        {
            InchesHg = inchesHg;
        }

        public static BarometricPressure FromInchesHg(double value)
        {
            return new BarometricPressure(value);
        }

        public static BarometricPressure FromInchesHg(decimal value)
        {
            return FromInchesHg((double)value);
        }

        public static BarometricPressure FromMillibars(double value)
        {
            return new BarometricPressure(value / 33.8637526);
        }

        public static BarometricPressure FromMillibars(decimal value)
        {
            return FromMillibars((double)value);
        }

        public static BarometricPressure FromPascals(double value)
        {
            return FromMillibars((double)value * 0.01);
        }

        [DataMember]
        public double InchesHg { get; }

        [IgnoreDataMember]
        public double Millibars => InchesHg * 33.8637526;

        public static BarometricPressure AltimeterFromAbsoluteMb(double absoluteMillibars, double elevationMeters)
        {
            double part1 = absoluteMillibars - 0.3;
            double part2 = (1.0 + (0.0000842288 * (elevationMeters / Math.Pow(part1, 0.190284))));

            double altimeter_setting_pressure_mb = part1 * Math.Pow(part2, 5.2553026);
            return BarometricPressure.FromInchesHg(altimeter_setting_pressure_mb * 0.02953);
        }
    }
}
