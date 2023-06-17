using System.Text;

namespace HVO.JKBmsMonitor
{
    public class JkBmsGetDeviceSettingsResponse : JkBmsResponse
    {
        public JkBmsGetDeviceSettingsResponse(ReadOnlyMemory<byte> data) : base(data)
        {
            this.InitializeFromPayload();
        }

        public int Unknown6 { get; private set; }
        public int CellUnderVoltageProtection { get; private set; }
        public int CellUnderVoltageProtectionRecovery { get; private set; }
        public int CellOverVoltageProtection { get; private set; }
        public int CellOverVoltageProtectionRecovery { get; private set; }
        public int BalanceTriggerVoltage { get; private set; }
        public int PowerOffVoltage { get; private set; }
        public int MaxChargeCurrent { get; private set; }
        public int ChargeOverCurrentProtectionDelay { get; private set; }
        public int ChargeOverCurrentProtectionDelayRecovery { get; private set; }
        public int MaxDischargeCurrent { get; private set; }
        public int DischargeOverCurrentProtectionDelay { get; private set; }
        public int DischargeOverCurrentProtectionDelayRecovery { get; private set; }
        public int SCPRTime { get; private set; }
        public int MaxBalanceCurrent { get; private set; }
        public int ChargeOTP { get; private set; }
        public int ChargeOTPRecovery { get; private set; }
        public int DischargeOTP { get; private set; }
        public int DischargeOTPRecovery { get; private set; }
        public int ChargeUTP { get; private set; }
        public int ChargeUTPRecovery { get; private set; }
        public int MOSOTP { get; private set; }
        public int MOSOTPRecovery { get; private set; }
        public int CellCount { get; private set; }
        public int ChargeSwitch { get; private set; }
        public int DischargeSwitch { get; private set; }
        public int BalancerSwitch { get; private set; }
        public int NominalBatteryCapacity { get; private set; }
        public int Unknown134 { get; private set; }
        public int StartBalanceVoltage { get; private set; }

