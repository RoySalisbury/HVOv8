using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HVO.Hardware.RoofControllerV3
{
    public class RoofControllerOptions
    {
        public int CloseRoofRelayPin { get; set; } = 23; // FORWARD
        public int OpenRoofRelayPin { get; set; } = 24; // REVERSE
        public int StopRoofRelayPin { get; set; } = 25;
        public int KeypadEnableRelayPin { get; set; } = 26;

        public int RoofClosedLimitSwitchPin { get; set; } = 4;
        public int RoofOpenedLimitSwitchPin { get; set; } = 5;
        public int LimitSwitch3 { get; set; } = 6;

        public int CloseRoofButtonPin { get; set; } = 7;
        public int OpenRoofButtonPin { get; set; } = 8;
        public int StopRoofButtonPin { get; set; } = 9;
    }
}
