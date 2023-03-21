namespace HVO.Power.OutbackPowerSystems
{
  using System;

  [Flags]
  public enum OutbackFxInverterChargerWarningMode
  {
    None = 0,
    ACInputFreqHigh = 1,
    ACInputFreqLow = 2,
    ACInputVoltageHigh = 4,
    ACInputVoltageLow = 8,
    BuyAmpsGreaterThanInputSize = 16,
    TemperatureSensorFailed = 32,
    CommunicationsError = 64,
    FanFailure = 128
  }
}
