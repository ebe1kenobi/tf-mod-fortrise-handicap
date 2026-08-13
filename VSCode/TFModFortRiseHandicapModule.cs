//compatible 8 joueur teamdeathmatch lastmanstanding teamdeathmath
//add customname support for popup

using System;
using System.Diagnostics;
using FortRise;
using TowerFall;
using Microsoft.Extensions.Logging;

//CHAR_A_DIE -> orange aie
//    *         ouille
namespace TFModFortRiseHandicap
{
  public class TFModFortRiseHandicapModule : Mod
  {
    public static TFModFortRiseHandicapModule Instance;

    internal Type[] Hookables = [
        typeof(MyVersusModeButton),
        typeof(MyRespawnPlayer),
        typeof(MySession),
        typeof(MyHeadhuntersRoundLogic),
        typeof(MyLastManStandingRoundLogic),
        typeof(MyTeamDeathmatchRoundLogic),
    ];

    public static TFModFortRiseHandicapSettings Settings => Instance.GetSettings<TFModFortRiseHandicapSettings>()!;

    /// <summary>
    /// Modes ou le systeme de vies de ce mod s'applique : ce sont ceux dont le
    /// RoundLogic est patche (MyHeadhuntersRoundLogic, MyLastManStandingRoundLogic,
    /// MyTeamDeathmatchRoundLogic). Ailleurs, les vies ne sont jamais decomptees.
    ///
    /// Le test existait deja pour decider d'ouvrir la popup Y depuis le menu, mais
    /// pas cote jeu : la barre de vies restait affichee dans un mode ajoute par un
    /// mod (PlayTag...) apres une partie ou des vies avaient ete reglees.
    ///
    /// IsCustom est teste en premier : un mode de mod recoit une valeur d'enum
    /// au-dela de celles du jeu, mais s'appuyer sur ce detail serait fragile.
    /// </summary>
    public static bool IsHandicapMode(MatchSettings settings)
    {
      if (settings == null)
        return false;

      if (settings.IsCustom)
        return false;

      return settings.Mode == Modes.HeadHunters
          || settings.Mode == Modes.LastManStanding
          || settings.Mode == Modes.TeamDeathmatch;
    }

    /// <summary>
    /// Ecrit les reglages sur disque immediatement.
    ///
    /// FortRise ne les sauvegarde qu'en quittant le menu Options du jeu
    /// (MainMenu.DestroyOptions) ou lors d'une sauvegarde de partie. Une valeur
    /// changee depuis une popup du mod restait donc en memoire et etait perdue en
    /// quittant le jeu. SaveSettings est internal cote FortRise, d'ou la reflexion.
    /// </summary>
    public static void SaveSettingsNow()
    {
      if (Instance == null)
        return;

      try
      {
        var method = typeof(Mod).GetMethod("SaveSettings",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (method != null)
          method.Invoke(Instance, null);
      }
      catch (Exception ex)
      {
        TFModFortRiseHandicap.Logger.Info($"[Settings] sauvegarde immediate impossible : {ex.Message}");
      }
    }

    public TFModFortRiseHandicapModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
      if (!Debugger.IsAttached)
      {
        //Debugger.Launch(); // Proposera d’attacher Visual Studio
      }
      Instance = this;
      TFModFortRiseHandicap.Logger.Init(logger);

      foreach (var hookable in Hookables)
      {
        hookable.GetMethod(nameof(IHookable.Load))!.Invoke(null, [context.Harmony]);
      }
    }

    public override ModuleSettings CreateSettings()
    {
      return new TFModFortRiseHandicapSettings();
    }
  }
}
