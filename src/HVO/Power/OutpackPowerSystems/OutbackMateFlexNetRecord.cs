namespace HVO.Power.OutbackPowerSystems {
  using System;
  using System.Runtime.Serialization;

  [DataContract]
  public sealed class OutbackMateFlexNetRecord : OutbackMateRecord {

    internal OutbackMateFlexNetRecord(DateTimeOffset recordDateTime, byte hubPort, decimal batteryVoltage, byte batteryStateOfCharge, short? batteryTempatureC, 
      bool chargeParamsMet, OutbackMateFlexNetRelayState relayState, OutbackMateFlexNetRelayMode relayMode, 
      bool shuntAEnabled, decimal shuntAAmps, bool shuntBEnabled, decimal shuntBAmps, bool shuntCEnabled, decimal shuntCAmps,
      OutbackMateFlexNetExtraValueType? extraValueType, decimal? extraValue, string rawData)
      : base(OutbackMateRecordType.FlexNetDC, recordDateTime, hubPort, rawData) {

      this.BatteryVoltage = batteryVoltage;
      this.BatteryStateOfCharge = batteryStateOfCharge;
      this.BatteryTempatureC = batteryTempatureC;

      this.ChargeParamsMet = chargeParamsMet;
      this.RelayState = relayState;
      this.RelayMode = relayMode;

      this.ShuntAEnabled = shuntAEnabled;
      this.ShuntAAmps = shuntAAmps;
      this.ShuntBEnabled = shuntBEnabled;
      this.ShuntBAmps = shuntBAmps;
      this.ShuntCEnabled = shuntCEnabled;
      this.ShuntCAmps = shuntCAmps;

      this.ExtraValueType = extraValueType;
      this.ExtraValue = extraValue;
    }

    [DataMember]
    public decimal BatteryVoltage { get; private set; }
    [DataMember]
    public byte BatteryStateOfCharge { get; private set; }
    [DataMember]
    public short? BatteryTempatureC { get; private set; }

    [DataMember]
    public bool ChargeParamsMet { get; private set; }
    [DataMember]
    public OutbackMateFlexNetRelayState RelayState { get; private set; }
    [DataMember]
    public OutbackMateFlexNetRelayMode RelayMode { get; private set; }

    [DataMember]
    public bool ShuntAEnabled { get; private set; }
    [DataMember]
    public decimal ShuntAAmps { get; private set; }
    [DataMember]
    public bool ShuntBEnabled { get; private set; }
    [DataMember]
    public decimal ShuntBAmps { get; private set; }
    [DataMember]
    public bool ShuntCEnabled { get; private set; }
    [DataMember]
    public decimal ShuntCAmps { get; private set; }

    [DataMember]
    public OutbackMateFlexNetExtraValueType? ExtraValueType { get; private set; }
    [DataMember]
    public decimal? ExtraValue { get; private set; }

  }
}
