using System;
using System.Collections.Generic;
using System.Text;

namespace HVO.Astronomy
{
    // http://www.stjarnhimlen.se/comp/tutorial.htm
    // http://www.stjarnhimlen.se/comp/riset.html
    //public static class Sun
    //{
    //    private const double RADEG = (180.0 / Math.PI);
    //    private const double DEGRAD = (Math.PI / 180.0);

    //    //  0 degrees: Center of Sun's disk touches a mathematical horizon
    //    // -0.25 degrees: Sun's upper limb touches a mathematical horizon
    //    // -0.583 degrees: Center of Sun's disk touches the horizon; atmospheric refraction accounted for
    //    // -0.833 degrees: Sun's upper limb touches the horizon; atmospheric refraction accounted for
    //    // -6 degrees: Civil twilight (one can no longer read outside without artificial illumination)
    //    // -12 degrees: Nautical twilight (navigation using a sea horizon no longer possible)
    //    // -15 degrees: Amateur astronomical twilight (the sky is dark enough for most astronomical observations)
    //    // -18 degrees: Astronomical twilight (the sky is completely dark)

    //    public static double SunRightAscensionDeclination(double dayNumber, out double rightAscension, out double declination)
    //    {
    //        /* Compute Sun's ecliptical coordinates */
    //        double longitude;
    //        double distance = SunPosition(dayNumber, out longitude);

    //        /* Compute ecliptic rectangular coordinates (z=0) */
    //        var x = distance * Math.Cos(longitude * DEGRAD);
    //        var y = distance * Math.Sin(longitude * DEGRAD);

    //        /* Compute obliquity of ecliptic (inclination of Earth's axis) */
    //        var eclipticObliquity = 23.4393 - 3.563E-7 * dayNumber;

    //        /* Convert to equatorial rectangular coordinates - x is unchanged */
    //        var z = y * Math.Sin(eclipticObliquity * DEGRAD);
    //        y = y * Math.Cos(eclipticObliquity * DEGRAD);

    //        /* Convert to spherical coordinates */
    //        rightAscension = RADEG * Math.Atan2(y, x);
    //        declination = RADEG * Math.Atan2(z, Math.Sqrt(x * x + y * y));

    //        /* Solar distance */
    //        return distance;
    //    }
    //    public static double SunAltitudeAzimuth(double dayNumber, double siteLatitude, double siteLongitude, out double altitude, out double azimuth)
    //    {
    //        double rightAscension, declination;
    //        double distance = SunRightAscensionDeclination(dayNumber, out rightAscension, out declination);

    //        var hourAngle = (DateCalculations.GMST0(dayNumber) + (dayNumber - Math.Floor(dayNumber)) * 360 + siteLongitude) - rightAscension;

    //        // Convert the Sun's hourAngle and declination to a rectangular (x,y,z) coordinate system where the X axis points to the celestial 
    //        // equator in the south, the Y axis to the horizon in the west, and the Z axis to the north celestial pole.
    //        var x = Math.Cos(hourAngle * DEGRAD) * Math.Cos(declination * DEGRAD);
    //        var y = Math.Sin(hourAngle * DEGRAD) * Math.Cos(declination * DEGRAD);
    //        var z = Math.Sin(declination * DEGRAD);

    //        // Rotate this x, y, z system along an axis going east-west, i.e. the Y axis, in such a way that the Z axis will point to the zenith.
    //        var xhor = x * Math.Sin(siteLatitude * DEGRAD) - z * Math.Cos(siteLatitude * DEGRAD);
    //        var yhor = y;
    //        var zhor = x * Math.Cos(siteLatitude * DEGRAD) + z * Math.Sin(siteLatitude * DEGRAD);

    //        altitude = RADEG * Math.Asin(zhor);
    //        azimuth = RADEG * Math.Atan2(yhor, xhor) + 180;
    //        return distance;
    //    }

    //    public static int SunriseSunset(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset sunrise, out DateTimeOffset sunset)
    //    {
    //        TimeSpan sunriseTimeSpan;
    //        TimeSpan sunsetTimeSpan;

    //        int result = __SunriseSunSet(date, siteLatitude, siteLongitude, (-35.0 / 60.0), true, out sunriseTimeSpan, out sunsetTimeSpan);

    //        sunrise = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(sunriseTimeSpan);
    //        sunset = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(sunsetTimeSpan);

    //        return result;
    //    }
    //    public static int CivilTwilight(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset twilightStart, out DateTimeOffset twilightEnd)
    //    {
    //        TimeSpan twilightStartTimeSpan;
    //        TimeSpan twilightEndTimeSpan;

