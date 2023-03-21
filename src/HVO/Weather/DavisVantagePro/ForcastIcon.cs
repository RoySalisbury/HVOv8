using System.ComponentModel;

namespace HVO.Weather.DavisVantagePro
{
    public enum ForcastIcon
    {
        [Description("Mostly Cloudy")]
        Cloud = 2,

        [Description("Mostly Cloudy, Rain within 12 hours.")]
        CloudRain = 3,

        [Description("Partially Cloudy")]
        PartialSunCloud = 6,

        [Description("Partially Cloudy, Rain within 12 hours.")]
        PartialSunCloudRain = 7,

        [Description("Mostly Clear")]
        Sun = 8,

        [Description("Mostly Cloudy, Snow within 12 hours.")]
        CloudSnow = 18,

        [Description("Mostly Cloudy, Rain or Snow within 12 hours.")]
        CloudRainSnow = 19,

        [Description("Partially Cloudy, Snow within 12 hours.")]
        PartialSunCloudSnow = 22,

        [Description("Partially Cloudy, Rain or Snow within 12 hours.")]
        PartialSunCloudRainSnow = 23
    }
}
