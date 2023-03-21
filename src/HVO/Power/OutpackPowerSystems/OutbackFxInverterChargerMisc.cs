namespace HVO.Power.OutbackPowerSystems
{
  using System;

  [Flags]
  public enum OutbackFxInverterChargerMisc
  {
    None = 0,
    AC240 = 1,
    Unused1 = 2,
    Unused2 = 4,
    Unused3 = 8,
    Unused4 = 16,
    Unused5 = 32,
    Unused6 = 64,
    AuxOutputOn = 128
  }
}