    //        int result = __SunriseSunSet(date, siteLatitude, siteLongitude, -6.0, false, out twilightStartTimeSpan, out twilightEndTimeSpan);

    //        twilightStart = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(twilightStartTimeSpan);
    //        twilightEnd = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(twilightEndTimeSpan);

    //        return result;
    //    }
    //    public static int NauticalTwilight(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset twilightStart, out DateTimeOffset twilightEnd)
    //    {
    //        TimeSpan twilightStartTimeSpan;
    //        TimeSpan twilightEndTimeSpan;

    //        int result = __SunriseSunSet(date, siteLatitude, siteLongitude, -12.0, false, out twilightStartTimeSpan, out twilightEndTimeSpan);

    //        twilightStart = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(twilightStartTimeSpan);
    //        twilightEnd = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(twilightEndTimeSpan);

    //        return result;
    //    }
    //    public static int AstronomicalTwilight(DateTime date, double siteLatitude, double siteLongitude, out DateTimeOffset twilightStart, out DateTimeOffset twilightEnd)
    //    {
    //        TimeSpan twilightStartTimeSpan;
    //        TimeSpan twilightEndTimeSpan;

    //        int result = __SunriseSunSet(date, siteLatitude, siteLongitude, -18.0, false, out twilightStartTimeSpan, out twilightEndTimeSpan);

    //        twilightStart = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(twilightStartTimeSpan);
    //        twilightEnd = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.FromHours(0)).Add(twilightEndTimeSpan);

    //        return result;
    //    }

    //    public static double DayLength(DateTime date, double siteLatitude, double siteLongitude)
    //    {
    //        return __DayLength(date, siteLatitude, siteLongitude, (-35.0 / 60.0), true);
    //    }
    //    public static double CivilTwilightDayLength(DateTime date, double siteLatitude, double siteLongitude)
    //    {
    //        return __DayLength(date, siteLatitude, siteLongitude, -6.0, false);
    //    }
    //    public static double NauticalTwilightDayLength(DateTime date, double siteLatitude, double siteLongitude)
    //    {
    //        return __DayLength(date, siteLatitude, siteLongitude, -12.0, false);
    //    }
    //    public static double AstronomicalTwilightDayLength(DateTime date, double siteLatitude, double siteLongitude)
    //    {
    //        return __DayLength(date, siteLatitude, siteLongitude, -18.0, false);
    //    }

    //    /// <summary>
    //    /// Computes the Sun's ecliptic longitude and distance at an instant given in J200_UT.  
    //    /// The Sun's ecliptic latitude is not computed, since it's always very near 0.
    //    /// </summary>
    //    internal static double SunPosition(double dayNumber, out double longitude)
    //    {
    //        /* Compute mean elements */
    //        double meanAnomaly = Revolution(356.0470 + 0.9856002585 * dayNumber);
    //        double perihelionLongitude = 282.9404 + 4.70935E-5 * dayNumber;
    //        double eccentricity = 0.016709 - 1.151E-9 * dayNumber;

    //        /* Compute true longitude and radius vector */
    //        double E = meanAnomaly + eccentricity * RADEG * Math.Sin(meanAnomaly * DEGRAD) * (1.0 + eccentricity * Math.Cos(meanAnomaly * DEGRAD));

    //        double x = Math.Cos(E * DEGRAD) - eccentricity;
    //        double y = Math.Sqrt(1.0 - eccentricity * eccentricity) * Math.Sin(E * DEGRAD);

    //        /* True anomaly */
    //        double v = RADEG * Math.Atan2(y, x);

    //        /* True solar longitude */
    //        longitude = Revolution(v + perihelionLongitude);

    //        /* Solar distance */
    //        return Math.Sqrt(x * x + y * y);
    //    }

    //    /// <summary>
    //    /// Reduce angle to within 0..360 degrees
    //    /// </summary>
    //    private static double Revolution(double x)
    //    {
    //        return (x - 360.0 * Math.Floor(x * (1.0 / 360.0)));
    //    }

    //    /// <summary>
    //    /// Reduce angle to within -180.0 .. +180.0 degrees
    //    /// </summary>
    //    private static double Revolution180(double x)
    //    {
    //        return (x - 360.0 * Math.Floor(x * (1.0 / 360.0) + 0.5));
    //    }

