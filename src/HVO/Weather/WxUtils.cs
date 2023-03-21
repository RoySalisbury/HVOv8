using System;
using System.Collections.Generic;
using System.Text;

namespace HVO.Weather
{
    public static class WxUtils
    {
        public enum SLPAlgorithm
        {
            paDavisVP,  // algorithm closely approximates SLP calculation used inside Davis Vantage Pro weather equipment console (http://www.davisnet.com/weather)
            paUnivie,   // http://www.univie.ac.at/IMG-Wien/daquamap/Parametergencom.html
            paManBar    // from Manual of Barometry (1963)
        }

        // Altimeter algorithms
        public enum AltimeterAlgorithm
        {
            aaASOS,     // formula described in the ASOS training docs
            aaASOS2,    // metric formula that was likely used to derive the aaASOS formula
            aaMADIS,    // apparently the formula used by the MADIS system
            aaNOAA,     // essentially the same as aaSMT with any result differences caused by unit conversion rounding error and geometric vs. geopotential elevation
            aaWOB,      // Weather Observation Handbook (algorithm similar to aaASOS & aaASOS2 - main differences being precision of constants used)
            aaSMT       // Smithsonian Meteorological Tables (1963)
        }

        // Vapor Pressure algorithms
        public enum VapAlgorithm
        {
            vaDavisVp,  // algorithm closely approximates calculation used by Davis Vantage Pro weather stations and software
            vaBuck,     // this and the remaining algorithms described at http://cires.colorado.edu/~voemel/vp.html
            vaBuck81,
            vaBolton,
            vaTetenNWS,
            vaTetenMurray,
            vaTeten
        }

        private const SLPAlgorithm DefaultSLPAlgorithm = SLPAlgorithm.paManBar;
        private const AltimeterAlgorithm DefaultAltimeterAlgorithm = AltimeterAlgorithm.aaMADIS;
        private const VapAlgorithm DefaultVapAlgorithm = VapAlgorithm.vaBolton;

        // U.S. Standard Atmosphere (1976) constants
        private const double gravity = 9.80665;                                // g at sea level at latitude 45.5 degrees in m/sec^2
        private const double uGC = 8.31432;                                    // universal gas constant in J/mole-K
        private const double moleAir = 0.0289644;                              // mean molecular mass of air in kg/mole
        private const double moleWater = 0.01801528;                           // molecular weight of water in kg/mole
        private const double gasConstantAir = uGC / moleAir;                     // (287.053) gas constant for air in J/kgK
        private const double standardSLP = 1013.25;                            // standard sea level pressure in hPa
        private const double standardSlpInHg = 29.921;                         // standard sea level pressure in inHg
        private const double standardTempK = 288.15;                           // standard sea level temperature in Kelvin
        private const double earthRadius45 = 6356.766;                         // radius of the earth at latitude 45.5 degrees in km

        private const double standardLapseRate = 0.0065;                       // standard lapse rate (6.5C/1000m i.e. 6.5K/1000m)
        private const double standardLapseRateFt = standardLapseRate * 0.3048; // (0.0019812) standard lapse rate per foot (1.98C/1000ft)
        private const double vpLapseRateUS = 0.00275;                          // lapse rate used by Davis VantagePro (2.75F/1000ft)
        private const double manBarLapseRate = 0.0117;                         // lapse rate from Manual of Barometry (11.7F/1000m, which = 6.5C/1000m)


        public static double StationToSensorPressure(double pressureHPa, double sensorElevationM, double stationElevationM, Temperature temperature)
        {
            // from ASOS formula specified in US units
            BarometricPressure barometer = BarometricPressure.FromMillibars(pressureHPa);
            return BarometricPressure.FromInchesHg(barometer.InchesHg / Power10(0.00813 * MToFt(sensorElevationM - stationElevationM) / temperature.Rankine)).Millibars;
        }

