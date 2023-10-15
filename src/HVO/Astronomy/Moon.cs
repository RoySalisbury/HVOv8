using System;
using System.Drawing;

namespace HVO.Astronomy
{
    public class Moon
    {
        private static readonly double MeanSynodicMonth = 29.53058867; // Mean synodic month (average time between new moons)
        private const double LunarCyclePeriod = 29.53058868;
        private const double EarthRadiusInKm = 6378.14;

        public enum MoonPhase
        {
            NewMoon,
            WaxingCrescent,
            FirstQuarter,
            WaxingGibbous,
            FullMoon,
            WaningGibbous,
            LastQuarter,
            WaningCrescent
        }


        /// <summary>
        /// Calculates the date and time of the last New Moon before the target date using the Meeus algorithm.
        /// </summary>
        /// <param name="targetDate">The target date for which to find the last New Moon.</param>
        /// <returns>The date and time of the last New Moon as a <see cref="DateTime"/> value.</returns>
        public static DateTime GetLastNewMoonDateTime(DateTime targetDate)
        {
            // Calculate the value of k for the target date
            int k = CalculateK(targetDate);

            // Calculate the value of T (time in Julian centuries since J2000.0) for the k
            double t = CalculateT(k);

            // Calculate the time of the last New Moon using the Meeus algorithm
            double jdeLastNewMoon = DateCalculations.JulianDayEpoch + LunarCyclePeriod * k + 0.00000011 * t * t - 0.00000000031 * t * t * t;

            // Convert the Julian Ephemeris Day (JDE) to a DateTime value
            DateTime lastNewMoonDateTime = ConvertJDEToDateTime(jdeLastNewMoon);

            return lastNewMoonDateTime;
        }

        /// <summary>
        /// Calculates the date and time of the next New Moon following the target date using the Meeus algorithm.
        /// </summary>
        /// <param name="targetDate">The target date for which to find the next New Moon.</param>
        /// <returns>The date and time of the next New Moon as a <see cref="DateTime"/> value.</returns>
        public static DateTime GetNextNewMoonDateTime(DateTime targetDate)
        {
            // Calculate the value of k for the target date
            int k = CalculateK(targetDate);

            // Calculate the value of T (time in Julian centuries since J2000.0) for the k
            double t = CalculateT(k);

            // Calculate the time of the last New Moon using the Meeus algorithm
            double jdeLastNewMoon = DateCalculations.JulianDayEpoch + LunarCyclePeriod * k + 0.00000011 * t * t - 0.00000000031 * t * t * t;

            // Calculate the time of the next New Moon by adding the mean synodic month to the JDE of the last New Moon
            double jdeNextNewMoon = jdeLastNewMoon + MeanSynodicMonth;

            // Convert the Julian Ephemeris Day (JDE) to a DateTime value
            DateTime nextNewMoonDateTime = ConvertJDEToDateTime(jdeNextNewMoon);

            // If the calculated New Moon date is before the target date, calculate the New Moon for the following lunar cycle
            if (nextNewMoonDateTime <= targetDate)
            {
                jdeNextNewMoon += MeanSynodicMonth;
                nextNewMoonDateTime = ConvertJDEToDateTime(jdeNextNewMoon);
            }

            return nextNewMoonDateTime;
        }

        /// <summary>
        /// Calculates the age of the Moon in days since the last New Moon for a given date.
        /// The age of the Moon is the time elapsed since the last New Moon, indicating the number of days since the last lunar cycle began.
        /// </summary>
        /// <param name="targetDate">The date for which to calculate the Moon's age.</param>
        /// <returns>The age of the Moon in days since the last New Moon.</returns>
        public static double GetMoonAge(DateTime targetDate)
        {
            // Get the Julian Ephemeris Day (JDE) of the last New Moon
            double lastNewMoonJDE = CalculateJDE(GetLastNewMoonK(targetDate));

            // Calculate the Julian Ephemeris Day (JDE) of the target date
            double targetDateJDE = DateCalculations.GetJulianDate(targetDate);

            // Calculate the number of days since the last New Moon
            double daysSinceNewMoon = targetDateJDE - lastNewMoonJDE;

            // Ensure the result is positive (if the target date is before the last New Moon, add the lunar cycle period)
            if (daysSinceNewMoon < 0)
            {
                daysSinceNewMoon += MeanSynodicMonth;
            }

            return daysSinceNewMoon;
        }

