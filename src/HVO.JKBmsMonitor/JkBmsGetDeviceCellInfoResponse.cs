using System.Collections;

namespace HVO.JKBmsMonitor
{
    public class JkBmsGetDeviceCellInfoResponse : JkBmsResponse
    {
        public JkBmsGetDeviceCellInfoResponse() : base() { }

        protected override void InitializeFromPayload(ReadOnlySpan<byte> payload)
        {
            base.InitializeFromPayload(payload);

            #region Packet Specs
            // (0) 55-AA-EB-90  Header
            // (4) 02           FrameType
            // (5) 44           FrameNumber

            // (6) 2E-0E   CellVoltage01
            // (8) 2F-0E   CellVoltage02
            // (10) 2D-0E   CellVoltage03
            // (12) 2F-0E   CellVoltage04
            // (14) 2F-0E   CellVoltage05
            // (16) 2F-0E   CellVoltage06
            // (18) 2F-0E   CellVoltage07
            // (20) 2F-0E   CellVoltage08
            // (22) 2E-0E   CellVoltage08
            // (24) 2E-0E   CellVoltage11
            // (26) 2F-0E   CellVoltage11
            // (28) 2F-0E   CellVoltage12
            // (30) 2F-0E   CellVoltage13
            // (32) 2E-0E   CellVoltage14
            // (34) 2F-0E   CellVoltage15
            // (36) 2E-0E   CellVoltage16
            // (38) 00-00   CellVoltage17
            // (40) 00-00   CellVoltage18
            // (42) 00-00   CellVoltage19
            // (44) 00-00   CellVoltage20
            // (46) 00-00   CellVoltage21
            // (48) 00-00   CellVoltage22
            // (50) 00-00   CellVoltage23
            // (52) 00-00   CellVoltage24
            //              ... Can go to 32 cells with offset on some devices ...
            //
            // (54) FF-FF-00-00  EnabledCellsBitmask
            // (58) 2E-0E  Average Cell Voltage
            // (60) 02-00  delta Cell Voltage
            // (62) 01  Max Voltage Cell Index
            // (63) 02  Min Voltage Cell Index

            // (64)  37-00  CellResistance01
            // (66)  37-00  CellResistance02
            // (68)  35-00  CellResistance03
            // (70)  34-00  CellResistance04
            // (72)  34-00  CellResistance05
            // (74)  34-00  CellResistance06
            // (76)  34-00  CellResistance07
            // (78)  35-00  CellResistance08
            // (80)  35-00  CellResistance09
            // (82)  36-00  CellResistance10
            // (84)  39-00  CellResistance11
            // (86)  38-00  CellResistance12
            // (88)  3B-00  CellResistance13
            // (90)  3A-00  CellResistance14
            // (92)  34-00  CellResistance15
            // (94)  35-00  CellResistance16
            // (96)  00-00  CellResistance17
            // (98)  00-00  CellResistance18
            // (100)  00-00  CellResistance19
            // (102)  00-00  CellResistance20
            // (104)  00-00  CellResistance21
            // (106)  00-00  CellResistance22
            // (108)  00-00  CellResistance23
            // (110)  00-00  CellResistance24
            //              ... Can go to 32 cells with offset on some devices ...

            // OffsetStart


            // (112)  00-00  Power Tube Temperature
            // (114)  00-00-00-00   Wire resistance warning bitmask

            // (118)  E4-E2-00-00   Battery Voltage
            // (122)  00-00-00-00   Battery Power
            // (126)  00-00-00-00   Charge Current

            // (130)  06-01  Temp Sensor 1
            // (132)  01-01  Temp Sensor 2
            // (134)  03-01  Power Tube Temp Sensor -OR- Errors Bitmask
            //
            // (136)  00-00  System Alarms Bitmask
            // (138)  00-00  Balance Current
            // (140)  00     Bslance Action (0 = off, 1 = Charging, 2 = Discharging) 
            // (141)  62     State of Charge
            // (142)  C5-3A-04-00 Capacity Remaining
            // (146)  C0-45-04-00 Nominal Capacity
            // (150)  20-00-00-00 Cycle Count
            // (154)  05-98-89-00 Cycle Capacity
            // (158)  64-00
            // (160)  6A-0A
            // (162)  88-AF-7B-01 Total runtime in seconds
            // (166)  01    Charge Mosfet Enabled (0, 1)
            // (167)  01    Discharge Mosfet enabled (0, 1)
            // (168)  6B
            // (169)  06-00
            // (171)  00-00
            // (173)  00-00
            // (175)  00-00
            // (177)  00-00
            // (179)  00-00
            // (181)  00-07
            // (183)  00-01
            // (185)  00-00
            // (187)  00-C1
            // (189)  03-00
            // (190)  00
            // (191)  00
            // (192)  00
            // (193)  28-C5
            // (195)  3E-40
            // (197)  00-00-00-00-E2-04-00-00-00-00
            // (207)  00-01-00-05-00-00-E9
            // (214)  25-56-08-00 Uptime 100ms
            // (218)  00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00-00
            // (299)  3E    CRC
            #endregion

            // The offset is based on the device firmware version of this payload
            int offset = 0;
            int numberOfCells = 24 + (offset / 2);

            this.CellVoltages = new  ushort[numberOfCells];
            this.CellResistance = new ushort[numberOfCells];

            for (int i = 0; i < numberOfCells; i++)
            {
                this.CellVoltages[i] = BitConverter.ToUInt16(payload.Slice((i * 2) + 6, 2));
                this.CellResistance[i] = BitConverter.ToUInt16(payload.Slice((i * 2) + 64 + offset, 2));
            }

            this.AverageCellVoltage = BitConverter.ToUInt16(payload.Slice((58 + offset), 2));
            this.DeltaCellVoltage = BitConverter.ToUInt16(payload.Slice((60 + offset), 2));

            this.MaxCellVoltageIndex = payload[62];
            this.MinCellVoltageIndex = payload[63];

            offset = offset * 2;

            var powerTubeTemperature = BitConverter.ToInt16(payload.Slice(112 + offset, 2));

            this.WireResistanceWarnings = new BitArray(payload.Slice(114 + offset, 4).ToArray());

            this.BatteryVoltage = BitConverter.ToUInt32(payload.Slice(118 + offset, 4));
            this.BatteryPower = BitConverter.ToUInt32(payload.Slice(122 + offset, 4)); // WARNING: Unsigned. Calculate manually (v*c)
            this.ChargeCurrent = BitConverter.ToUInt32(payload.Slice(126 + offset, 4));
        }

        public ushort[] CellVoltages { get; private set; }
        public ushort[] CellResistance { get; private set; }
        public ushort AverageCellVoltage { get; private set; }
        public ushort DeltaCellVoltage { get; private set; }
        public byte MaxCellVoltageIndex { get; private set; }
        public byte MinCellVoltageIndex { get; private set; }

        public BitArray WireResistanceWarnings {  get; private set; }
        public uint BatteryVoltage { get; private set; }
        public uint BatteryPower { get; private set; }
        public uint ChargeCurrent { get; private set; }
    }



}
