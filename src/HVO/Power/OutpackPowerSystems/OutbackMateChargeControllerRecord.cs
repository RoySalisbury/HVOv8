namespace HVO.Power.OutbackPowerSystems {
  using System;
  using System.Runtime.Serialization;
  
  [DataContract]
  public sealed class OutbackMateChargeControllerRecord : OutbackMateRecord {

    internal OutbackMateChargeControllerRecord(DateTimeOffset recordDateTime, byte hubPort, short pvAmps, short pvVoltage, decimal chargerAmps, decimal chargerVoltage, short dailyAmpHoursProduced, int dailyWattHoursProduced, OutbackMateChargeControllerMode mode, OutbackMateChargeControllerAuxRelayMode auxRelayMode, OutbackMateChargeControllerErrorMode errorMode, string rawData)
      : base(OutbackMateRecordType.ChargeController, recordDateTime, hubPort, rawData) {

      this.PVAmps = pvAmps;
      this.PVVoltage = pvVoltage;
      this.ChargerAmps = chargerAmps;
      this.ChargerVoltage = chargerVoltage;
      this.DailyAmpHoursProduced = dailyAmpHoursProduced;
      this.DailyWattHoursProduced = dailyWattHoursProduced;
      this.Mode = mode;
      this.AuxRelayMode = auxRelayMode;
      this.ErrorMode = errorMode;
    }

    [DataMember]
    public short PVAmps { get; private set; }
    [DataMember]
    public short PVVoltage { get; private set; }

    [DataMember]
    public decimal ChargerAmps { get; private set; }
    [DataMember]
    public decimal ChargerVoltage { get; private set; }

    [DataMember]
    public short DailyAmpHoursProduced { get; private set; }
    [DataMember]
    public int DailyWattHoursProduced { get; private set; }

    [DataMember]
    public OutbackMateChargeControllerMode Mode { get; private set; }
    [DataMember]
    public OutbackMateChargeControllerAuxRelayMode AuxRelayMode { get; private set; }
    [DataMember]
    public OutbackMateChargeControllerErrorMode ErrorMode { get; private set; }

  }
}
