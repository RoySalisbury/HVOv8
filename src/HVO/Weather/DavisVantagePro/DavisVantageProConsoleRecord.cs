using System;
using System.Runtime.Serialization;

namespace HVO.Weather.DavisVantagePro
{
    [DataContract]
    public sealed class DavisVantageProConsoleRecord
    {
        private DavisVantageProConsoleRecord(byte[] rawDataRecord, DateTimeOffset recordDateTime)
        {
            this.RawDataRecord = rawDataRecord;
            this.RecordDateTime = recordDateTime;
        }

        public static DavisVantageProConsoleRecord Create(byte[] rawDataRecord, DateTimeOffset recordDateTime, bool validateCrc = true)
        {
            if ((rawDataRecord == null) || (rawDataRecord.Length < 99))
            {
                throw new ArgumentOutOfRangeException("rawDataRecord");
            }

            // Validate the CRC
            if (validateCrc && (ValidatePacktCrc(rawDataRecord) == false))
            {
                throw new InvalidOperationException("Packet CRC is invalid");
            }


            if ((rawDataRecord[0] != 0x4C /* L */) || (rawDataRecord[1] != 0x4F /* O */) || (rawDataRecord[2] != 0x4F /* O */))
            {
                throw new ArgumentException();
            }

            // Lets get all the actual data values for everything first.  
            byte barometerTrend = rawDataRecord[3];
            byte packetType = rawDataRecord[4];
            ushort nextArchiveRecord = BitConverter.ToUInt16(rawDataRecord, 5);
            ushort barometer = BitConverter.ToUInt16(rawDataRecord, 7);
            short insideTemperature = BitConverter.ToInt16(rawDataRecord, 9);
            byte insideHumidity = rawDataRecord[11];
            short outsideTemperature = BitConverter.ToInt16(rawDataRecord, 12);
            byte windSpeed = rawDataRecord[14];
            byte tenMinuteWindSpeedAverage = rawDataRecord[15];
            ushort windDirection = BitConverter.ToUInt16(rawDataRecord, 16);
            byte extraTempStation1 = rawDataRecord[18];      // rawDataRecord[18] == byte.MaxValue ? double.MaxValue : rawDataRecord[18] - 90
            byte extraTempStation2 = rawDataRecord[19];      // same as above
            byte extraTempStation3 = rawDataRecord[20];      // same as above
            byte extraTempStation4 = rawDataRecord[21];      // same as above
            byte extraTempStation5 = rawDataRecord[22];      // same as above
            byte extraTempStation6 = rawDataRecord[23];
            byte extraTempStation7 = rawDataRecord[24];
            byte soilTempStation1 = rawDataRecord[25];
            byte soilTempStation2 = rawDataRecord[26];
            byte soilTempStation3 = rawDataRecord[27];
            byte soilTempStation4 = rawDataRecord[28];
            byte leafTempStation1 = rawDataRecord[29];
            byte leafTempStation2 = rawDataRecord[30];
            byte leafTempStation3 = rawDataRecord[31];
            byte leafTempStation4 = rawDataRecord[32];
            byte outsideHumidity = rawDataRecord[33];
            byte extraHumidityStation1 = rawDataRecord[34];  // same as above
            byte extraHumidityStation2 = rawDataRecord[35];  // same as above
            byte extraHumidityStation3 = rawDataRecord[36];  // same as above
            byte extraHumidityStation4 = rawDataRecord[37];  // same as above
            byte extraHumidityStation5 = rawDataRecord[38];  // same as above
            byte extraHumidityStation6 = rawDataRecord[39];  // same as above
            byte extraHumidityStation7 = rawDataRecord[40];  // same as above
            ushort rainRate = BitConverter.ToUInt16(rawDataRecord, 41);
            byte uvIndex = rawDataRecord[43];
            ushort solarRadiation = BitConverter.ToUInt16(rawDataRecord, 44);
            ushort stormRain = BitConverter.ToUInt16(rawDataRecord, 46);
            ushort stormStartDate = BitConverter.ToUInt16(rawDataRecord, 48);
            ushort dailyRainAmount = BitConverter.ToUInt16(rawDataRecord, 50);
            ushort monthlyRainAmount = BitConverter.ToUInt16(rawDataRecord, 52);
            ushort yearlyRainAmount = BitConverter.ToUInt16(rawDataRecord, 54);
            ushort dailyETAmount = BitConverter.ToUInt16(rawDataRecord, 56);
            ushort monthlyETAmount = BitConverter.ToUInt16(rawDataRecord, 58);
            ushort yearlyETAmount = BitConverter.ToUInt16(rawDataRecord, 60);
            byte soilMostureStation1 = rawDataRecord[62];
            byte soilMostureStation2 = rawDataRecord[63];
            byte soilMostureStation3 = rawDataRecord[64];
            byte soilMostureStation4 = rawDataRecord[65];
            byte leafWetnessStation1 = rawDataRecord[66];
            byte leafWetnessStation2 = rawDataRecord[67];
            byte leafWetnessStation3 = rawDataRecord[68];
            byte leafWetnessStation4 = rawDataRecord[69];
            byte insideAlarms = rawDataRecord[70];
            byte rainAlarms = rawDataRecord[71];
            ushort outsideAlarms = BitConverter.ToUInt16(rawDataRecord, 72);
            byte extraTempHumidityAlarm1 = rawDataRecord[74];
            byte extraTempHumidityAlarm2 = rawDataRecord[75];
            byte extraTempHumidityAlarm3 = rawDataRecord[76];
            byte extraTempHumidityAlarm4 = rawDataRecord[77];
            byte extraTempHumidityAlarm5 = rawDataRecord[78];
            byte extraTempHumidityAlarm6 = rawDataRecord[79];
            byte extraTempHumidityAlarm7 = rawDataRecord[80];
            byte extraTempHumidityAlarm8 = rawDataRecord[81];
            byte extraSoilLeafAlarm1 = rawDataRecord[82];
            byte extraSoilLeafAlarm2 = rawDataRecord[83];
            byte extraSoilLeafAlarm3 = rawDataRecord[84];
            byte extraSoilLeafAlarm4 = rawDataRecord[85];
            byte transmitterBatteryStatus = rawDataRecord[86];
            ushort consoleBatteryVoltage = BitConverter.ToUInt16(rawDataRecord, 87);
            byte forcastIcons = rawDataRecord[89];
            byte forcastRuleNumber = rawDataRecord[90];
            ushort sunriseTime = BitConverter.ToUInt16(rawDataRecord, 91);
            ushort sunsetTime = BitConverter.ToUInt16(rawDataRecord, 93);
            // LF-CR = packetData[95], packetData[96]
            ushort crcValue = BitConverter.ToUInt16(rawDataRecord, 97);

            // Helper conversion function
            Func<ushort, DateTime> dateConversion = (delegate (ushort value) {
                int year = (value & 0x007F) + 2000;   // bit  0 to  6 = year
                int day = (value & 0x0F80) >> 7;      // bit  7 to 11 = day
                int month = (value & 0xF000) >> 12;   // bit 12 to 15 = month

                return new DateTime(year, month, day);
            });

            // Helper conversion function
            Func<ushort, TimeSpan> timeConversion = (delegate (ushort value) {
                int hour = value / 100;
                int minute = value % 100;

                return new TimeSpan(hour, minute, 0);
            });

            return new DavisVantageProConsoleRecord(rawDataRecord, recordDateTime)
            {
                Barometer = barometer / 1000.0,
                BarometerTrend = (BarometerTrend)barometerTrend,
                ConsoleBatteryVoltage = ((consoleBatteryVoltage * 300) / 512) / 100.0,
                DailyETAmount = dailyETAmount / 1000.0,
                DailyRainAmount = dailyRainAmount / 100.0,
                ForcastIcons = (ForcastIcon)forcastIcons,
                InsideHumidity = insideHumidity,
                InsideTemperature = Temperature.FromFahrenheit(insideTemperature / 10.0),
                MonthlyETAmount = monthlyETAmount / 100.0,
                MonthlyRainAmount = monthlyRainAmount / 100.0,
                NextArchiveRecord = nextArchiveRecord,
                OutsideHumidity = (outsideHumidity == byte.MaxValue) ? (byte?)null : outsideHumidity,
                OutsideTemperature = (outsideTemperature == short.MaxValue) ? null : Temperature.FromFahrenheit(outsideTemperature / 10.0),
                RainRate = (rainRate == ushort.MaxValue) ? (double?)(null) : rainRate / 100.0,
                SolarRadiation = (solarRadiation == short.MaxValue) ? (ushort?)null : solarRadiation,
                StormRain = (stormRain == short.MaxValue) ? (double?)null : stormRain / 100.0,
                StormStartDate = (stormStartDate == ushort.MaxValue) ? (DateTime?)null : dateConversion(stormStartDate),

                SunriseTime = timeConversion(sunriseTime),
                SunsetTime = timeConversion(sunsetTime),
                TenMinuteWindSpeedAverage = (tenMinuteWindSpeedAverage == byte.MaxValue) ? (byte?)null : tenMinuteWindSpeedAverage,
                UVIndex = (uvIndex == byte.MaxValue) ? (byte?)null : uvIndex,
                WindDirection = (windDirection == short.MaxValue) ? (ushort?)null : windDirection,
                WindSpeed = (windSpeed == byte.MaxValue) ? (byte?)null : windSpeed,
                YearlyETAmount = yearlyETAmount / 100.0,
                YearlyRainAmount = yearlyRainAmount / 100.0
            };
        }

