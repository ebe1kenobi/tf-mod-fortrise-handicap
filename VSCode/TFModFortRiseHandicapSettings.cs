using FortRise;

namespace TFModFortRiseHandicap
{
  public class TFModFortRiseHandicapSettings : ModuleSettings
  {
    public override void Create(ISettingsCreate settings)
    {
      settings.CreateNumber("Handicap: Immunity on respawn (seconds)", handicapRespawnImmunitySeconds, (x) => handicapRespawnImmunitySeconds = x, 0, 10);
    }

    //[SettingsName("Handicap: Immunity on respawn (seconds)")]
    //[SettingsNumber(0, 10)]
    public int handicapRespawnImmunitySeconds { get; set; } = 2;
  }
}