        /// <summary>
        /// Calculates the phase of the Moon for a given date, determining whether it is a New Moon, Waxing Crescent, First Quarter, Waxing Gibbous, Full Moon, Waning Gibbous, Last Quarter, or Waning Crescent.
        /// </summary>
        /// <param name="date">The date for which to calculate the Moon phase.</param>
        /// <returns>The phase of the Moon represented by the <see cref="MoonPhase"/> enum.</returns>
        public static MoonPhase GetMoonPhase(DateTime date)
        {
            double moonAge = GetMoonAge(date);

            // Determine the Moon phase based on its age in days
            if (moonAge < 1.845 || moonAge >= 29.155) // New Moon
                return MoonPhase.NewMoon;
            else if (moonAge < 6.845) // Waxing Crescent
                return MoonPhase.WaxingCrescent;
            else if (moonAge < 8.845) // First Quarter
                return MoonPhase.FirstQuarter;
            else if (moonAge < 13.845) // Waxing Gibbous
                return MoonPhase.WaxingGibbous;
            else if (moonAge < 15.845) // Full Moon
                return MoonPhase.FullMoon;
            else if (moonAge < 20.845) // Waning Gibbous
                return MoonPhase.WaningGibbous;
            else if (moonAge < 22.845) // Last Quarter
                return MoonPhase.LastQuarter;
            else // Waning Crescent
                return MoonPhase.WaningCrescent;
        }

        public static Bitmap DrawMoonPhaseImage(DateTime date)
        {
            int imageSize = 300; // Set the desired image size
            Bitmap moonPhaseImage = new Bitmap(imageSize, imageSize);
            using (Graphics g = Graphics.FromImage(moonPhaseImage))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                MoonPhase phase = GetMoonPhase(date);
                double illuminatedPortion = GetIlluminatedPortion(phase);

                g.FillRectangle(Brushes.Black, 0, 0, imageSize, imageSize);

                // Draw the moon with the illuminated portion as a circle
                int moonDiameter = imageSize - 20;
                int moonX = 10 + (int)(illuminatedPortion * moonDiameter);
                int moonY = imageSize / 2;
                g.FillEllipse(Brushes.White, moonX, moonY - moonDiameter / 2, moonDiameter, moonDiameter);
            }

            return moonPhaseImage;
        }

