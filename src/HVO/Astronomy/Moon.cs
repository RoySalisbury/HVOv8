using System;
using System.Collections.Generic;
using System.Text;

namespace HVO.Astronomy
{
    public sealed class Moon
    {
        private const double RADEG = (180.0 / Math.PI);
        private const double DEGRAD = (Math.PI / 180.0);

        public static double CalculateMoonAltitudeAzmuth(double dayNumber, double siteLatitude, double siteLongitude, out double altitude, out double azimuth)
        {
//            var distance = Sun.SunPosition(dayNumber, out var sunLongitude);

            double moonRightAscension, moonDeclination;
            var moonDistance = CalculateMoonRightAscensionDeclination(dayNumber, out moonRightAscension, out moonDeclination);
            var hourAngle = (DateCalculations.GMST0(dayNumber) + (dayNumber - Math.Floor(dayNumber)) * 360 + siteLongitude) - moonRightAscension;

            var x = Math.Cos(hourAngle * DEGRAD) * Math.Cos(moonDeclination * DEGRAD);
            var y = Math.Sin(hourAngle * DEGRAD) * Math.Cos(moonDeclination * DEGRAD);
            var z = Math.Sin(moonDeclination * DEGRAD);


            var xhor = x * Math.Sin(siteLatitude * DEGRAD) - z * Math.Cos(siteLatitude * DEGRAD);
            var yhor = y;
            var zhor = x * Math.Cos(siteLatitude * DEGRAD) + z * Math.Sin(siteLatitude * DEGRAD);

            altitude = RADEG * (Math.Asin(zhor));  // ok regner ikke måne elevation helt riktig...

            altitude = altitude - (RADEG * (Math.Asin(1 / moonDistance * Math.Cos(altitude * DEGRAD))));
            azimuth = RADEG * (Math.Atan2(yhor, xhor));

            if (siteLatitude < 0)
            {
                azimuth += 180; // added 180 deg 
            }
            else
            {
                azimuth -= 180;
            }
            return 0;
        }
        public static double CalculateMoonRightAscensionDeclination(double dayNumber, out double rightAscension, out double declination)
        {
            var sunMeanAnomaly = Revolution(356.0470 + 0.9856002585 * dayNumber);
            var sunEclipticObliquity = Revolution(23.4393 - 3.563E-7 * dayNumber);

            var sunDistance = Sun.SunPosition(dayNumber, out var sunLongitude);

            //The orbital elements of the Moon are:
            var N = Revolution(125.1228 - 0.0529538083 * dayNumber);  //   (Long asc. node)
            var i = 5.1454;                            //   (Inclination)
            var w = Revolution(318.0634 + 0.1643573223 * dayNumber);  //   (Arg. of perigee)
            var moonMeanDistance = 60.2666;                           //   (Mean distance in Earth radi)

            var moonEccentricity = 0.054900;                          //   (Eccentricity)
            var moonMeanAnomaly = Revolution(115.3654 + 13.0649929509 * dayNumber); //   (Mean anomaly)

            // Next, we compute E, the eccentric anomaly. We start with a first approximation (E0 and M in degrees):
            var eccentricAnomaly = moonMeanAnomaly + (180 / Math.PI) * moonEccentricity * Math.Sin(moonMeanAnomaly * DEGRAD) * (1 + moonEccentricity * Math.Cos(moonMeanAnomaly * DEGRAD));
            eccentricAnomaly = Revolution(eccentricAnomaly);

            var E_Error = 9.0;
            int maxIterations = 10;

            //var E0 = eccentricAnomaly;
            while ((E_Error > 0.005) && (maxIterations-- > 0))
            {
                var E0 = eccentricAnomaly;

                eccentricAnomaly = E0 - (E0 - (180 / Math.PI) * moonEccentricity * Math.Sin(E0 * DEGRAD) - moonMeanAnomaly) / (1 - moonEccentricity * Math.Cos(E0 * DEGRAD));
                eccentricAnomaly = Revolution(eccentricAnomaly);

                if (eccentricAnomaly < E0)
                {
                    E_Error = E0 - eccentricAnomaly;
                }
                else
                {
                    E_Error = eccentricAnomaly - E0;
                }
            }

            // Now we've computed E - the next step is to compute the Moon's distance and true anomaly. First we compute 
            // rectangular (x,y) coordinates in the plane of the lunar orbit:
            var x = moonMeanDistance * (Math.Cos(eccentricAnomaly * DEGRAD) - moonEccentricity);
            var y = moonMeanDistance * Math.Sin(Revolution(eccentricAnomaly) * DEGRAD) * Math.Sqrt(1 - moonEccentricity * moonEccentricity);

            // Then we convert this to distance and true anonaly:
            var moon_distance = Math.Sqrt(x * x + y * y);
            var v = Revolution(RADEG * (Math.Atan2(y, x)));

            // Now we know the Moon's position in the plane of the lunar orbit. To compute the Moon's position in ecliptic coordinates, we apply these formulae:
            var xeclip = moon_distance * (Math.Cos(N * DEGRAD) * Math.Cos((v + w) * DEGRAD) - Math.Sin((N) * DEGRAD) * Math.Sin((v + w) * DEGRAD) * Math.Cos((i) * DEGRAD));
            var yeclip = moon_distance * (Math.Sin((N) * DEGRAD) * Math.Cos((v + w) * DEGRAD) + Math.Cos((N) * DEGRAD) * Math.Sin((v + w) * DEGRAD) * Math.Cos((i) * DEGRAD));
            var zeclip = moon_distance * Math.Sin((v + w) * DEGRAD) * Math.Sin((i) * DEGRAD);

            var moon_longitude = Revolution(RADEG * (Math.Atan2(yeclip, xeclip)));   // OK
            var moon_latitude = RADEG * (Math.Atan2(zeclip, Math.Sqrt(xeclip * xeclip + yeclip * yeclip)));

            // First we need several fundamental arguments:
            //var sunsBasicPositions = SunsBasicPositions(dayNumber);

            var Lm = Revolution(N + w + moonMeanAnomaly); // Moon's mean longitude
            var D = Revolution(Lm - sunLongitude); // Moon's mean elongation
            var F = Revolution(Lm - N); // Moon's argument of latitude

            // Perbutations Moons Longitude    
            var moonLongitudePerbutation = -1.274 * Math.Sin(DEGRAD * (moonMeanAnomaly - 2 * D));  //  (Evection)
            moonLongitudePerbutation += +0.658 * Math.Sin(DEGRAD * (2 * D));       //  (Variation)
            moonLongitudePerbutation += -0.186 * Math.Sin(DEGRAD * (sunMeanAnomaly));          //  (Yearly equation)
            moonLongitudePerbutation += -0.059 * Math.Sin(DEGRAD * (2 * moonMeanAnomaly - 2 * D));
            moonLongitudePerbutation += -0.057 * Math.Sin(DEGRAD * (moonMeanAnomaly - 2 * D + sunMeanAnomaly));
            moonLongitudePerbutation += +0.053 * Math.Sin(DEGRAD * (moonMeanAnomaly + 2 * D));
            moonLongitudePerbutation += +0.046 * Math.Sin(DEGRAD * (2 * D - sunMeanAnomaly));
            moonLongitudePerbutation += +0.041 * Math.Sin(DEGRAD * (moonMeanAnomaly - sunMeanAnomaly));
            moonLongitudePerbutation += -0.035 * Math.Sin(DEGRAD * (D));           //  (Parallactic equation)
            moonLongitudePerbutation += -0.031 * Math.Sin(DEGRAD * (moonMeanAnomaly + sunMeanAnomaly));
            moonLongitudePerbutation += -0.015 * Math.Sin(DEGRAD * (2 * F - 2 * D));
            moonLongitudePerbutation += +0.011 * Math.Sin(DEGRAD * (moonMeanAnomaly - 4 * D));

            // Perbutations Moons Latitude
            var moonLatitudePerbutation = -0.173 * Math.Sin(DEGRAD * (F - 2 * D));
            moonLatitudePerbutation += -0.055 * Math.Sin(DEGRAD * (moonMeanAnomaly - F - 2 * D));
            moonLatitudePerbutation += -0.046 * Math.Sin(DEGRAD * (moonMeanAnomaly + F - 2 * D));
            moonLatitudePerbutation += +0.033 * Math.Sin(DEGRAD * (F + 2 * D));
            moonLatitudePerbutation += +0.017 * Math.Sin(DEGRAD * (2 * moonMeanAnomaly + F));

            // Perbutations Moons Distance
            var moonDistancePerbutation = -0.58 * Math.Cos(DEGRAD * (moonMeanAnomaly - 2 * D)) - 0.46 * Math.Cos(DEGRAD * (2 * D));

            // Add these to the ecliptic positions we computed earlier 
            moon_longitude += moonLongitudePerbutation;
            moon_latitude += moonLatitudePerbutation;
            moon_distance += moonDistancePerbutation;

            var xh = moon_distance * Math.Cos(DEGRAD * (moon_longitude)) * Math.Cos(DEGRAD * (moon_latitude));
            var yh = moon_distance * Math.Sin(DEGRAD * (moon_longitude)) * Math.Cos(DEGRAD * (moon_latitude));
            var zh = moon_distance * Math.Sin(DEGRAD * (moon_latitude));

            // rotate to rectangular equatorial coordinates
            var xequat = xh;
            var yequat = yh * Math.Cos(DEGRAD * (sunEclipticObliquity)) - zh * Math.Sin(DEGRAD * (sunEclipticObliquity));
            var zequat = yh * Math.Sin(DEGRAD * (sunEclipticObliquity)) + zh * Math.Cos(DEGRAD * (sunEclipticObliquity));

            rightAscension = Revolution(RADEG * (Math.Atan2(yequat, xequat)));
            declination = RADEG * (Math.Atan2(zequat, Math.Sqrt(xequat * xequat + yequat * yequat)));
            return moon_distance;
        }

