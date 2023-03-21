using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace HVO.Power.OutbackPowerSystems
{
  [DataContract]
  public sealed class OutbackMateInverterChargerRecord : OutbackMateRecord
  {
    public OutbackMateInverterChargerRecord(DateTimeOffset recordDateTime, byte hubPort, byte inverterCurrent, byte chargerCurrent,
      byte buyCurrent, ushort acInputVoltage, ushort acOutputVoltage, byte sellCurrent,
      OutbackFxInverterChargerOperationalMode operationalMode,
      OutbackFxInverterChargerErrorMode errorMode,
      OutbackFxInverterChargerACInputMode acInputMode,
      decimal batteryVoltage,
      OutbackFxInverterChargerMisc misc,
      OutbackFxInverterChargerWarningMode warningMode,
      string rawRecord)
      : base(OutbackMateRecordType.InverterCharger, recordDateTime, hubPort, rawRecord)
    {
      this.InverterCurrent = inverterCurrent;
      this.ChargerCurrent = chargerCurrent;
      this.BuyCurrent = buyCurrent;
      this.ACInputVoltage = acInputVoltage;
      this.ACOutputVoltage = acOutputVoltage;
      this.SellCurrent = sellCurrent;
      this.OperationalMode = operationalMode;
      this.ErrorMode = errorMode;
      this.ACInputMode = acInputMode;
      this.BatteryVoltage = batteryVoltage;
      this.Misc = misc;
      this.WarningMode = warningMode;
    }

    [DataMember]
    public byte InverterCurrent { get; private set; }

    [DataMember]
    public byte ChargerCurrent { get; private set; }
    
    [DataMember]
    public byte BuyCurrent { get; private set; }
    
    [DataMember]
    public ushort ACInputVoltage { get; private set; }
    
    [DataMember]
    public ushort ACOutputVoltage { get; private set; }
    
    [DataMember]
    public byte SellCurrent { get; private set; }
    
    [DataMember]
    public OutbackFxInverterChargerOperationalMode OperationalMode { get; private set; }
    
    [DataMember]
    public OutbackFxInverterChargerErrorMode ErrorMode { get; private set; }
    
    [DataMember]
    public OutbackFxInverterChargerACInputMode ACInputMode { get; private set; }
    
    [DataMember]
    public decimal BatteryVoltage { get; private set; }
    
    [DataMember]
    public OutbackFxInverterChargerMisc Misc { get; private set; }

    [DataMember]
    public OutbackFxInverterChargerWarningMode WarningMode { get; private set; }
  }
}
