namespace HVO.Weather
{
    public sealed class Distance
    {
        private Distance() { }

        public static Distance FromMeters(double value)
        {
            return new Distance() { Meters = value };
        }

        public static Distance FromFeet(double value)
        {
            return new Distance() { Feet = value };
        }

        public double Kilometers
        {
            get
            {
                return Meters / 1000;
            }

            set
            {
                Meters = value * 1000;
            }
        }

        public double Centimeters
        {
            get
            {
                return Meters * 100;
            }

            set
            {
                Meters = value / 10;
            }
        }

        public double Meters { get; set; }

        public double Inches
        {
            get
            {
                return Feet * 12;
            }

            set
            {
                Feet = value / 12;
            }
        }

        public double Feet
        {
            get
            {
                return Meters * 3.28084;
            }

            set
            {
                Meters = value / 3.28084;
            }
        }

        public double Miles
        {
            get
            {
                return Feet / 5280;
            }

            set
            {
                Feet = value * 5280;
            }
        }
    }
}
