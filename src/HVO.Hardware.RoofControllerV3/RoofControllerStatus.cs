using System.Text.Json.Serialization;

namespace HVO.Hardware.RoofControllerV3
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RoofControllerStatus
    {
        Unknown = 0,
        NotInitialized = 1,
        Closed = 2,
        Closing = 3,
        Open = 4,
        Opening = 5,
        Stopped = 6,
        Error = 99
    }
}