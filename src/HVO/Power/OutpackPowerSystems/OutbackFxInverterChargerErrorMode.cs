namespace HVO.Power.OutbackPowerSystems
{
  using System;

  [Flags]
  public enum OutbackFxInverterChargerErrorMode
  {
    None = 0,
    LowVoltageACOutput = 1,
    StackingError = 2,
    OverTemperature = 4,
    LowBattery = 8,
    PhaseLoss = 16,
    HighBattery = 32,
    ShortedOutput = 64,
    BackFeed = 128
  }
}
