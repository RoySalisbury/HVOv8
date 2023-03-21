namespace HVO.Power.OutbackPowerSystems {
  using System;

  [Flags]
  public enum OutbackMateChargeControllerErrorMode {
    None = 0,
    Unused1 = 1,
    Unused2 = 2,
    Unused3 = 4,
    Unused4 = 8,
    Unused5 = 16,
    ShortedBatterySensor = 32,
    TooHot = 64,
    HighVOC = 128
  }
}


