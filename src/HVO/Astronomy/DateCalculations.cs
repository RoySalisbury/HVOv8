using System;
using System.Collections.Generic;

namespace HVO.Astronomy
{
    /// <summary>
    /// A utility class providing date and time calculations related to Julian dates, J2000 epoch, Greenwich Mean Sidereal Time (GMST),
    /// and Local Sidereal Time (LST).
    /// </summary>
    public static class DateCalculations
    {
        /// <summary>
        /// The Julian Day Epoch constant used in Julian date calculations.
        /// The Julian day represents the number of days and fractions of a day since noon on January 1, 4713 BCE.
        /// </summary>
        internal const double JulianDayEpoch = 1721424.5;

        /// <summary>
        /// The number of Julian days in a tropical century (100 years).
        /// </summary>
        internal static readonly double JulianDaysPerCentury = 36525.0;

        /// <summary>
        /// The Julian date corresponding to the J2000 epoch (January 1, 2000, 12:00 TT).
        /// </summary>
        private static readonly double J2000EpochJulianDate = 2451545.0;

        /// <summary>
        /// The number of seconds in a sidereal day, which is the time it takes for the Earth to complete one rotation relative to the stars.
        /// </summary>
        private static readonly double SecondsPerSiderealDay = 86164.09054;

        /// <summary>
        /// Calculates the Julian date for the given <see cref="DateTime"/> object.
        /// The Julian date represents the number of days and fractions of a day since noon on January 1, 4713 BCE.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> object for which the Julian date needs to be calculated.</param>
        /// <returns>The Julian date as a double-precision floating-point number.</returns>
        public static double GetJulianDate(DateTime dateTime)
        {
            return GetJulianDateInternal(dateTime.Year, dateTime.Month, dateTime.Day)
                        + dateTime.TimeOfDay.TotalSeconds / (24.0 * 60.0 * 60.0);
        }

        /// <summary>
        /// Calculates the Julian date for the given <see cref="DateTimeOffset"/> object.
        /// The Julian date represents the number of days and fractions of a day since noon on January 1, 4713 BCE.
        /// </summary>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> object for which the Julian date needs to be calculated.</param>
        /// <returns>The Julian date as a double-precision floating-point number.</returns>
        public static double GetJulianDate(DateTimeOffset dateTimeOffset)
        {
            return GetJulianDateInternal(dateTimeOffset.UtcDateTime.Year, dateTimeOffset.UtcDateTime.Month, dateTimeOffset.UtcDateTime.Day)
                       + dateTimeOffset.TimeOfDay.TotalSeconds / (24.0 * 60.0 * 60.0);
        }

        /// <summary>
        /// Calculates the J2000 date for the given <see cref="DateTime"/> object.
        /// The J2000 date represents the number of days and fractions of a day since noon on January 1, 2000 CE (J2000 epoch).
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> object for which the J2000 date needs to be calculated.</param>
        /// <returns>The J2000 date as a double-precision floating-point number.</returns>
        public static double GetJ2000Date(DateTime dateTime)
        {
            return GetJulianDate(dateTime) - J2000EpochJulianDate;
        }

        /// <summary>
        /// Calculates the J2000 date for the given <see cref="DateTimeOffset"/> object.
        /// The J2000 date represents the number of days and fractions of a day since noon on January 1, 2000 CE (J2000 epoch).
        /// </summary>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> object for which the J2000 date needs to be calculated.</param>
        /// <returns>The J2000 date as a double-precision floating-point number.</returns>
        public static double GetJ2000Date(DateTimeOffset dateTimeOffset)
        {
            return GetJulianDate(dateTimeOffset.UtcDateTime) - J2000EpochJulianDate;
        }

        /// <summary>
        /// Calculates the Greenwich Mean Sidereal Time (GMST) for the given <see cref="DateTime"/> object.
        /// GMST represents the angle between the Greenwich meridian and the vernal equinox, measured in seconds.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> object for which the GMST needs to be calculated.</param>
        /// <returns>The GMST as a double-precision floating-point number (measured in seconds).</returns>
        public static double GetGreenwichMeanSiderealTime(DateTime dateTime)
        {
            double daysSinceJ2000 = GetJ2000Date(dateTime);
            double gmstSeconds = daysSinceJ2000 * SecondsPerSiderealDay;
            gmstSeconds %= SecondsPerSiderealDay;
            return gmstSeconds;
        }