        public static double StationToAltimeter(double pressureHPa, double elevationM, AltimeterAlgorithm algorithm = DefaultAltimeterAlgorithm)
        {
            BarometricPressure barometer = BarometricPressure.FromMillibars(pressureHPa);

            switch (algorithm)
            {
                case AltimeterAlgorithm.aaASOS:
                    {
                        // see ASOS training at http://www.nwstc.noaa.gov
                        // see also http://wahiduddin.net/calc/density_altitude.htm
                        return BarometricPressure.FromInchesHg(Power(Power(barometer.InchesHg, 0.1903) + (1.313E-5 * MToFt(elevationM)), 5.255)).Millibars;
                    }
                case AltimeterAlgorithm.aaASOS2:
                    {
                        var geopEl = GeopotentialAltitude(elevationM);
                        var k1 = standardLapseRate * gasConstantAir / gravity; // approx. 0.190263
                        var k2 = 8.41728638E-5; // (standardLapseRate / standardTempK) * (Power(standardSLP,  k1)
                        return Power(Power(pressureHPa, k1) + (k2 * geopEl), 1 / k1);
                    }
                case AltimeterAlgorithm.aaMADIS:
                    {
                        // from MADIS API by NOAA Forecast Systems Lab, see http://madis.noaa.gov/madis_api.html
                        var k1 = 0.190284; // discrepency with calculated k1 probably because Smithsonian used less precise gas constant and gravity values
                        var k2 = 8.4184960528E-5; // (standardLapseRate / standardTempK) * (Power(standardSLP, k1)
                        return Power(Power(pressureHPa - 0.3, k1) + (k2 * elevationM), 1 / k1);
                    }
                case AltimeterAlgorithm.aaNOAA:
                    {
                        // see http://www.srh.noaa.gov/elp/wxclc/formulas/altimeterSetting.html
                        var k1 = 0.190284; // discrepency with k1 probably because Smithsonian used less precise gas constant and gravity values
                        var k2 = 8.42288069E-5; // (standardLapseRate / 288) * (Power(standardSLP, k1SMT);
                        return (pressureHPa - 0.3) * Power(1 + (k2 * (elevationM / Power(pressureHPa - 0.3, k1))), 1 / k1);
                    }
                case AltimeterAlgorithm.aaWOB:
                    {
                        // see http://www.wxqa.com/archive/obsman.pdf
                        var k1 = standardLapseRate * gasConstantAir / gravity; // approx. 0.190263
                        var k2 = 1.312603E-5; //(standardLapseRateFt / standardTempK) * Power(standardSlpInHg, k1);
                        return BarometricPressure.FromInchesHg(Power(Power(barometer.InchesHg, k1) + (k2 * MToFt(elevationM)), 1 / k1)).Millibars;
                    }
                case AltimeterAlgorithm.aaSMT:
                    {
                        // see WMO Instruments and Observing Methods Report No.19 at http://www.wmo.int/pages/prog/www/IMOP/publications/IOM-19-Synoptic-AWS.pdf
                        var k1 = 0.190284; // discrepency with calculated value probably because Smithsonian used less precise gas constant and gravity values
                        var k2 = 4.30899E-5; // (standardLapseRate / 288) * (Power(standardSlpInHg, k1SMT));
                        var geopEl = GeopotentialAltitude(elevationM);
                        return BarometricPressure.FromInchesHg((barometer.InchesHg - 0.01) * Power(1 + (k2 * (geopEl / Power(barometer.InchesHg - 0.01, k1))), 1 / k1)).Millibars;
                    }
                default:
                    {
                        // unknown algorithm
                        return pressureHPa;
                    }
            }
        }

        public static double StationToSeaLevelPressure(double pressureHPa, double elevationM, Temperature currentTemperature, Temperature meanTemperature, byte humidity, SLPAlgorithm algorithm = DefaultSLPAlgorithm)
        {
            return pressureHPa * PressureReductionRatio(pressureHPa, elevationM, currentTemperature, meanTemperature, humidity, algorithm);
        }

