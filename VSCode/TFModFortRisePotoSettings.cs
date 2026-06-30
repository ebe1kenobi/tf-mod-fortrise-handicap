using FortRise;

namespace TFModFortRisePoto
{
  public class TFModFortRisePotoSettings : ModuleSettings
  {
    [SettingsName("Handicap: Immunity on respawn (seconds)")]
    [SettingsNumber(0, 10)]
    public int handicapRespawnImmunitySeconds = 2;
  }
}
