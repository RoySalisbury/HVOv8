namespace HVO.Power.OutbackPowerSystems {
  public enum OutbackMateFlexNetExtraValueType {
    Unknown = 0,
    AccumulatedShuntAAmpHours = 1,
    AccumulatedShuntAWattHours = 2,
    AccumulatedShuntBAmpHours = 3,
    AccumulatedShuntBWattHours = 4,
    AccumulatedShuntCAmpHours = 5,
    AccumulatedShuntCWattHours = 6,
    DaysSinceFull = 7,
    TodaysMinimumSOC = 8,
    TodaysNetInputAmpHours = 9,
    TodaysNetOutputAmpHours = 10,
    TodaysNetInputWattHours = 11,
    TodaysNetOutputWattHours = 12,
    ChargeFactorCorrectedNetAmpHours = 13,
    ChargeFactorCorrectedNetWattHours = 14
  }
}