        public static double SensorToStationPressure(double pressureHPa, double sensorElevationM, double stationElevationM, Temperature temperature)
        {
            // see ASOS training at http://www.nwstc.noaa.gov
            // from US units ASOS formula
            BarometricPressure barometer = BarometricPressure.FromMillibars(pressureHPa);
            return BarometricPressure.FromInchesHg(barometer.InchesHg * Power10(0.00813 * MToFt(sensorElevationM - stationElevationM) / temperature.Rankine)).Millibars;
        }

        //public static double AltimeterToStationPressure(double pressureHPa, double elevationM, AltimeterAlgorithm algorithm = DefaultAltimeterAlgorithm){ retun 0.0; }

        public static double SeaLevelToStationPressure(double pressureHPa, double elevationM, Temperature currentTemperature, Temperature meanTemperature, byte humidity, SLPAlgorithm algorithm = DefaultSLPAlgorithm)
        {
            return pressureHPa / PressureReductionRatio(pressureHPa, elevationM, currentTemperature, meanTemperature, humidity, algorithm);
        }

        // low level pressure related functions
        public static double PressureReductionRatio(double pressureHPa, double elevationM, Temperature currentTemperature, Temperature meanTemperature, byte humidity, SLPAlgorithm algorithm = DefaultSLPAlgorithm)
        {
            switch (algorithm)
            {
                case SLPAlgorithm.paDavisVP:
                    {
                        // see http://www.exploratorium.edu/weather/barometer.html

                        double hCorr = 0;
                        if (humidity > 0)
                        {
                            hCorr = (9 / 5) * HumidityCorrection(currentTemperature, elevationM, humidity, VapAlgorithm.vaDavisVp);
                        }

                        // In the case of DavisVp, take the constant values literally.
                        return Power(10, (MToFt(elevationM) / (122.8943111 * (meanTemperature.Fahrenheit + 460 + (MToFt(elevationM) * vpLapseRateUS / 2) + hCorr))));
                    }
                case SLPAlgorithm.paUnivie:
                    {
                        //  see http://www.univie.ac.at/IMG-Wien/daquamap/Parametergencom.html
                        double geopElevationM = GeopotentialAltitude(elevationM);
                        return Math.Exp(((gravity / gasConstantAir) * geopElevationM) / (VirtualTempK(pressureHPa, meanTemperature, humidity) + (geopElevationM * standardLapseRate / 2)));
                    }
                case SLPAlgorithm.paManBar:
                    {
                        // see WMO Instruments and Observing Methods Report No.19 at http://www.wmo.int/pages/prog/www/IMOP/publications/IOM-19-Synoptic-AWS.pdf
                        // see WMO Instruments and Observing Methods Report No.19 at http://www.wmo.ch/web/www/IMOP/publications/IOM-19-Synoptic-AWS.pdf

                        double hCorr = 0;
                        if (humidity > 0)
                        {
                            hCorr = (9 / 5) * HumidityCorrection(currentTemperature, elevationM, humidity, VapAlgorithm.vaBuck);
                        }

                        double geopElevationM = GeopotentialAltitude(elevationM);
                        return Math.Exp(geopElevationM * 6.1454E-2 / (meanTemperature.Fahrenheit + 459.7 + (geopElevationM * manBarLapseRate / 2) + hCorr));
                    }
                default:
                    {
                        // unknown algorithm
                        return 1;
                    }
            }
        }

        public static double ActualVaporPressure(Temperature temperature, byte humidity, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            return (humidity * SaturationVaporPressure(temperature, algorithm)) / 100;
        }

