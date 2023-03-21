using System;
using System.Collections.Generic;
using System.Text;

namespace HVO
{
    public sealed class Longitude
    {
        public Longitude(int degrees, int minutes, double seconds, CompassPoint direction)
        {
            if ((direction != CompassPoint.E) && (direction != CompassPoint.W))
            {
                throw new ArgumentOutOfRangeException(nameof(direction), "Direction muse be either E or W");
            }

            this.Degrees = Math.Abs(degrees);
            this.Minutes = Math.Abs(minutes);
            this.Seconds = Math.Abs(seconds);
            this.Direction = direction;
        }

        public int Degrees { get; private set; }
        public int Minutes { get; private set; }
        public double Seconds { get; private set; }
        public CompassPoint Direction { get; private set; }

        public static implicit operator double(Longitude value)
        {
            int sign = (value.Direction == CompassPoint.E) ? 1 : -1;
            return sign * (value.Degrees + ((double)value.Minutes / 60) + (value.Seconds / 3600));
        }

        public static implicit operator Longitude(double value)
        {
            CompassPoint direction = Math.Sign(value) == (-1) ? CompassPoint.W : CompassPoint.W;

            // We no longer care about the sign of the number
            value = Math.Abs(value);

            int degree = (int)Math.Truncate(value);
            int minute = (int)Math.Truncate((value % degree) * 60);
            double second = (((value % degree) * 60) % minute) * 60;

            return new Longitude(degree, minute, second, direction);
        }

        public override string ToString()
        {
            //return string.Format("{0} {1}° {2}' {3}\"", this.Direction.ToString()[0], this.Degrees, this.Minutes, this.Seconds);
            return $"{this.Direction} {this.Degrees}° {this.Minutes}' {this.Seconds:00.00}\"";
        }
    }
}