        public static bool ValidatePacktCrc(byte[] rawDataRecord)
        {
            using (var crc16 = new Security.Cryptography.Crc16())
            {
                ushort calculatedCrcValue = BitConverter.ToUInt16(crc16.ComputeHash(rawDataRecord, 0, 97), 0);
                ushort originalCrcValue = BitConverter.ToUInt16(rawDataRecord, 97);

                return (calculatedCrcValue == originalCrcValue);
            }
        }

        [DataMember]
        public byte[] RawDataRecord
        {
            get;
            private set;
        }

        [DataMember]
        public DateTimeOffset RecordDateTime
        {
            get;
            private set;
        }

        [DataMember]
        public BarometerTrend BarometerTrend
        {
            get;
            private set;
        }

        [DataMember]
        public ushort NextArchiveRecord
        {
            get;
            private set;
        }

        [DataMember]
        public double Barometer
        {
            get;
            private set;
        }

        [DataMember]
        public Temperature InsideTemperature
        {
            get;
            private set;
        }

        [DataMember]
        public byte InsideHumidity
        {
            get;
            private set;
        }

        [DataMember]
        public Temperature OutsideTemperature
        {
            get;
            private set;
        }

        [DataMember]
        public byte? WindSpeed
        {
            get;
            private set;
        }