        public static double SaturationVaporPressure(Temperature temperature, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            // see http://cires.colorado.edu/~voemel/vp.html   comparison of vapor pressure algorithms
            // see (for DavisVP) http://www.exploratorium.edu/weather/dewpoint.html
            switch (algorithm)
            {
                case VapAlgorithm.vaDavisVp:
                    {
                        return 6.112 * Math.Exp((17.62 * temperature.Celsius) / (243.12 + temperature.Celsius)); // Davis Calculations Doc
                    }
                case VapAlgorithm.vaBuck:
                    {
                        return 6.1121 * Math.Exp((18.678 - (temperature.Celsius / 234.5)) * temperature.Celsius / (257.14 + temperature.Celsius)); // Buck(1996)
                    }
                case VapAlgorithm.vaBuck81:
                    {
                        return 6.1121 * Math.Exp((17.502 * temperature.Celsius) / (240.97 + temperature.Celsius)); // Buck(1981)
                    }
                case VapAlgorithm.vaBolton:
                    {
                        return 6.112 * Math.Exp(17.67 * temperature.Celsius / (temperature.Celsius + 243.5)); // Bolton(1980)
                    }
                case VapAlgorithm.vaTetenNWS:
                    {
                        return 6.112 * Power(10, (7.5 * temperature.Celsius / (temperature.Celsius + 237.7))); // Magnus Teten see www.srh.weather.gov/elp/wxcalc/formulas/vaporPressure.html
                    }
                case VapAlgorithm.vaTetenMurray:
                    {
                        return Power(10, (7.5 * temperature.Celsius / (237.5 + temperature.Celsius)) + 0.7858); // Magnus Teten (Murray 1967)
                    }
                case VapAlgorithm.vaTeten:
                    {
                        return 6.1078 * Power(10, (7.5 * temperature.Celsius / (temperature.Celsius + 237.3))); // Magnus Teten see www.vivoscuola.it/US/RSIGPP3202/umidita/attivita/relhumONA.htm
                    }
                default:
                    {
                        // unknown algorithm
                        return 0;
                    }
            }
        }

        public static double MixingRatio(double pressureHPa, Temperature temperature, byte humidity)
        {
            // see http://www.wxqa.com/archive/obsman.pdf
            // see also http://www.vivoscuola.it/US/RSIGPP3202/umidita/attiviat/relhumONA.htm
            double vapPres = ActualVaporPressure(temperature, humidity, VapAlgorithm.vaBuck);
            return 1000 * (((moleWater / moleAir) * vapPres) / (pressureHPa - vapPres));
        }

        public static double VirtualTempK(double pressureHPa, Temperature temperature, byte humidity)
        {
            // see http://www.univie.ac.at/IMG-Wien/daquamap/Parametergencom.html
            // see also http://www.vivoscuola.it/US/RSIGPP3202/umidita/attiviat/relhumONA.htm
            // see also http://wahiduddin.net/calc/density_altitude.htm
            double epsilon = 1 - (moleWater / moleAir);

            double vapPres = ActualVaporPressure(temperature, humidity, VapAlgorithm.vaBuck);
            return (temperature.Kelvin) / (1 - (epsilon * (vapPres / pressureHPa)));
        }

        public static double HumidityCorrection(Temperature temperature, double elevationM, byte humidity, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            double vapPress = ActualVaporPressure(temperature, humidity, algorithm);
            return (vapPress * ((2.8322E-9 * (elevationM * elevationM)) + (2.225E-5 * elevationM) + 0.10743));
        }

        // temperature related functions
        internal static Temperature DewPoint(Temperature temperature, byte humidity, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            double lnVapor = Math.Log(ActualVaporPressure(temperature, humidity, algorithm));

            switch (algorithm)
            {
                case VapAlgorithm.vaDavisVp:
                    {
                        return Temperature.FromCelsius(((243.12 * lnVapor) - 440.1) / (19.43 - lnVapor));
                    }
                default:
                    {
                        return Temperature.FromCelsius(((237.7 * lnVapor) - 430.22) / (19.08 - lnVapor));
                    }
            }
        }

