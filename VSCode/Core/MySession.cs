using FortRise;
using HarmonyLib;
using TowerFall;

namespace TFModFortRiseHandicap
{
  public class MySession : IHookable
  {
    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Session), nameof(Session.StartGame)),
          postfix: new HarmonyMethod(StartGame_patch)
      );
    }

    // StartGame n'est appele qu'une fois par match : c'est la que l'avance
    // en victoires reglee dans la popup est injectee dans les scores.
    private static void StartGame_patch(Session __instance)
    {
      PlayerHandicap.ApplyVictoryHandicap(__instance);
    }
  }
}