        public static int CalculateMoonAge(DateTime date)
        {
            var dayNumber = DateCalculations.J2000_UT(date) + 2451545.0;
            dayNumber = (dayNumber - 2451550.1) / 29.530588853;

            dayNumber = dayNumber - (int)dayNumber;
            if (dayNumber < 0)
            {
                dayNumber += 1;
            }

            var moonAge = (int)(dayNumber * 29.53);
            return moonAge;
        }
        public static string CalculateMoonPhase(DateTime date)
        {
            var moonAge = CalculateMoonAge(date);
            if ((moonAge == 0) || (moonAge == 29))
            {
                return "New Moon";
            }
            else if ((moonAge == 1) || (moonAge == 2) || (moonAge == 3) || (moonAge == 4) || (moonAge == 5) || (moonAge == 6))
            {
                return "Waxing Cresent";
            }
            else if (moonAge == 7)
            {
                return "First Quarter";
            }
            else if ((moonAge == 8) || (moonAge == 9) || (moonAge == 10) || (moonAge == 11) || (moonAge == 12) || (moonAge == 13))
            {
                return "Waxing Gibbous";
            }
            else if (moonAge == 14)
            {
                return "Full Moon";
            }
            else if ((moonAge == 15) || (moonAge == 16) || (moonAge == 17) || (moonAge == 18) || (moonAge == 19) || (moonAge == 20) || (moonAge == 21))
            {
                return "Waning Gibbous";
            }
            else if (moonAge == 22)
            {
                return "Last Quarter";
            }
            else if ((moonAge == 23) || (moonAge == 24) || (moonAge == 25) || (moonAge == 26) || (moonAge == 27) || (moonAge == 28))
            {
                return "Waning Cresent";
            }

            return "";
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