        protected override void InitializeFromPayload()
        {
            var payload = this.Payload.Span;

            // 6     4   0x58 0x02 0x00 0x00    Unknown6
            this.Unknown6 = BitConverter.ToInt32(payload.Slice(6, 4));

            // 10    4   0x54 0x0B 0x00 0x00    Cell UVP
            this.CellUnderVoltageProtection = BitConverter.ToInt32(payload.Slice(10, 4));

            // 14    4   0x80 0x0C 0x00 0x00    Cell OVP Recovery
            this.CellUnderVoltageProtectionRecovery = BitConverter.ToInt32(payload.Slice(14, 4));

            // 18    4   0xCC 0x10 0x00 0x00    Cell OVP
            this.CellOverVoltageProtection = BitConverter.ToInt32(payload.Slice(18, 4));

            // 22    4   0x68 0x10 0x00 0x00    Cell OVP Recovery
            this.CellOverVoltageProtectionRecovery = BitConverter.ToInt32(payload.Slice(22, 4));

            // 26    4   0x0A 0x00 0x00 0x00    Balance trigger voltage
            this.BalanceTriggerVoltage = BitConverter.ToInt32(payload.Slice(26, 4));

            // 30    4   0x00 0x00 0x00 0x00    Unknown30
            // 34    4   0x00 0x00 0x00 0x00    Unknown34
            // 38    4   0x00 0x00 0x00 0x00    Unknown38
            // 42    4   0x00 0x00 0x00 0x00    Unknown42

            // 46    4   0xF0 0x0A 0x00 0x00    Power off voltage
            this.PowerOffVoltage = BitConverter.ToInt32(payload.Slice(46, 4));

            // 50    4   0xA8 0x61 0x00 0x00    Max. charge current
            this.MaxChargeCurrent = BitConverter.ToInt32(payload.Slice(50, 4));

            // 54    4   0x1E 0x00 0x00 0x00    Charge OCP delay
            this.ChargeOverCurrentProtectionDelay = BitConverter.ToInt32(payload.Slice(54, 4));

            // 58    4   0x3C 0x00 0x00 0x00    Charge OCP recovery delay
            this.ChargeOverCurrentProtectionDelayRecovery = BitConverter.ToInt32(payload.Slice(58, 4));

            // 62    4   0xF0 0x49 0x02 0x00    Max. discharge current
            this.MaxDischargeCurrent = BitConverter.ToInt32(payload.Slice(62, 4));

            // 66    4   0x2C 0x01 0x00 0x00    Discharge OCP delay
            this.DischargeOverCurrentProtectionDelay = BitConverter.ToInt32(payload.Slice(66, 4));

            // 70    4   0x3C 0x00 0x00 0x00    Discharge OCP recovery delay
            this.DischargeOverCurrentProtectionDelayRecovery = BitConverter.ToInt32(payload.Slice(70, 4));

            // 74    4   0x3C 0x00 0x00 0x00    SCPR time
            this.SCPRTime = BitConverter.ToInt32(payload.Slice(74, 4));

            // 78    4   0xD0 0x07 0x00 0x00    Max balance current
            this.MaxBalanceCurrent = BitConverter.ToInt32(payload.Slice(78, 4));

            // 82    4   0xBC 0x02 0x00 0x00    Charge OTP
            this.ChargeOTP = BitConverter.ToInt32(payload.Slice(82, 4));

            // 86    4   0x58 0x02 0x00 0x00    Charge OTP Recovery
            this.ChargeOTPRecovery = BitConverter.ToInt32(payload.Slice(86, 4));

            // 90    4   0xBC 0x02 0x00 0x00    Discharge OTP
            this.DischargeOTP = BitConverter.ToInt32(payload.Slice(90, 4));

            // 94    4   0x58 0x02 0x00 0x00    Discharge OTP Recovery
            this.DischargeOTPRecovery = BitConverter.ToInt32(payload.Slice(94, 4));

            // 98    4   0x38 0xFF 0xFF 0xFF    Charge UTP
            this.ChargeUTP = BitConverter.ToInt32(payload.Slice(98, 4));

            // 102   4   0x9C 0xFF 0xFF 0xFF    Charge UTP Recovery
            this.ChargeUTPRecovery = BitConverter.ToInt32(payload.Slice(102, 4));

            // 106   4   0x84 0x03 0x00 0x00    MOS OTP
            this.MOSOTP = BitConverter.ToInt32(payload.Slice(106, 4));

            // 110   4   0xBC 0x02 0x00 0x00    MOS OTP Recovery
            this.MOSOTPRecovery = BitConverter.ToInt32(payload.Slice(110, 4));

            // 114   4   0x0D 0x00 0x00 0x00    Cell count
            this.CellCount = BitConverter.ToInt32(payload.Slice(114, 4));

            // 118   4   0x01 0x00 0x00 0x00    Charge switch
            this.ChargeSwitch = BitConverter.ToInt32(payload.Slice(118, 4));

            // 122   4   0x01 0x00 0x00 0x00    Discharge switch
            this.DischargeSwitch = BitConverter.ToInt32(payload.Slice(122, 4));

            // 126   4   0x01 0x00 0x00 0x00    Balancer switch
            this.BalancerSwitch = BitConverter.ToInt32(payload.Slice(126, 4));

            // 130   4   0x88 0x13 0x00 0x00    Nominal battery capacity
            this.NominalBatteryCapacity = BitConverter.ToInt32(payload.Slice(130, 4));

            // 134   4   0xDC 0x05 0x00 0x00    Unknown134
            this.Unknown134 = BitConverter.ToInt32(payload.Slice(134, 4));

            // 138   4   0xE4 0x0C 0x00 0x00    Start balance voltage
            this.StartBalanceVoltage = BitConverter.ToInt32(payload.Slice(138, 4));
        }
    }
}