        /// <summary>
        /// Calculates the altitude and azimuth of the Moon for a specific date and observer's location (latitude and longitude).
        /// </summary>
        /// <param name="targetDate">The target date for which to calculate the Moon's altitude and azimuth.</param>
        /// <param name="latitude">The latitude of the observer's location in degrees.</param>
        /// <param name="longitude">The longitude of the observer's location in degrees.</param>
        /// <returns>
        /// A tuple containing the Moon's altitude (elevation angle above the horizon) in degrees and azimuth (direction) in degrees.
        /// The altitude ranges from -90° (bottom of the horizon) to +90° (top of the horizon).
        /// The azimuth ranges from 0° (North) to 360° (clockwise from North).
        /// </returns>
        public static (double altitude, double azimuth) GetMoonAltitudeAzimuth(DateTime targetDate, double latitude, double longitude)
        {
            // Get Moon's position (Right Ascension and Declination)
            (double ra, double dec) = GetMoonPosition(targetDate);

            // Convert observer's latitude and longitude to radians
            double latRad = ToRadians(latitude);
            double lonRad = ToRadians(longitude);

            // Get Julian Ephemeris Day (JDE) for the target date
            double jd = DateCalculations.GetJulianDate(targetDate);

            // Julian centuries since J2000.0
            double t = (jd - DateCalculations.JulianDayEpoch) / DateCalculations.JulianDaysPerCentury;

            // Obliquity of the ecliptic (ε) in radians
            double obliquity = ToRadians(23.439292 - 0.00013 * t);

            // Hour angle (H) in radians
            double h = ToRadians(15 * (DateCalculations.GetLocalSiderealTime(targetDate, longitude) - ra));

            // Moon's altitude in radians
            double altitudeRad = Math.Asin(Math.Sin(dec) * Math.Sin(latRad) + Math.Cos(dec) * Math.Cos(latRad) * Math.Cos(h));

            // Moon's azimuth in radians
            double azimuthRad = Math.Atan2(-Math.Sin(h), Math.Tan(dec) * Math.Cos(latRad) - Math.Sin(latRad) * Math.Cos(h));

            // Convert to degrees
            double altitude = ToDegrees(altitudeRad);
            double azimuth = ToDegrees(azimuthRad);

            // Normalize azimuth angle to the range [0, 360)
            azimuth = To360Range(azimuth);

            return (altitude, azimuth);
        }

        /// <summary>
        /// Calculates the rise and set times of the Moon for a specific date and observer's location (latitude and longitude).
        /// </summary>
        /// <param name="targetDate">The target date for which to calculate the Moon's rise and set times.</param>
        /// <param name="latitude">The latitude of the observer's location in degrees.</param>
        /// <param name="longitude">The longitude of the observer's location in degrees.</param>
        /// <returns>
        /// A tuple containing the rise time and set time of the Moon as <see cref="DateTimeOffset"/> values in the observer's local time zone.
        /// If the Moon does not rise or set on the target date at the given location, the corresponding value in the tuple will be null.
        /// </returns>
        public static (DateTimeOffset? riseTime, DateTimeOffset? setTime) GetMoonRiseSetTime(DateTimeOffset targetDate, double latitude, double longitude)
        {
            // Get the observer's timezone
            TimeZoneInfo observerTimeZone = TimeZoneInfo.Local;

            // Convert the target date to UTC
            targetDate = targetDate.ToUniversalTime();

            // Get Moon's position (Right Ascension and Declination)
            (double ra, double dec) = GetMoonPosition(targetDate);

            // Convert observer's latitude and longitude to radians
            double latRad = ToRadians(latitude);
            double lonRad = ToRadians(longitude);

            // Calculate the observer's Local Sidereal Time (LST) in degrees
            double lst = DateCalculations.GetLocalSiderealTime(targetDate, longitude);

            // Calculate the Moon's hour angle (in degrees)
            double h = ToDegrees(Math.Acos((Math.Sin(ToRadians(-0.83)) - Math.Sin(latRad) * Math.Sin(dec)) / (Math.Cos(latRad) * Math.Cos(dec))));

            // Calculate the Moon's rise and set times in hours since midnight (UTC)
            double riseTimeHours = 360 - h - lst;
            double setTimeHours = h + lst;

            // Normalize the rise and set times to the range [0, 360)
            riseTimeHours = To360Range(riseTimeHours);
            setTimeHours = To360Range(setTimeHours);

            // Convert the rise and set times to UTC
            DateTimeOffset riseTimeUtc = targetDate.Date.AddHours(riseTimeHours);
            DateTimeOffset setTimeUtc = targetDate.Date.AddHours(setTimeHours);

            // Adjust the rise and set times for the observer's timezone
            DateTimeOffset riseTimeLocal = TimeZoneInfo.ConvertTime(riseTimeUtc, observerTimeZone);
            DateTimeOffset setTimeLocal = TimeZoneInfo.ConvertTime(setTimeUtc, observerTimeZone);

            // Check if the Moon does not rise or set on the target date
            if (double.IsNaN(riseTimeHours) || double.IsNaN(setTimeHours))
            {
                return (null, null);
            }

            // Check if the Moon sets before it rises (moon is up all day)
            if (riseTimeHours > setTimeHours)
            {
                // Adjust the rise time to the previous day
                riseTimeLocal = riseTimeLocal.AddDays(-1);
            }

            // Check if the Moon rises before the target date
            if (riseTimeLocal < targetDate)
            {
                // Adjust the rise time to the next day
                riseTimeLocal = riseTimeLocal.AddDays(1);
            }

            return (riseTimeLocal, setTimeLocal);
        }

