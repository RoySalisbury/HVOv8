using System;


namespace HVO.Astronomy
{
    public class SkyCoordinatesCalculator
    {
        private const double RadiansPerDegree = Math.PI / 180.0;
        private const double DegreePerRadians = 180.0 / Math.PI;


        // Converts degrees to radians
        private static double DegreesToRadians(double degrees) => degrees * RadiansPerDegree;

        // Converts radians to degrees
        private static double RadiansToDegrees(double radians) => radians * DegreePerRadians;

        // Calculate the altitude and azimuth
        public static void CalculateAltitudeAzimuth(double RA, double Dec, double observerLatitude, double localSiderealTime,
                                                   out double altitude, out double azimuth)
        {
            // Convert input angles from degrees to radians
            RA = DegreesToRadians(RA);
            Dec = DegreesToRadians(Dec);
            observerLatitude = DegreesToRadians(observerLatitude);
            localSiderealTime = DegreesToRadians(localSiderealTime);

            // Calculate the hour angle (H)
            double H = localSiderealTime - RA;

            // Calculate the altitude (Alt)
            double sinAlt = Math.Sin(Dec) * Math.Sin(observerLatitude) + Math.Cos(Dec) * Math.Cos(observerLatitude) * Math.Cos(H);
            double Alt = Math.Asin(sinAlt);

            // Calculate the azimuth (Az)
            double cosAz = (Math.Sin(Dec) - Math.Sin(Alt) * Math.Sin(observerLatitude)) / (Math.Cos(Alt) * Math.Cos(observerLatitude));
            double Az = Math.Acos(cosAz);

            // Convert altitude and azimuth from radians to degrees
            altitude = RadiansToDegrees(Alt);
            azimuth = RadiansToDegrees(Az);

            // Adjust azimuth value based on the quadrant
            if (Math.Sin(H) > 0)
            {
                azimuth = 360 - azimuth;
            }
        }
    }

}