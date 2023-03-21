namespace HVO.Power.OutbackPowerSystems
{
  using System;
  using System.Linq;
  using System.Runtime.Serialization;

  [DataContract]
  public abstract class OutbackMate2Record : IOutbackMateRecord, ICloneable
  {
    [Flags]
    private enum FlexNetStatus
    {
      ChargeParamsMet = 1,
      RelayStateOpen = 2,
      RelayModeAutomatic = 4,
      ShuntAValueNegative = 8,
      ShuntBValueNegative = 16,
      ShuntCValueNegative = 32
    }

    private enum FlexNetExtraInfoType
    {
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

    protected OutbackMate2Record(OutbackMateRecordType recordType, DateTimeOffset recordDateTime, byte hubPort, string rawData)
    {
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
      if (string.IsNullOrWhiteSpace(rawRecord))
      {
        throw new ArgumentNullException("rawRecord");
      }

      // Split the rawRecord into its distinct data parts
      string[] rawRecordParts = rawRecord.Split(',');

      #region OutbackMateChargeControllerRecord
      // The first part is the type of record that we are creating.
      if (rawRecordParts[1] == "3")
      {
        //                    0          1          2           3                4            5            6
        //valuenames = ['address','device_id', 'unused', 'charge_current','pv_current','pv_voltage', 'daily_kwh', 
        //                    7              8           9              10             11           12         13
        //              'charge_tenths', 'aux_mode','error_modes','charge_mode','battery_volts', 'daily_ah','unused']

        byte hubPort = Convert.ToByte(rawRecordParts[0]);
        int unused1 = Convert.ToInt32(rawRecordParts[2]);

        decimal chargerAmps = Convert.ToDecimal(rawRecordParts[3]) + (Convert.ToDecimal(rawRecordParts[7]) / 10);
        var pvAmps = Convert.ToInt16(rawRecordParts[4]);

        var pvVoltage = Convert.ToInt16(rawRecordParts[5]);
        int dailyWattHoursProduced = Convert.ToInt32(rawRecordParts[6]) * 100;
        byte chargerAuxRelayMode = Convert.ToByte(rawRecordParts[8]);
        short chargerErrorMode = Convert.ToInt16(rawRecordParts[9]);
        byte chargerMode = Convert.ToByte(rawRecordParts[10]);
        decimal chargerVoltage = Convert.ToDecimal(rawRecordParts[11]) / 10;
        short dailyAmpHoursProduced = Convert.ToInt16(rawRecordParts[12]);
        int unused2 = Convert.ToInt32(rawRecordParts[13]);

        // return the new record
        return new OutbackMateChargeControllerRecord(recordDateTime, hubPort, pvAmps, pvVoltage, chargerAmps, chargerVoltage, dailyAmpHoursProduced, dailyWattHoursProduced,
          (OutbackMateChargeControllerMode)chargerMode,
          (OutbackMateChargeControllerAuxRelayMode)chargerAuxRelayMode,
          (OutbackMateChargeControllerErrorMode)chargerErrorMode,
          rawRecord);
      }
      #endregion

      #region OutbackMateFlexNetRecord
      if (rawRecordParts[1] == "4")
      {
        byte hubPort = Convert.ToByte(rawRecordParts[0]);

        decimal shuntAAmps = Convert.ToDecimal(rawRecordParts[2]) / 10;
        decimal shuntBAmps = Convert.ToDecimal(rawRecordParts[3]) / 10;
        decimal shuntCAmps = Convert.ToDecimal(rawRecordParts[4]) / 10;

        decimal batteryVoltage = Convert.ToDecimal(rawRecordParts[7]) / 10;
        byte batteryStateOfCharge = Convert.ToByte(rawRecordParts[8]);

        bool shuntAEnabled = rawRecordParts[9][0] == '0';
        bool shuntBEnabled = rawRecordParts[9][1] == '0';
        bool shuntCEnabled = rawRecordParts[9][2] == '0';

        short? batteryTempatureC = Convert.ToInt16(rawRecordParts[11]);
        if (batteryTempatureC != 99)
        {
          batteryTempatureC -= 10;
        }
        else
        {
          batteryTempatureC = null;
        }

        bool isExtraValueNegative = (Convert.ToByte(rawRecordParts[5]) & 0x40) == 0x40;
        FlexNetExtraInfoType flexNetExtraInfoType = (FlexNetExtraInfoType)(Convert.ToInt32(rawRecordParts[5]) & 0x3F);

        OutbackMateFlexNetExtraValueType? extraValueType = null;
        decimal? extraValue = null;

        switch (flexNetExtraInfoType)
        {
          case FlexNetExtraInfoType.AccumulatedShuntAAmpHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntAAmpHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1);
              break;
            }
          case FlexNetExtraInfoType.AccumulatedShuntAWattHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntAWattHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1) * 10;
              break;
            }
          case FlexNetExtraInfoType.AccumulatedShuntBAmpHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntBAmpHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1);
              break;
            }
          case FlexNetExtraInfoType.AccumulatedShuntBWattHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntBWattHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1) * 10;
              break;
            }
          case FlexNetExtraInfoType.AccumulatedShuntCAmpHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntCAmpHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1);
              break;
            }
          case FlexNetExtraInfoType.AccumulatedShuntCWattHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.AccumulatedShuntCWattHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1) * 10;
              break;
            }
          case FlexNetExtraInfoType.DaysSinceFull:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.DaysSinceFull;
              extraValue = Convert.ToDecimal(Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1)) / 10;
              break;
            }
          case FlexNetExtraInfoType.TodaysMinimumSOC:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.TodaysMinimumSOC;
              extraValue = Convert.ToByte(rawRecordParts[6]);
              break;
            }
          case FlexNetExtraInfoType.TodaysNetInputAmpHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetInputAmpHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1);
              break;
            }
          case FlexNetExtraInfoType.TodaysNetOutputAmpHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetOutputAmpHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1);
              break;
            }
          case FlexNetExtraInfoType.TodaysNetInputWattHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetInputWattHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1) * 10;
              break;
            }
          case FlexNetExtraInfoType.TodaysNetOutputWattHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.TodaysNetOutputWattHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1) * 10;
              break;
            }
          case FlexNetExtraInfoType.ChargeFactorCorrectedNetAmpHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.ChargeFactorCorrectedNetAmpHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1);
              break;
            }
          case FlexNetExtraInfoType.ChargeFactorCorrectedNetWattHours:
            {
              extraValueType = OutbackMateFlexNetExtraValueType.ChargeFactorCorrectedNetWattHours;
              extraValue = Convert.ToInt32(rawRecordParts[6]) * (isExtraValueNegative ? -1 : 1) * 10;
              break;
            }
        }

        FlexNetStatus statusFlags = (FlexNetStatus)Convert.ToInt32(rawRecordParts[10]);
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

      if (rawRecordParts[1] == "2")
      {
        byte hubPort = Convert.ToByte(rawRecordParts[0]);

        byte inverterCurrent = Convert.ToByte(rawRecordParts[2]);
        byte chargerCurrent = Convert.ToByte(rawRecordParts[3]);
        byte buyCurrent = Convert.ToByte(rawRecordParts[4]);

        var acInputVoltage = Convert.ToUInt16(rawRecordParts[5]);
        var acOutputVoltage = Convert.ToUInt16(rawRecordParts[6]);
        byte sellCurrent = Convert.ToByte(rawRecordParts[7]);

        var fxOperationalMode = Convert.ToInt32(rawRecordParts[8]);
        var errorMode = Convert.ToInt32(rawRecordParts[9]);
        var acInputMode = Convert.ToInt32(rawRecordParts[10]);

        decimal batteryVoltage = Convert.ToDecimal(rawRecordParts[11]) / 10;
        var misc = Convert.ToInt32(rawRecordParts[12]);
        var warningMode = Convert.ToInt32(rawRecordParts[13]);

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

    OutbackMateRecordType IOutbackMateRecord.RecordType
    {
      get { return this._recordType; }
    }

    [DataMember]
    public DateTimeOffset RecordDateTime { get; private set; }

    [DataMember]
    public byte HubPort { get; private set; }

    string IOutbackMateRecord.RawData
    {
      get { return this._rawData; }
    }

    #endregion

    #region ICloneable Members

    object ICloneable.Clone()
    {
      return this.MemberwiseClone();
    }

    #endregion
  }
}