        [DataMember]
        public byte? TenMinuteWindSpeedAverage
        {
            get;
            private set;
        }

        [DataMember]
        public ushort? WindDirection
        {
            get;
            private set;
        }

        [DataMember]
        public byte? OutsideHumidity
        {
            get;
            private set;
        }

        [DataMember]
        public double? RainRate
        {
            get;
            private set;
        }

        [DataMember]
        public byte? UVIndex
        {
            get;
            private set;
        }

        [DataMember]
        public ushort? SolarRadiation
        {
            get;
            private set;
        }

        [DataMember]
        public double? StormRain
        {
            get;
            private set;
        }

        [DataMember]
        public DateTime? StormStartDate
        {
            get;
            private set;
        }

        [DataMember]
        public double DailyRainAmount
        {
            get;
            private set;
        }

        [DataMember]
        public double MonthlyRainAmount
        {
            get;
            private set;
        }

        [DataMember]
        public double YearlyRainAmount
        {
            get;
            private set;
        }

        [DataMember]
        public double ConsoleBatteryVoltage
        {
            get;
            private set;
        }

        [DataMember]
        public ForcastIcon ForcastIcons
        {
            get;
            private set;
        }

        [DataMember]
        public TimeSpan SunriseTime
        {
            get;
            private set;
        }

        [DataMember]
        public TimeSpan SunsetTime
        {
            get;
            private set;
        }

        [DataMember]
        public double DailyETAmount
        {
            get;
            private set;
        }

        [DataMember]
        public double MonthlyETAmount
        {
            get;
            private set;
        }

        [DataMember]
        public double YearlyETAmount
        {
            get;
            private set;
        }

