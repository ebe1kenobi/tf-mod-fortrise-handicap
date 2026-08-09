using FortRise;

namespace TFModFortRiseHandicap
{
  /// <summary>
  /// Reglages du module, doublons volontaires de ceux de la popup (Y sur le bouton
  /// de mode) : les deux ecrans agissent sur les memes valeurs.
  ///
  /// Victoires et vies sont per-joueur cote popup, mais globales ici. Changer un
  /// reglage l'applique donc a TOUS les joueurs ; a l'inverse, la popup ne remonte
  /// une valeur ici que si elle est la meme pour tout le monde (voir
  /// PlayerHandicap.SyncSettings).
  ///
  /// L'immunite, elle, n'a qu'une seule valeur : elle vit dans ce reglage, et la
  /// popup ecrit directement dedans.
  /// </summary>
  public class TFModFortRiseHandicapSettings : ModuleSettings
  {

    // FortRise n'ecrit les reglages qu'en SORTANT du menu Options
    // (MainMenu.DestroyOptions) : quitter le jeu depuis ce menu perdait la
    // modification. Chaque changement declenche donc une sauvegarde immediate.
    public override void Create(ISettingsCreate settings)
    {
      settings.CreateNumber("Victories for all players", handicapVictories,
          (x) =>
          {
            handicapVictories = x;
            PlayerHandicap.SetVictoriesForAll(x);
            TFModFortRiseHandicapModule.SaveSettingsNow();
          },
          0, 20);

      settings.CreateNumber("Lives for all players", handicapLives,
          (x) =>
          {
            handicapLives = x;
            PlayerHandicap.SetLivesForAll(x);
            TFModFortRiseHandicapModule.SaveSettingsNow();
          },
          1, 20);

      settings.CreateNumber("Immunity on respawn (seconds)", handicapRespawnImmunitySeconds,
          (x) => { handicapRespawnImmunitySeconds = x; TFModFortRiseHandicapModule.SaveSettingsNow(); },
          0, PlayerHandicap.MaxImmunitySeconds);
    }

    // Avance en victoires appliquee a tous les joueurs.
    public int handicapVictories { get; set; } = 0;

    // Nombre de vies applique a tous les joueurs.
    public int handicapLives { get; set; } = 1;

    //[SettingsName("Handicap: Immunity on respawn (seconds)")]
    //[SettingsNumber(0, 10)]
    public int handicapRespawnImmunitySeconds { get; set; } = 2;
  }
}