        /// <summary>
        /// Calculates the rise and set times of the Moon for a specific date and observer's location (latitude and longitude).
        /// </summary>
        /// <param name="dateTime">The target date and time for which to calculate the Moon's rise and set times.</param>
        /// <param name="latitude">The latitude of the observer's location in degrees.</param>
        /// <param name="longitude">The longitude of the observer's location in degrees.</param>
        /// <returns>
        /// A tuple containing the rise time and set time of the Moon as <see cref="DateTime"/> values in the observer's local time zone.
        /// If the Moon does not rise or set on the target date at the given location, the corresponding value in the tuple will be null.
        /// </returns>
        public static (DateTime? riseTime, DateTime? setTime) GetMoonRiseSetTime(DateTime dateTime, double latitude, double longitude)
        {
            DateTimeOffset targetDate = new DateTimeOffset(dateTime, TimeSpan.Zero);
            (DateTimeOffset? riseTimeOffset, DateTimeOffset? setTimeOffset) = GetMoonRiseSetTime(targetDate, latitude, longitude);

            DateTime? riseTime = riseTimeOffset?.DateTime;
            DateTime? setTime = setTimeOffset?.DateTime;

            return (riseTime, setTime);
        }

        /// <summary>
        /// Calculates the position of the Moon (Right Ascension and Declination) for a specific date and time.
        /// </summary>
        /// <param name="dateTime">The target date and time for which to calculate the Moon's position.</param>
        /// <returns>
        /// A tuple containing the Moon's Right Ascension and Declination in degrees.
        /// </returns>
        public static (double rightAscension, double declination) GetMoonPosition(DateTime dateTime)
        {
            // Calls the internal method to perform the Moon position calculation
            return GetMoonPositionInternal(dateTime);
        }

        /// <summary>
        /// Calculates the position of the Moon (Right Ascension and Declination) for a specific date and time in a specific time zone.
        /// </summary>
        /// <param name="dateTimeOffset">The target date and time for which to calculate the Moon's position, along with the observer's time zone.</param>
        /// <returns>
        /// A tuple containing the Moon's Right Ascension and Declination in degrees.
        /// </returns>
        public static (double rightAscension, double declination) GetMoonPosition(DateTimeOffset dateTimeOffset)
        {
            // Calls the internal method to perform the Moon position calculation
            return GetMoonPositionInternal(dateTimeOffset.DateTime);
        }



        private static double GetIlluminatedPortion(MoonPhase phase)
        {
            // Calculate and return the illuminated portion based on the moon phase
            switch (phase)
            {
                case MoonPhase.NewMoon:
                    return 0.0;
                case MoonPhase.WaxingCrescent:
                    return 0.25;
                case MoonPhase.FirstQuarter:
                    return 0.5;
                case MoonPhase.WaxingGibbous:
                    return 0.75;
                case MoonPhase.FullMoon:
                    return 1.0;
                case MoonPhase.WaningGibbous:
                    return 0.75;
                case MoonPhase.LastQuarter:
                    return 0.5;
                case MoonPhase.WaningCrescent:
                    return 0.25;
                default:
                    return 0.0;
            }
        }