    //    private static double __DayLength(DateTime date, double siteLatitude, double siteLongitude, double horizonAltitude, bool upperLimb)
    //    {
    //        // Calculate a dayNumber from noon at the site location.
    //        double dayNumber = DateCalculations.J2000_UT(new DateTime(date.Year, date.Month, date.Day, 12, 0, 0)) - (siteLongitude / 360.0);

    //        /* Compute obliquity of ecliptic (inclination of Earth's axis) */
    //        var eclipticObliquity = 23.4393 - 3.563E-7 * dayNumber;

    //        /* Compute Sun's position */
    //        double sunLongitude;
    //        var distance = SunPosition(dayNumber, out sunLongitude);

    //        /* Compute sine and cosine of Sun's declination */
    //        var sinSunDeclination = Math.Sin(eclipticObliquity * DEGRAD) * Math.Sin(sunLongitude * DEGRAD);
    //        var cosSunDeclination = Math.Sqrt(1.0 - sinSunDeclination * sinSunDeclination);

    //        if (upperLimb)
    //        {
    //            // Compute the altitude when the sun's upper limb touches the horizon (takes into account atmospheric refraction).
    //            double apparentRadius = (0.2666 / distance);
    //            horizonAltitude -= apparentRadius;
    //        }

    //        // Compute the diurnal arc that the Sun traverses to reach the specified altitude altitude
    //        double diurnalArc = (Math.Sin(horizonAltitude * DEGRAD) - Math.Sin(siteLatitude * DEGRAD) * Math.Sin(sinSunDeclination * DEGRAD)) /
    //          (Math.Cos(siteLatitude * DEGRAD) * Math.Cos(cosSunDeclination * DEGRAD));

    //        if (diurnalArc >= 1.0)
    //        {
    //            /* Sun always below horizonAltitude */
    //            return 0.0;
    //        }
    //        else if (diurnalArc <= -1.0)
    //        {
    //            /* Sun always above horizonAltitude */
    //            return 24.0;
    //        }
    //        else
    //        {
    //            return (2.0 / 15.0) * (RADEG * Math.Acos(diurnalArc));
    //        }
    //    }
    //    private static int __SunriseSunSet(DateTime date, double siteLatitude, double siteLongitude, double horizonAltitude, bool upperLimb, out TimeSpan sunrise, out TimeSpan sunset)
    //    {
    //        // Calculate a dayNumber from noon at the site location.
    //        double dayNumber = DateCalculations.J2000_UT(new DateTime(date.Year, date.Month, date.Day, 12, 0, 0)) - (siteLongitude / 360.0);

    //        // The local sidereal time
    //        double localSiderealTime = Revolution(DateCalculations.GMST0(dayNumber) + 180.0 + siteLongitude);

    //        // The exact RA and DEC for the sun
    //        double rightAscension, declination;
    //        double distance = SunRightAscensionDeclination(dayNumber, out rightAscension, out declination);

    //        // Compute time when Sun is at south - in hours UT 
    //        double sunInSouthUT = 12.0 - Revolution180(localSiderealTime - rightAscension) / 15.0;

    //        if (upperLimb)
    //        {
    //            // Compute the altitude when the sun's upper limb touches the horizon (takes into account atmospheric refraction).
    //            double apparentRadius = (0.2666 / distance);
    //            horizonAltitude -= apparentRadius;
    //        }

    //        // Compute the diurnal arc that the Sun traverses to reach the specified altitude altitude
    //        double diurnalArc = (Math.Sin(horizonAltitude * DEGRAD) - Math.Sin(siteLatitude * DEGRAD) * Math.Sin(declination * DEGRAD)) /
    //          (Math.Cos(siteLatitude * DEGRAD) * Math.Cos(declination * DEGRAD));

    //        if (diurnalArc >= 1.0)
    //        {
    //            /* Sun always below horizonAltitude */
    //            sunrise = TimeSpan.FromHours(sunInSouthUT);
    //            sunset = TimeSpan.FromHours(sunInSouthUT);
    //            return -1;
    //        }
    //        else if (diurnalArc <= -1.0)
    //        {
    //            /* Sun always above horizonAltitude */
    //            sunrise = TimeSpan.FromHours(sunInSouthUT - 12);
    //            sunset = TimeSpan.FromHours(sunInSouthUT + 12);
    //            return +1;
    //        }
    //        else
    //        {
    //            double offset = (RADEG * Math.Acos(diurnalArc)) / 15.0;
    //            sunrise = TimeSpan.FromHours(sunInSouthUT - offset);
    //            sunset = TimeSpan.FromHours(sunInSouthUT + offset);
    //            return 0;
    //        }
    //    }
    //}
}
