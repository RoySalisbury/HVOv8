using System.ComponentModel;

namespace HVO.Weather
{
    public enum CompassPoint
    {
        [Description("N")]
        N = 0,
        [Description("NNE")]
        NNE = 22,
        [Description("NE")]
        NE = 45,
        [Description("ENE")]
        ENE = 68,
        [Description("E")]
        E = 90,
        [Description("ESE")]
        ESE = 112,
        [Description("SE")]
        SE = 135,
        [Description("SSE")]
        SSE = 158,
        [Description("S")]
        S = 180,
        [Description("SSW")]
        SSW = 202,
        [Description("SW")]
        SW = 225,
        [Description("WSW")]
        WSW = 248,
        [Description("W")]
        W = 270,
        [Description("WNW")]
        WNW = 292,
        [Description("NW")]
        NW = 315,
        [Description("NNW")]
        NNW = 338
    }
}