        /// <summary>
        /// Calculates the value of k, which represents the approximate number of new moons, for a given target date.
        /// The value of k is used in lunar calculations to determine the position and phase of the Moon.
        /// </summary>
        /// <param name="targetDate">The target date for which to calculate the value of k.</param>
        /// <returns>The value of k representing the approximate number of new moons for the given target date.</returns>
        private static int CalculateK(DateTime targetDate)
        {
            int year = targetDate.Year;
            int month = targetDate.Month;

            // Adjust the year and month if the date is in January or February
            int adjustedYear = month < 3 ? year - 1 : year;
            int adjustedMonth = month < 3 ? month + 12 : month;

            // Calculate k using the Meeus algorithm
            return (int)(12.3685 * (adjustedYear - 1900) + adjustedMonth - 1);
        }

        /// <summary>
        /// Calculates the value of T, representing time in Julian centuries since J2000.0, for a given value of k.
        /// T is used in lunar calculations to determine the position and phase of the Moon.
        /// </summary>
        /// <param name="k">The value of k representing the approximate number of new moons for a specific date.</param>
        /// <returns>The value of T representing time in Julian centuries since J2000.0.</returns>
        private static double CalculateT(int k)
        {
            return k / 1236.85;
        }

        /// <summary>
        /// Calculates the value of k, representing the approximate number of new moons, for the last New Moon before the target date.
        /// The value of k is used in lunar calculations to determine the position and phase of the Moon.
        /// </summary>
        /// <param name="targetDate">The target date for which to calculate the value of k.</param>
        /// <returns>The value of k representing the approximate number of new moons for the last New Moon before the target date.</returns>
        private static int GetLastNewMoonK(DateTime targetDate)
        {
            // Calculate k for the target date
            int k = CalculateK(targetDate);

            // Check if the target date is before or after the last New Moon
            if (targetDate < GetLastNewMoonDateTime(targetDate))
            {
                // If the target date is before the last New Moon, decrement k to get the k for the last New Moon
                k -= 1;
            }

            return k;
        }



        /// <summary>
        /// Calculates the Julian Ephemeris Day (JDE) for a given value of k, which represents the approximate number of new moons.
        /// JDE is used in astronomical calculations to represent dates and times in the Julian calendar system.
        /// </summary>
        /// <param name="k">The value of k representing the approximate number of new moons.</param>
        /// <returns>The Julian Ephemeris Day (JDE) for the given value of k.</returns>
        private static double CalculateJDE(double k)
        {
            double t = k / 1236.85;
            double t2 = t * t;
            double t3 = t2 * t;
            double t4 = t3 * t;
            return 2451550.09765 + 29.53058867 * k + 0.0001337 * t2 - 0.000000150 * t3 + 0.00000000073 * t4;
        }

        /// <summary>
        /// Converts a Julian Ephemeris Day (JDE) to a <see cref="DateTime"/> object in the UTC timezone.
        /// JDE is used in astronomical calculations to represent dates and times in the Julian calendar system.
        /// </summary>
        /// <param name="jde">The Julian Ephemeris Day (JDE) to convert.</param>
        /// <returns>A <see cref="DateTime"/> object representing the date and time in the UTC timezone corresponding to the given JDE.</returns>
        private static DateTime ConvertJDEToDateTime(double jde)
        {
            // Convert JDE to Julian Day (JD)
            double jd = jde + 0.5;
            int z = (int)jd;
            double f = jd - z;

            // Calculate date components
            int a = z + 1;
            int alpha = (int)(a / 365.25);
            int b = a + 1524 + alpha - (int)(alpha / 4.0);
            int c = (int)((b - 122.1) / 365.25);
            int d = (int)(365.25 * c);
            int e = (int)((b - d) / 30.6001);

            int day = b - d - (int)(30.6001 * e) + (int)f;
            int month = e < 14 ? e - 1 : e - 13;
            int year = month > 2 ? c - 4716 : c - 4715;

            // Calculate time components
            double timeOfDayFraction = f * 24.0;
            int hour = (int)timeOfDayFraction;
            timeOfDayFraction -= hour;
            int minute = (int)(timeOfDayFraction * 60.0);
            timeOfDayFraction -= (minute / 60.0);
            int second = (int)(timeOfDayFraction * 60.0);

            // Create and return the DateTime object
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }


