using System;

namespace HVO.Astronomy
{
    public static class DateCalculations
    {
        public static double J2000(DateTime dateTime)
        {
            return dateTime.Subtract(new DateTime(2000, 1, 1, 12, 0, 0)).TotalSeconds / 86400;
        }

        public static double J2000_UT(DateTime dateTime)
        {
            return dateTime.Subtract(new DateTime(2000, 1, 1, 12, 0, 0).AddDays(-1.5)).TotalSeconds / 86400;
        }

        /// <summary>
        /// This function computes GMST0, the Greenwich Mean Sidereal Time at 0h UT (i.e. the sidereal 
        /// time at the Greenwhich meridian at  0h UT).  GMST is then the sidereal time at Greenwich at 
        /// any time of the day.
        /// </summary>
        internal static double GMST0(double dayNumber)
        {
            return Revolution((180.0 + 356.0470 + 282.9404) + (0.9856002585 + 4.70935E-5) * dayNumber);
        }

        /// <summary>
        /// Reduce angle to within 0..360 degrees
        /// </summary>
        private static double Revolution(double x)
        {
            return (x - 360.0 * Math.Floor(x * (1.0 / 360.0)));
        }
    }
}
