//compatible 8 joueur teamdeathmatch lastmanstanding teamdeathmath
//add customname support for popup

using System;
using System.Diagnostics;
using FortRise;
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

    public TFModFortRiseHandicapModule(IModContent content, IModuleContext context, ILogger logger) : base(content, context, logger)
    {
      if (!Debugger.IsAttached)
      {
        //Debugger.Launch(); // Proposera d’attacher Visual Studio
      }
      Instance = this;
      //TFModFortRiseHandicap.Logger.Init("TFModFortRiseHandicap");

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
