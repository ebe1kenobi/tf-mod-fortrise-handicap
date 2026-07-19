using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using TowerFall;

namespace TFModFortRiseHandicap
{
  public class MyLastManStandingRoundLogic : IHookable
  {
    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(LastManStandingRoundLogic), nameof(LastManStandingRoundLogic.OnPlayerDeath)),
          prefix: new HarmonyMethod(OnPlayerDeath_patch)
      );
    }

    public static bool OnPlayerDeath_patch(LastManStandingRoundLogic __instance, Player player, PlayerCorpse corpse, int playerIndex, DeathCause deathType, Vector2 position, int killerIndex)
    {
      // Check if this player has remaining lives
      if (MyRespawnPlayer.LivesRemaining[playerIndex] > 0)
      {
        // Don't call the original OnPlayerDeath to prevent round end logic
        // Just handle the death without triggering round end checks
        return false;
      }

      // Player is out of lives, use normal death handling
      return true;
    }
  }
}