        // Helper method to ensure an angle is within 0 to 360 degrees range
        private static double To360Range(double degrees)
        {
            double result = degrees % 360.0;
            if (result < 0)
                result += 360.0;
            return result;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private static double ToDegrees(double radians)
        {
            return radians * 180.0 / Math.PI;
        }


        /// <summary>
        /// Calculates the position of the Moon (Right Ascension and Declination) for a specific date and time (in UTC).
        /// </summary>
        /// <param name="dateTime">The target date and time (in UTC) for which to calculate the Moon's position.</param>
        /// <returns>
        /// A tuple containing the Moon's Right Ascension and Declination in radians.
        /// </returns>
        private static (double rightAscension, double declination) GetMoonPositionInternal(DateTime dateTime)
        {
            // Convert the date to UTC for consistent calculations
            dateTime = dateTime.ToUniversalTime();

            // Get Julian Ephemeris Day (JDE) for the date
            double jd = DateCalculations.GetJulianDate(dateTime);

            // Calculate T (time in Julian centuries since J2000.0)
            double t = (jd - 2451545.0) / 36525.0;

            // Mean elongation of the Moon
            double d = 297.8502042 + 445267.1115168 * t - 0.0016300 * t * t + t * t * t / 545868.0 - t * t * t * t / 113065000.0;
            d = To360Range(d);

            // Mean anomaly of the Sun
            double m = 357.5291092 + 35999.0502909 * t - 0.0001536 * t * t + t * t * t / 24490000.0;
            m = To360Range(m);

            // Mean anomaly of the Moon
            double mp = 134.9634114 + 477198.8676313 * t + 0.0089970 * t * t + t * t * t / 69699.0 - t * t * t * t / 14712000.0;
            mp = To360Range(mp);

            // Moon's argument of latitude
            double f = 93.2720993 + 483202.0175273 * t - 0.0034029 * t * t - t * t * t / 3526000.0 + t * t * t * t / 863310000.0;
            f = To360Range(f);

            // Convert to radians
            d = ToRadians(d);
            m = ToRadians(m);
            mp = ToRadians(mp);
            f = ToRadians(f);

            // Longitude correction terms
            double a1 = ToRadians(119.75 + 131.849 * t);
            double a2 = ToRadians(53.09 + 479264.290 * t);
            double a3 = ToRadians(313.45 + 481266.484 * t);

            // Moon's mean elongation from the Sun
            double dRad = d + ToRadians(6.288750 * Math.Sin(a2) + 1.274018 * Math.Sin(2 * mp - a2) +
                            0.658309 * Math.Sin(2 * mp) + 0.213616 * Math.Sin(2 * f) -
                            0.185596 * Math.Sin(m) - 0.114336 * Math.Sin(2 * d));

            // Moon's distance from the Earth (in Earth radii)
            double earthRadii = 60.40974 * (1 - Math.Cos(a2) - Math.Cos(2 * mp - a2) - Math.Cos(2 * mp) - Math.Cos(2 * f));

            // Convert the Moon's distance to kilometers
            double distanceKm = earthRadii * EarthRadiusInKm;

            // Moon's right ascension and declination
            double raRad = Math.Atan2(Math.Sin(dRad) * Math.Cos(a3), Math.Cos(dRad));
            double decRad = Math.Asin(Math.Sin(dRad) * Math.Sin(a3));

            // Convert to radians
            double rightAscension = ToDegrees(raRad);
            double declination = ToDegrees(decRad);

            return (rightAscension, declination);
        }
    }
}
