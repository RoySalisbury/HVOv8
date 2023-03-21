namespace HVO.Power.OutbackPowerSystems
{
  public enum OutbackFxInverterChargerOperationalMode
  {
    InverterOff = 0,
    Search = 1,
    InverterOn = 2,
    Charge = 3,
    Silent = 4,
    Float = 5,
    Equalize = 6,
    ChargerOff = 7,
    Support = 8,
    SellEnabled = 8,
    PassThru = 10,
    FxError = 90,
    AgsError = 91,
    CommunicationsError = 92
  }
}