        [IgnoreDataMember]
        public Temperature OutsideHeatIndex
        {
            get
            {
                if ((OutsideTemperature != null) && (OutsideHumidity != null) && (OutsideTemperature.Fahrenheit > 80) && (OutsideHumidity > 40))
                {
                    // Rothfusz 1990 [(OutsideTemperature > 80) && (OutsideHumidity > 40)]
                    //return (-42.379) +
                    //  (2.04901523 * OutsideTemperature.Value) +
                    //  (10.14333127 * OutsideHumidity.Value) -
                    //  (0.22475541 * OutsideTemperature.Value * OutsideHumidity.Value) -
                    //  (6.83783 * (Math.Pow(10, -3)) * (Math.Pow(OutsideTemperature.Value, 2))) -
                    //  (5.481717 * (Math.Pow(10, -2)) * (Math.Pow(OutsideHumidity.Value, 2))) +
                    //  (1.22874 * (Math.Pow(10, -3)) * (Math.Pow(OutsideTemperature.Value, 2)) * OutsideHumidity.Value) +
                    //  (8.5282 * (Math.Pow(10, -4)) * OutsideTemperature.Value * (Math.Pow(OutsideHumidity.Value, 2))) -
                    //  (1.99 * (Math.Pow(10, -6)) * (Math.Pow(OutsideTemperature.Value, 2)) * (Math.Pow(OutsideHumidity.Value, 2)));

                    // Schoen 2005
                    double heatIndex = (OutsideTemperature.Fahrenheit) - (0.9971 * Math.Exp(0.020867 * (OutsideTemperature.Fahrenheit * (1 - Math.Exp(0.0445 * (OutsideDewpoint.Fahrenheit - 57.2))))));
                    //return Temperature.FromFahrenheit((heatIndex > OutsideTemperature.Fahrenheit) ? heatIndex : OutsideTemperature.Fahrenheit);

                    return Temperature.FromFahrenheit(heatIndex);
                }
                return OutsideTemperature;
            }
            private set { }
        }

        [IgnoreDataMember]
        public Temperature OutsideWindChill
        {
            get
            {
                if ((OutsideTemperature != null) && (TenMinuteWindSpeedAverage != null) /* && (OutsideTemperature < 50) */ && (TenMinuteWindSpeedAverage > 0))
                {
                    // NWS (OutsideTemperature < 50 & TenMinuteWindSpeedAverage >= 3)
                    //double? windChill = 35.74 + (0.6215 * OutsideTemperature.Value) - (35.75 * Math.Pow(TenMinuteWindSpeedAverage.Value, 0.16)) + (0.4275 * OutsideTemperature.Value * Math.Pow(TenMinuteWindSpeedAverage.Value, 0.16));

                    // Steadman revised (1998)
                    double windChill = (3.16) - (1.20 * TenMinuteWindSpeedAverage.Value) + (0.980 * OutsideTemperature.Fahrenheit) + (0.0044 * Math.Pow(TenMinuteWindSpeedAverage.Value, 2)) + (0.0083 * (TenMinuteWindSpeedAverage.Value * OutsideTemperature.Fahrenheit));
                    return Temperature.FromFahrenheit((windChill < OutsideTemperature.Fahrenheit) ? windChill : OutsideTemperature.Fahrenheit);
                }
                return OutsideTemperature;
            }
            private set { }
        }

        [IgnoreDataMember]
        public Temperature OutsideDewpoint
        {
            get
            {
                // For humidity below <= 0%
                double outsideHumidity = 0.1;
                if ((OutsideHumidity != null) || (OutsideHumidity > 0))
                {
                    outsideHumidity = (double)OutsideHumidity.Value;
                }

                if ((OutsideTemperature != null) && (OutsideHumidity != null) && (OutsideHumidity > 0))
                {
                    // Magnus–Tetens formula (Barenbrug 1974)
                    var z1 = ((17.27 * OutsideTemperature.Celsius) / (237.7 + OutsideTemperature.Celsius)) + Math.Log(outsideHumidity / 100);
                    return Temperature.FromCelsius((237.7 * z1) / (17.27 - z1));
                }
                return null;
            }
            private set { }
        }
    }
}