        /// <summary>
        /// Calculates the Greenwich Mean Sidereal Time (GMST) for the given <see cref="DateTimeOffset"/> object.
        /// GMST represents the angle between the Greenwich meridian and the vernal equinox, measured in seconds.
        /// </summary>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> object for which the GMST needs to be calculated.</param>
        /// <returns>The GMST as a double-precision floating-point number (measured in seconds).</returns>
        public static double GetGreenwichMeanSiderealTime(DateTimeOffset dateTimeOffset)
        {
            return GetGreenwichMeanSiderealTime(dateTimeOffset.UtcDateTime);
        }

        /// <summary>
        /// Calculates the Local Sidereal Time (LST) for the given <see cref="DateTime"/> object and longitude.
        /// LST represents the angle between the observer's meridian and the vernal equinox, measured in degrees.
        /// </summary>
        /// <param name="dateTime">The <see cref="DateTime"/> object for which the LST needs to be calculated.</param>
        /// <param name="longitude">The longitude of the observer, in degrees.</param>
        /// <returns>The LST as a double-precision floating-point number (measured in degrees).</returns>
        public static double GetLocalSiderealTime(DateTime dateTime, double longitude)
        {
            double jd = GetJulianDate(dateTime);
            double gmst = GetGreenwichMeanSiderealTime(dateTime);
            double lst = gmst + longitude / 360.0 * 24.0;
            lst = lst < 0 ? lst + 24 : lst;
            lst = lst >= 24 ? lst - 24 : lst;
            return lst;
        }

        /// <summary>
        /// Calculates the Local Sidereal Time (LST) for the given <see cref="DateTimeOffset"/> object and longitude.
        /// LST represents the angle between the observer's meridian and the vernal equinox, measured in degrees.
        /// </summary>
        /// <param name="dateTimeOffset">The <see cref="DateTimeOffset"/> object for which the LST needs to be calculated.</param>
        /// <param name="longitude">The longitude of the observer, in degrees.</param>
        /// <returns>The LST as a double-precision floating-point number (measured in degrees).</returns>
        public static double GetLocalSiderealTime(DateTimeOffset dateTimeOffset, double longitude)
        {
            return GetLocalSiderealTime(dateTimeOffset.UtcDateTime, longitude);
        }

        /// <summary>
        /// Calculates the Julian date for a given date.
        /// The Julian date represents the number of days and fractions of a day since noon on January 1, 4713 BCE.
        /// </summary>
        /// <param name="year">The year of the date for which the Julian date needs to be calculated.</param>
        /// <param name="month">The month of the date for which the Julian date needs to be calculated.</param>
        /// <param name="day">The day of the date for which the Julian date needs to be calculated.</param>
        /// <returns>The Julian date as a double-precision floating-point number.</returns>
        private static double GetJulianDateInternal(int year, int month, int day)
        {
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            double a = Math.Floor(year / 100.0);
            double b = 2 - a + Math.Floor(a / 4.0);

            double julianDay = Math.Floor(365.25 * (year + 4716.0)) + Math.Floor(30.6001 * (month + 1)) + day + b - JulianDayEpoch;

            return julianDay;
        }


        internal static double CalculateJDE(double k)
        {
            double t = k / 1236.85;
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            return 2451550.09765 + 29.53058867 * k + 0.0001337 * t2 - 0.000000150 * t3 + 0.00000000073 * t4;
        }

        internal static DateTime ConvertJDEToDateTime(double jde)
        {
            double jd = jde + 0.5;
            int z = (int)jd;
            double f = jd - z;
            int a = z + 1;
            int alpha = (int)(a / 365.25);
            int b = a + 1524 + alpha - (int)(alpha / 4.0);
            int c = (int)((b - 122.1) / 365.25);
            int d = (int)(365.25 * c);
            int e = (int)((b - d) / 30.6001);
            int day = b - d - (int)(30.6001 * e) + (int)f;
            int month = e < 14 ? e - 1 : e - 13;
            int year = month > 2 ? c - 4716 : c - 4715;
            return new DateTime(year, month, day);
        }



    }
}