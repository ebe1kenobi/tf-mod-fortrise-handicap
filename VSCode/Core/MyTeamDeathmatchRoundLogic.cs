using Microsoft.Xna.Framework;
using TowerFall;
namespace TFModFortRisePoto
{
  internal class MyTeamDeathmatchRoundLogic
  {
    internal static void Load()
    {
      On.TowerFall.TeamDeathmatchRoundLogic.OnPlayerDeath += OnPlayerDeath;
    }

    internal static void Unload()
    {
      On.TowerFall.TeamDeathmatchRoundLogic.OnPlayerDeath -= OnPlayerDeath;
    }

    public static void OnPlayerDeath(On.TowerFall.TeamDeathmatchRoundLogic.orig_OnPlayerDeath orig, global::TowerFall.TeamDeathmatchRoundLogic self, global::TowerFall.Player player, global::TowerFall.PlayerCorpse corpse, int playerIndex, DeathCause cause, Vector2 position, int killerIndex) {
      // Check if this player has remaining lives
      if (MyRespawnPlayer.LivesRemaining[playerIndex] > 0)
      {
        // Don't call base.OnPlayerDeath to prevent round end logic
        // Just handle the death without triggering round end checks
        return;
      }

      // Player is out of lives, use normal death handling
      orig(self, player, corpse, playerIndex, cause, position, killerIndex);
    }
  }
}
