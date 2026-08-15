using System;
using FortRise;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TFModFortRiseHandicap
{
  /// <summary>
  /// In <see cref="RespawnLivesVersus"/> mode, players have N lives and respawn immediately when killed.
  /// No shield mechanic - normal death with instant respawn if lives remain.
  /// </summary>
  public class MyRespawnPlayer : IHookable
  {
    public static readonly int[] LivesRemaining = new int[8];
    public static readonly bool[] ShouldRespawn = new bool[8];
    private static readonly Vector2[] RoundSpawnPositions = new Vector2[8];
    private static readonly bool[] HasRoundSpawnPosition = new bool[8];
    private static readonly float[] ImmunityFramesRemaining = new float[8];
    private const int FramesPerSecond = 60;

    // Nombre de vies au-dela duquel on remplace la barre par un compteur.
    private const int MaxLifeSegments = 8;

    private struct UpdateState
    {
      public bool ShootLockedByImmunity;
      public bool PreviousShootLock;
    }

    public static void Load(IHarmony harmony)
    {
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Added)),
          postfix: new HarmonyMethod(Added_patch)
      );
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HUDRender)),
          prefix: new HarmonyMethod(HUDRender_prefix_patch)
      );
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Player), nameof(Player.HurtBouncedOn)),
          prefix: new HarmonyMethod(HurtBouncedOn_patch)
      );
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Player), nameof(Player.Update)),
          prefix: new HarmonyMethod(Update_prefix_patch),
          postfix: new HarmonyMethod(Update_postfix_patch)
      );
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Session), nameof(Session.StartRound)),
          postfix: new HarmonyMethod(StartRound_patch)
      );
      harmony.Patch(
          AccessTools.DeclaredMethod(typeof(Session), nameof(Session.OnPlayerDeath)),
          prefix: new HarmonyMethod(OnPlayerDeath_prefix_patch),
          postfix: new HarmonyMethod(OnPlayerDeath_postfix_patch)
      );
    }

    /// <summary>
    /// Le systeme de vies n'existe que dans les modes dont le RoundLogic est patche.
    /// Ce test manquait sur tous les hooks sauf l'affichage : dans un mode ajoute par
    /// un mod (PlayTag...), le HUD etait bien masque mais les joueurs continuaient de
    /// reapparaitre, puisque OnPlayerDeath decomptait les vies malgre tout.
    /// </summary>
    private static bool EnabledFor(Player self)
    {
      return self != null
          && self.Level != null
          && self.Level.Session != null
          && TFModFortRiseHandicapModule.IsHandicapMode(self.Level.Session.MatchSettings);
    }

    private static bool EnabledFor(Session session)
    {
      return session != null
          && TFModFortRiseHandicapModule.IsHandicapMode(session.MatchSettings);
    }

    private static void Added_patch(Player __instance)
    {
      if (!EnabledFor(__instance)) return;

      int p = __instance.PlayerIndex;
      if (LivesRemaining[p] <= 0)
      {
        LivesRemaining[p] = PlayerHandicap.GetStartingLives(p);
      }
      ShouldRespawn[p] = false;
      if (!HasRoundSpawnPosition[p])
      {
        RoundSpawnPositions[p] = __instance.Position;
        HasRoundSpawnPosition[p] = true;
      }
    }

    /// <summary>
    /// Masque le compteur de fleches pendant l'immunite qui suit une reapparition.
    ///
    /// Mettre ArrowHUD.Visible a false ne suffit pas : Player.HUDRender appelle
    /// ArrowHUD.Render() directement, sans jamais consulter Visible. On saute donc la
    /// methode entiere et on rend l'indicateur de joueur nous-memes, puisque c'est
    /// l'autre chose qu'elle affichait. Le postfix, lui, s'execute quand meme et
    /// continue de dessiner la barre de vies.
    /// </summary>
    private static bool HUDRender_prefix_patch(Player __instance, bool wrapped)
    {
      if (!EnabledFor(__instance))
        return true;

      if (ImmunityFramesRemaining[__instance.PlayerIndex] <= 0f)
        return true;

      if (!wrapped && __instance.Indicator != null)
        __instance.Indicator.Render();

      return false;
    }

    private static void StartRound_patch(Session __instance)
    {
      if (!EnabledFor(__instance))
      {
        // Compteurs remis a zero hors mode handicap : un reglage laisse par une
        // partie precedente ne doit rien pouvoir declencher ici.
        for (int i = 0; i < LivesRemaining.Length; i++)
        {
          LivesRemaining[i] = 0;
          ShouldRespawn[i] = false;
          HasRoundSpawnPosition[i] = false;
          ImmunityFramesRemaining[i] = 0f;
        }
        return;
      }

      for (int i = 0; i < TFGame.Players.Length; i++)
      {
        LivesRemaining[i] = TFGame.Players[i] ? PlayerHandicap.GetStartingLives(i) : 0;
        ShouldRespawn[i] = false;
        HasRoundSpawnPosition[i] = false;
        ImmunityFramesRemaining[i] = 0f;
      }

      foreach (Entity entity in __instance.CurrentLevel.Players)
      {
        if (entity is Player player)
        {
          int p = player.PlayerIndex;
          RoundSpawnPositions[p] = player.Position;
          HasRoundSpawnPosition[p] = true;
        }
      }

      // La barre de vies est une entite du niveau, ajoutee a chaque manche : le
      // postfix de HUDRender qui la dessinait ne partait pas, la methode etant trop
      // petite pour survivre au JIT. Voir LivesHUD.
      __instance.CurrentLevel.Add<LivesHUD>(new LivesHUD());
    }

    private static void Update_prefix_patch(Player __instance, ref UpdateState __state)
    {
      if (!EnabledFor(__instance)) return;

      int p = __instance.PlayerIndex;
      __state.ShootLockedByImmunity = ImmunityFramesRemaining[p] > 0f;
      if (__state.ShootLockedByImmunity)
      {
        __state.PreviousShootLock = Player.ShootLock;
        Player.ShootLock = true;
      }
    }

    private static void Update_postfix_patch(Player __instance, ref UpdateState __state)
    {
      if (!__state.ShootLockedByImmunity)
        return;

      int p = __instance.PlayerIndex;
      Player.ShootLock = __state.PreviousShootLock;
      ImmunityFramesRemaining[p] = Math.Max(0f, ImmunityFramesRemaining[p] - Engine.TimeMult);
    }

    private static bool HurtBouncedOn_patch(Player __instance, int bouncerIndex)
    {
      if (!EnabledFor(__instance)) return true;

      if (bouncerIndex >= 0 &&
          bouncerIndex < ImmunityFramesRemaining.Length &&
          ImmunityFramesRemaining[bouncerIndex] > 0f)
      {
        return false;
      }

      return true;
    }

    private static void OnPlayerDeath_prefix_patch(Session __instance, Player player, PlayerCorpse corpse, int playerIndex, DeathCause deathType, Vector2 position, int killerIndex, ref bool __state)
    {
      __state = false;
      if (!EnabledFor(__instance)) return;

      ref int lives = ref LivesRemaining[playerIndex];

      if (lives > 1)
      {
        lives--;
        // Le respawn est declenche dans le postfix, une fois la mort traitee.
        __state = true;
      }
      else if (lives == 1)
      {
        lives--;
      }
    }

    private static void OnPlayerDeath_postfix_patch(Session __instance, Player player, PlayerCorpse corpse, int playerIndex, DeathCause deathType, Vector2 position, int killerIndex, ref bool __state)
    {
      if (!__state)
        return;

      Alarm.Set(corpse, 1, delegate
      {
        RespawnPlayer(__instance, playerIndex, player.Allegiance, player.TeamColor);
      });
    }

    private static void RespawnPlayer(Session session, int playerIndex, Allegiance allegiance, Allegiance teamColor)
    {
      if (session?.CurrentLevel == null)
        return;
      if (session.CurrentLevel.GetPlayer(playerIndex) != null)
        return;

      Vector2 spawnPos;
      if (HasRoundSpawnPosition[playerIndex])
      {
        spawnPos = RoundSpawnPositions[playerIndex];
      }
      else
      {
        var spawnPoints = session.CurrentLevel.GetXMLPositions("PlayerSpawn");
        spawnPos = spawnPoints.Count > 0 ? spawnPoints[playerIndex % spawnPoints.Count] : new Vector2(100, 100);
      }

      var newPlayer = new Player(
        playerIndex,
        spawnPos,
        allegiance,
        teamColor,
        session.GetPlayerInventory(playerIndex),
        session.GetSpawnHatState(playerIndex),
        false,
        false,
        true
      );

      session.CurrentLevel.Add(newPlayer);

      int immunityFrames = Math.Max(0, PlayerHandicap.GetImmunitySeconds() * FramesPerSecond);
      if (immunityFrames > 0)
      {
        newPlayer.Flash(immunityFrames);
        ImmunityFramesRemaining[playerIndex] = immunityFrames;
      }
      else
      {
        ImmunityFramesRemaining[playerIndex] = 0f;
      }
    }
  }
}
