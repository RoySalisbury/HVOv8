using System.ComponentModel;

namespace HVO.Weather.DavisVantagePro
{
    public enum BarometerTrend : short
    {
        [Description("")]
        Unknown = 80,

        [Description("")]
        Unavailable = short.MaxValue,

        [Description("&dArr;")]
        FallingRapidly = 196, // -60

        [Description("&darr;")]
        FallingSlowly = 236,  // -20

        [Description("&harr;")]
        Steady = 0,

        [Description("&uarr;")]
        RisingSlowly = 20,

        [Description("&uArr;")]
        RisingRapidly = 60
    }
}
