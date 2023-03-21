using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace HVO
{
    public sealed class RightAscension
    {
        private readonly TimeSpan _rightAscension;

        private RightAscension(TimeSpan timespan)
        {
            this._rightAscension = timespan;
        }

        public RightAscension(int hours, int minutes, double seconds)
        {
            if (hours < 0 || hours > 23)
            {
                throw new ArgumentOutOfRangeException(nameof(hours), "Hours must be between 0 and 23");
            }

            if (minutes < 0 || minutes > 59)
            {
                throw new ArgumentOutOfRangeException(nameof(minutes), "Minutes must be between 0 and 59");
            }

            if ((seconds < 0) || ((seconds < 60) == false))
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Seconds must be greater than 0 and less than 60.");
            }

            this._rightAscension = TimeSpan.FromSeconds(seconds + (minutes * 60) + (hours * 3600));
        }

        public static RightAscension FromTimeSpan(TimeSpan timespan)
        {
            return new RightAscension(timespan);
        }

        public static RightAscension FromDegrees(double degrees)
        {
            return new RightAscension(TimeSpan.FromHours(Math.Abs(degrees) / 15.0));
        }

        public static RightAscension FromHours(double totalhours)
        {
            return new RightAscension(TimeSpan.FromHours(totalhours));
        }

        public int Hours => this._rightAscension.Hours;

        public int Minutes => this._rightAscension.Minutes;

        public double Seconds => Math.Round((this._rightAscension.TotalMinutes - Math.Truncate(this._rightAscension.TotalMinutes)) * 60, 4);

        public double Degrees => this._rightAscension.TotalHours * 15;

        public override string ToString()
        {
            return this._rightAscension.ToString("h'h 'm'm 's'.'ffff's'", CultureInfo.InvariantCulture);
        }

        public TimeSpan ToTimeSpan() => this._rightAscension;
    }
}