        internal static Temperature WindChill(Temperature temperature, double windSpeedKmph)
        {
            // see http://ams.allenpress.com/perlserv/?request=get-abstract&doi=10.1175/BAMS-86-10-1453
            // see http://www.msc.ec.gc.ca/education/windchill/science_equations_e.cfm
            // see http://www.weather.gov/os/windchill/index.shtml
            double result;

            if ((temperature.Celsius >= 10.0) | (windSpeedKmph <= 4.8))
            {
                result = temperature.Celsius;
            }
            else
            {
                double windPow = Power(windSpeedKmph, 0.16);
                result = 13.12 + (0.6215 * temperature.Celsius) - (11.37 * windPow) + (0.3965 * temperature.Celsius * windPow);
            }

            if (result > temperature.Celsius)
            {
                result = temperature.Celsius;
            }

            return Temperature.FromCelsius(result);
        }

        internal static Temperature HeatIndex(Temperature temperature, byte humidity)
        {
            double Result;

            if (temperature.Fahrenheit < 80)
            {
                // heat index algorithm is only valid for temps above 80F
                Result = temperature.Fahrenheit;
            }
            else
            {
                double tSqrd = temperature.Fahrenheit * temperature.Fahrenheit;
                double hum = humidity;
                double hSqrd = hum * hum;

                Result = 0 - 42.379 + (2.04901523 * temperature.Fahrenheit) + (10.14333127 * humidity)
                      - (0.22475541 * temperature.Fahrenheit * humidity) - (0.00683783 * tSqrd)
                      - (0.05481717 * hSqrd) + (0.00122874 * tSqrd * humidity)
                      + (0.00085282 * temperature.Fahrenheit * hSqrd) - (0.00000199 * tSqrd * hSqrd);

                // Rothfusz adjustments
                if ((humidity < 13) & (temperature.Fahrenheit >= 80) & (temperature.Fahrenheit <= 112))
                {
                    Result = Result - ((13 - humidity) / 4) * Math.Sqrt((17 - Math.Abs(temperature.Fahrenheit - 95)) / 17);
                }
                else if ((humidity > 85) & (temperature.Fahrenheit >= 80) & (temperature.Fahrenheit <= 87))
                {
                    Result = Result + ((humidity - 85) / 10) * ((87 - temperature.Fahrenheit) / 5);
                }
            }
            return Temperature.FromFahrenheit(Result);
        }

        internal static Temperature Humidex(Temperature temperature, byte humidity)
        {
            return Temperature.FromCelsius(temperature.Celsius + ((5 / 9) * (ActualVaporPressure(temperature, humidity, VapAlgorithm.vaTetenNWS) - 10.0)));
        }

        // simplified algorithm for geopotential altitude from US Standard Atmosphere 1976 .. assumes latitude 45.5 degrees
        public static double GeopotentialAltitude(double geometricAltitudeM)
        {
            return (earthRadius45 * 1000 * geometricAltitudeM) / ((earthRadius45 * 1000) + geometricAltitudeM); ;
        }

        // Feet to Meters
        internal static double FtToM(double value)
        {
            return value * 0.3048;
        }

        // Meters to Feet
        internal static double MToFt(double value)
        {
            return value / 0.3048;
        }

        // Inches to Millimeters
        internal static double InToMm(double value)
        {
            return value * 25.4;
        }

        // Millimeters to Inches
        internal static double MmToIn(double value)
        {
            return value / 25.4;
        }

        // Miles to Kilometers
        internal static double MToKm(double value)
        {
            return value * 1.609344;
        }

        // Kilometers to Miles
        internal static double KmToM(double value)
        {
            return value / 1.609344;
        }

        // raise base to the given power (i.e. base**exponent
        private static double Power(double b, double exponent)
        {
            if (exponent == 0.0)
            {
                return 1.0; // n**0 = 1
            }
            else if ((b == 0.0) & (exponent > 0.0))
            {
                return 0.0; // 0**n = 0, n > 0
            }
            else
            {
                return Math.Exp(exponent * Math.Log(b));
            }
        }

        private static double Power10(double exponent)
        {
            double ln10 = 2.302585093; // Ln(10);

            if (exponent == 0.0)
            {
                return 1.0;
            }
            else
            {
                return Math.Exp(exponent * ln10);
            }
        }
    }
}
