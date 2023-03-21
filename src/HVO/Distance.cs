namespace HVO
{
    public sealed class Distance
    {
        private const double MetersPerInch = 0.0254;

        private const int InchesPerFoot = 12;
        private const int FeetPerMile = 5280;

        private const int CentimetersPerMeter = 100;
        private const int MetersPerKilometer = 1000;

        private Distance() { }

        public static Distance FromCentimeters(double value)
        {
            return new Distance() { Centimeters = value };
        }

        public static Distance FromMeters(double value)
        {
            return new Distance() { Meters = value };
        }

        public static Distance FromKilometers(double value)
        {
            return new Distance() { Kilometers = value };
        }

        public static Distance FromInches(double value)
        {
            return new Distance() { Inches = value };
        }

        public static Distance FromFeet(double value)
        {
            return new Distance() { Feet = value };
        }

        public static Distance FromMiles(double value)
        {
            return new Distance() { Miles = value };
        }

        public double Kilometers
        {
            get
            {
                return Meters / MetersPerKilometer;
            }

            set
            {
                Meters = value * MetersPerKilometer;
            }
        }

        public double Centimeters
        {
            get
            {
                return Meters * CentimetersPerMeter;
            }

            set
            {
                Meters = value / CentimetersPerMeter;
            }
        }

        // Internally, everything is stored as meters
        public double Meters { get; set; }

        public double Inches
        {
            get
            {
                return this.Meters / MetersPerInch;
            }

            set
            {
                this.Meters = value * MetersPerInch;
            }
        }

        public double Feet
        {
            get
            {
                return (this.Meters / MetersPerInch) / InchesPerFoot;
            }

            set
            {
                this.Meters = (value * InchesPerFoot) * MetersPerInch;
            }
        }

        public double Miles
        {
            get
            {
                return ((this.Meters / MetersPerInch) / InchesPerFoot) / FeetPerMile;
            }

            set
            {
                this.Meters = (value * FeetPerMile * InchesPerFoot) * MetersPerInch;
            }
        }
    }
}
