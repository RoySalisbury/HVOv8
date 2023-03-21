namespace HVO.Power.OutbackPowerSystems {
  using System;
  using System.Linq;
  using System.Runtime.Serialization;
  
  [DataContract]
  public abstract class OutbackMateRecord : IOutbackMateRecord, ICloneable {
    [Flags]
    private enum FlexNetStatus {
      ChargeParamsMet = 1,
      RelayStateOpen = 2,
      RelayModeAutomatic = 4,
      ShuntAValueNegative = 8,
      ShuntBValueNegative = 16,
      ShuntCValueNegative = 32
    }

    private enum FlexNetExtraInfoType {
      AccumulatedShuntAAmpHours = 0,
      AccumulatedShuntAWattHours = 1,
      AccumulatedShuntBAmpHours = 2,
      AccumulatedShuntBWattHours = 3,
      AccumulatedShuntCAmpHours = 4,
      AccumulatedShuntCWattHours = 5,
      DaysSinceFull = 6,
      TodaysMinimumSOC = 7,
      TodaysNetInputAmpHours = 8,
      TodaysNetOutputAmpHours = 9,
      TodaysNetInputWattHours = 10,
      TodaysNetOutputWattHours = 11,
      ChargeFactorCorrectedNetAmpHours = 12,
      ChargeFactorCorrectedNetWattHours = 13
    }

    private OutbackMateRecordType _recordType;
    private string _rawData;

    protected OutbackMateRecord(OutbackMateRecordType recordType, DateTimeOffset recordDateTime, byte hubPort, string rawData) {
      this._recordType = recordType;
      this.RecordDateTime = recordDateTime;
      this.HubPort = hubPort;
      this._rawData = rawData;
    }

    /// <summary>
    /// Parse the raw data and create the specific record type instance
    /// </summary>
    /// <param name="recordDateTime"></param>
    /// <param name="rawRecord"></param>
    /// <returns></returns>
    public static IOutbackMateRecord Create(DateTimeOffset recordDateTime, string rawRecord)
    {
      if (string.IsNullOrWhiteSpace(rawRecord)) {
        throw new ArgumentNullException("rawRecord");
      }

      // Split the rawRecord into its distinct data parts
      string[] rawRecordParts = rawRecord.Split(',');

      #region OutbackMateChargeControllerRecord
      // The first part is the type of record that we are creating.
      if ("ABCDEFGHIJK".Contains(rawRecordParts[0])) {
        byte hubPort = Convert.ToByte(((byte)rawRecordParts[0][0]) - 65);
        int unused1 = Convert.ToInt32(rawRecordParts[1]);

        var pvAmps = Convert.ToInt16(rawRecordParts[3]);
        var pvVoltage = Convert.ToInt16(rawRecordParts[4]);

        decimal chargerAmps = Convert.ToDecimal(rawRecordParts[2]) + (Convert.ToDecimal(rawRecordParts[6]) / 10);
        decimal chargerVoltage = Convert.ToDecimal(rawRecordParts[10]) / 10;

        short dailyAmpHoursProduced = Convert.ToInt16(rawRecordParts[11]);
        int dailyWattHoursProduced = Convert.ToInt32(rawRecordParts[5]) * 100;

        byte chargerAuxRelayMode = Convert.ToByte(rawRecordParts[7]);
        short chargerErrorMode = Convert.ToInt16(rawRecordParts[8]);
        byte chargerMode = Convert.ToByte(rawRecordParts[9]);

        int unused2 = Convert.ToInt32(rawRecordParts[12]);
        int crc = Convert.ToInt32(rawRecordParts[13]);

        // Check the CRC of the packet to make sure that it is correct

        // return the new record
        return new OutbackMateChargeControllerRecord(recordDateTime, hubPort, pvAmps, pvVoltage, chargerAmps, chargerVoltage, dailyAmpHoursProduced, dailyWattHoursProduced,
          (OutbackMateChargeControllerMode)chargerMode,
          (OutbackMateChargeControllerAuxRelayMode)chargerAuxRelayMode,
          (OutbackMateChargeControllerErrorMode)chargerErrorMode,
          rawRecord);
      }
      #endregion

      #region OutbackMateFlexNetRecord
      if ("abcdefghij".Contains(rawRecordParts[0])) {
        byte hubPort = Convert.ToByte(((byte)rawRecordParts[0][0]) - 98);

        decimal shuntAAmps = Convert.ToDecimal(rawRecordParts[1]) / 10;
        decimal shuntBAmps = Convert.ToDecimal(rawRecordParts[2]) / 10;
        decimal shuntCAmps = Convert.ToDecimal(rawRecordParts[3]) / 10;

        decimal batteryVoltage = Convert.ToDecimal(rawRecordParts[6]) / 10;
        byte batteryStateOfCharge = Convert.ToByte(rawRecordParts[7]);

        bool shuntAEnabled = rawRecordParts[8][0] == '0';
        bool shuntBEnabled = rawRecordParts[8][1] == '0';
        bool shuntCEnabled = rawRecordParts[8][2] == '0';

        short? batteryTempatureC = Convert.ToInt16(rawRecordParts[10]);
        if (batteryTempatureC != 99) {
          batteryTempatureC -= 10;
        }
        else {
          batteryTempatureC = null;
        }

        bool isExtraValueNegative = (Convert.ToByte(rawRecordParts[4]) & 0x40) == 0x40;
        FlexNetExtraInfoType flexNetExtraInfoType = (FlexNetExtraInfoType)(Convert.ToInt32(rawRecordParts[4]) & 0x3F);

        OutbackMateFlexNetExtraValueType? extraValueType = null;
        decimal? extraValue = null;

        switch (flexNetExtraInfoType) {
          case FlexNetExtraInfoType.AccumulatedShuntAAmpHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntAAmpHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1);
            break;
          }
          case FlexNetExtraInfoType.AccumulatedShuntAWattHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntAWattHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1) * 10;
            break;
          }
          case FlexNetExtraInfoType.AccumulatedShuntBAmpHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntBAmpHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1);
            break;
          }
          case FlexNetExtraInfoType.AccumulatedShuntBWattHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntBWattHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1) * 10;
            break;
          }
          case FlexNetExtraInfoType.AccumulatedShuntCAmpHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntCAmpHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1);
            break;
          }
          case FlexNetExtraInfoType.AccumulatedShuntCWattHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntCWattHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1) * 10;
            break;
          }
          case FlexNetExtraInfoType.DaysSinceFull: {
            extraValueType = OutbackMateFlexNetExtraValueType.DaysSinceFull;
            extraValue = Convert.ToDecimal(Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1)) / 10;
            break;
          }
          case FlexNetExtraInfoType.TodaysMinimumSOC: {
            extraValueType = OutbackMateFlexNetExtraValueType.TodaysMinimumSOC;
            extraValue = Convert.ToByte(rawRecordParts[5]);
            break;
          }
          case FlexNetExtraInfoType.TodaysNetInputAmpHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetInputAmpHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1);
            break;
          }
          case FlexNetExtraInfoType.TodaysNetOutputAmpHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetOutputAmpHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1);
            break;
          }
          case FlexNetExtraInfoType.TodaysNetInputWattHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetInputWattHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1) * 10;
            break;
          }
          case FlexNetExtraInfoType.TodaysNetOutputWattHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetOutputWattHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1) * 10;
            break;
          }
          case FlexNetExtraInfoType.ChargeFactorCorrectedNetAmpHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.ChargeFactorCorrectedNetAmpHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1);
            break;
          }
          case FlexNetExtraInfoType.ChargeFactorCorrectedNetWattHours: {
            extraValueType = OutbackMateFlexNetExtraValueType.ChargeFactorCorrectedNetWattHours;
            extraValue = Convert.ToInt32(rawRecordParts[5]) * (isExtraValueNegative ? -1 : 1) * 10;
            break;
          }
        }

        FlexNetStatus statusFlags = (FlexNetStatus)Convert.ToInt32(rawRecordParts[9]);
        OutbackMateFlexNetRelayState relayState = (((statusFlags & FlexNetStatus.RelayStateOpen) == FlexNetStatus.RelayStateOpen) ? OutbackMateFlexNetRelayState.Open : OutbackMateFlexNetRelayState.Closed);
        OutbackMateFlexNetRelayMode relayMode = (((statusFlags & FlexNetStatus.RelayModeAutomatic) == FlexNetStatus.RelayModeAutomatic) ? OutbackMateFlexNetRelayMode.Automatic : OutbackMateFlexNetRelayMode.Manual);
        bool chargeParamsMet = ((statusFlags & FlexNetStatus.ChargeParamsMet) == FlexNetStatus.ChargeParamsMet);

        return new OutbackMateFlexNetRecord(recordDateTime, hubPort, batteryVoltage, batteryStateOfCharge, batteryTempatureC, chargeParamsMet, relayState, relayMode, 
          shuntAEnabled, shuntAAmps * ((statusFlags & FlexNetStatus.ShuntAValueNegative) == FlexNetStatus.ShuntAValueNegative ? -1 : 1),
          shuntBEnabled, shuntBAmps * ((statusFlags & FlexNetStatus.ShuntBValueNegative) == FlexNetStatus.ShuntBValueNegative ? -1 : 1),
          shuntCEnabled, shuntCAmps * ((statusFlags & FlexNetStatus.ShuntCValueNegative) == FlexNetStatus.ShuntCValueNegative ? -1 : 1),
          extraValueType, extraValue, rawRecord);
      }
      #endregion

      #region OutbackInverterChargerRecord

      if ("0123456789;".Contains(rawRecordParts[0])) {
        byte hubPort = Convert.ToByte(((byte)rawRecordParts[0][0]) - 48);

        byte inverterCurrent = Convert.ToByte(rawRecordParts[1]);
        byte chargerCurrent = Convert.ToByte(rawRecordParts[2]);
        byte buyCurrent = Convert.ToByte(rawRecordParts[3]);

        var acInputVoltage = Convert.ToUInt16(rawRecordParts[4]);
        var acOutputVoltage = Convert.ToUInt16(rawRecordParts[5]);
        byte sellCurrent = Convert.ToByte(rawRecordParts[6]);

        var fxOperationalMode = Convert.ToInt32(rawRecordParts[7]);
        var errorMode = Convert.ToInt32(rawRecordParts[8]);
        var acInputMode = Convert.ToInt32(rawRecordParts[9]);

        decimal batteryVoltage = Convert.ToDecimal(rawRecordParts[10]) / 10;
        var misc = Convert.ToInt32(rawRecordParts[11]);
        var warningMode = Convert.ToInt32(rawRecordParts[12]);

        return new OutbackMateInverterChargerRecord(recordDateTime, hubPort, inverterCurrent, chargerCurrent, buyCurrent,
          acInputVoltage, acOutputVoltage, sellCurrent,
          (OutbackFxInverterChargerOperationalMode)fxOperationalMode,
          (OutbackFxInverterChargerErrorMode)errorMode,
          (OutbackFxInverterChargerACInputMode)acInputMode, 
          batteryVoltage,
          (OutbackFxInverterChargerMisc)misc,
          (OutbackFxInverterChargerWarningMode)warningMode, 
          rawRecord);
      }

      #endregion

      return null;
    }

    #region IOutbackMateRecord Members

    OutbackMateRecordType IOutbackMateRecord.RecordType {
      get { return this._recordType; }
    }

    [DataMember]
    public DateTimeOffset RecordDateTime { get; private set; }

    [DataMember]
    public byte HubPort { get; private set; }

    string IOutbackMateRecord.RawData {
      get { return this._rawData; }
    }

    #endregion

    #region ICloneable Members

    object ICloneable.Clone() {
      return this.MemberwiseClone();
    }

    #endregion
  }
}
