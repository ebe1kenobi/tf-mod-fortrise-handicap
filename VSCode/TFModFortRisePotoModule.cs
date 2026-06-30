//compatible 8 joueur teamdeathmatch lastmanstanding teamdeathmath
//

using System;
using System.Diagnostics;
using System.IO;
using FortRise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Monocle;
using MonoMod.ModInterop;
using TowerFall;

//CHAR_A_DIE -> orange aie
//    *         ouille
namespace TFModFortRisePoto
{
  [Fort("com.ebe1.kenobi.tfmodfortrisepoto", "TFModFortRisePoto")]
  public class TFModFortRisePotoModule : FortModule
  {
    public static TFModFortRisePotoModule Instance;
    public override Type SettingsType => typeof(TFModFortRisePotoSettings);
    public static TFModFortRisePotoSettings Settings => (TFModFortRisePotoSettings)Instance.InternalSettings;
    public TFModFortRisePotoModule()
    {
      if (!Debugger.IsAttached)
      {
        //Debugger.Launch(); // Proposera d’attacher Visual Studio
      }
      Instance = this;
      Logger.Init("TFModFortRisePoto");
    }

    public override void LoadContent()
    {
    }

    public override void Load()
    {
      MyVersusModeButton.Load();
      MyRespawnPlayer.Load();
      MyHeadhuntersRoundLogic.Load();
      MyLastManStandingRoundLogic.Load();
      MyTeamDeathmatchRoundLogic.Load();
    }


    public override void Unload()
    {
      MyVersusModeButton.Unload();
      MyRespawnPlayer.Unload();
      MyHeadhuntersRoundLogic.Unload();
      MyLastManStandingRoundLogic.Unload();
      MyTeamDeathmatchRoundLogic.Unload();
    }
  }
}
