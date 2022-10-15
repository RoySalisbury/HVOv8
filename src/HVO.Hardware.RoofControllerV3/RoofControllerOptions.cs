using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.Hardware.RoofControllerV3
{
    public class RoofControllerOptions
    {
        public uint CloseRoofRelayPin { get; set; } = 6; // FORWARD
        public uint OpenRoofRelayPin { get; set; } = 13; // REVERSE
        public uint StopRoofRelayPin { get; set; } = 19;
        public uint KeypadEnableRelayPin { get; set; } = 26;
        public uint RoofClosedLimitSwitchPin { get; set; } = 23;
        public uint RoofOpenedLimitSwitchPin { get; set; } = 24;
        public uint StopRoofButtonPin { get; set; } = 16;
        public uint OpenRoofButtonPin { get; set; } = 20;
        public uint CloseRoofButtonPin { get; set; } = 21;
    }
}
