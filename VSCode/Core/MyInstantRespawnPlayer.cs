using System;
using FortRise;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TFModFortRisePoto
{
  /// <summary>
  /// In <see cref="RespawnLivesVersus"/> mode, players have N lives and respawn immediately when killed.
  /// No shield mechanic - normal death with instant respawn if lives remain.
  /// </summary>
  public static class MyRespawnPlayer
  {
    public static readonly int[] LivesRemaining = new int[8];
    public static readonly bool[] ShouldRespawn = new bool[8];
    private static readonly Vector2[] RoundSpawnPositions = new Vector2[8];
    private static readonly bool[] HasRoundSpawnPosition = new bool[8];
    private static readonly float[] ImmunityFramesRemaining = new float[8];
    private const int FramesPerSecond = 60;


    internal static void Load()
    {
      On.TowerFall.Player.Added += Added_patch;
      On.TowerFall.Player.HUDRender += HUDRender_patch;
      On.TowerFall.Player.HurtBouncedOn += HurtBouncedOn_patch;
      On.TowerFall.Player.Update += Update_patch;
      On.TowerFall.Session.StartRound += StartRound_patch;
      On.TowerFall.Session.OnPlayerDeath += OnPlayerDeath_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.Player.Added -= Added_patch;
      On.TowerFall.Player.HUDRender -= HUDRender_patch;
      On.TowerFall.Player.HurtBouncedOn -= HurtBouncedOn_patch;
      On.TowerFall.Player.Update -= Update_patch;
      On.TowerFall.Session.StartRound -= StartRound_patch;
      On.TowerFall.Session.OnPlayerDeath -= OnPlayerDeath_patch;
    }

    private static void Added_patch(On.TowerFall.Player.orig_Added orig, global::TowerFall.Player self)
    {
      orig(self);

      int p = self.PlayerIndex;
      if (LivesRemaining[p] <= 0)
      {
        LivesRemaining[p] = PlayerHandicap.GetStartingLives(p);
      }
      ShouldRespawn[p] = false;
      if (!HasRoundSpawnPosition[p])
      {
        RoundSpawnPositions[p] = self.Position;
        HasRoundSpawnPosition[p] = true;
      }
    }

    private static void HUDRender_patch(On.TowerFall.Player.orig_HUDRender orig, global::TowerFall.Player self, bool wrapped)
    {
      int playerIndex = self.PlayerIndex;
      bool hideArrowHud = ImmunityFramesRemaining[playerIndex] > 0f && self.ArrowHUD != null;
      bool previousArrowHudVisible = false;
      if (hideArrowHud)
      {
        previousArrowHudVisible = self.ArrowHUD.Visible;
        self.ArrowHUD.Visible = false;
      }

      orig(self, wrapped);

      if (hideArrowHud)
      {
        self.ArrowHUD.Visible = previousArrowHudVisible;
      }

      if (wrapped)
        return;

      int maxLives = Math.Max(1, PlayerHandicap.GetStartingLives(playerIndex));
      int lives = Math.Max(0, LivesRemaining[playerIndex]);
      if (maxLives <= 1)
        return;
      //if (self.State == global::TowerFall.Player.PlayerStates.Ducking || self.DodgeSliding || self.Invisible)
      if (self.State == global::TowerFall.Player.PlayerStates.Ducking || self.Invisible)
        return;

      //if (TFModFortRisePotoModule.Settings.lifeNumber > 8) {
      //  Vector2 textPos = self.Position + new Vector2(0f, -22f);
      //  Draw.OutlineTextCentered(TFGame.Font, lives.ToString(), textPos, Color.White, 1f);
      //  return;
      //}

      float segmentWidth = 3f;
      float segmentHeight = 3f;
      float gap = 1f;
      float barWidth = maxLives * segmentWidth + (maxLives - 1) * gap;
      Vector2 barPos = (self.Position + new Vector2(-barWidth * 0.5f, -12.5f)).Floor();

      Draw.Rect(barPos.X - 1f, barPos.Y - 1f, barWidth + 2f, segmentHeight + 2f, Color.Black * 0.75f);
      //Color lifeColor = Color.Lerp(Color.Red, Color.LimeGreen, (float)lives / maxLives);
      Color lifeColor = Color.LimeGreen;
      for (int i = 0; i < maxLives; i++)
      {
        Color c = i < lives ? lifeColor : new Color(45, 45, 45);
        Draw.Rect(barPos.X + i * (segmentWidth + gap), barPos.Y, segmentWidth, segmentHeight, c);
      }
    }

    private static void StartRound_patch(On.TowerFall.Session.orig_StartRound orig, global::TowerFall.Session self)
    {
      orig(self);
      for (int i = 0; i < TFGame.Players.Length; i++)
      {
        LivesRemaining[i] = TFGame.Players[i] ? PlayerHandicap.GetStartingLives(i) : 0;
        ShouldRespawn[i] = false;
        HasRoundSpawnPosition[i] = false;
        ImmunityFramesRemaining[i] = 0f;
      }

      foreach (Entity entity in self.CurrentLevel.Players)
      {
        if (entity is global::TowerFall.Player player)
        {
          int p = player.PlayerIndex;
          RoundSpawnPositions[p] = player.Position;
          HasRoundSpawnPosition[p] = true;
        }
      }
    }

    private static void Update_patch(On.TowerFall.Player.orig_Update orig, global::TowerFall.Player self)
    {
      int p = self.PlayerIndex;
      bool shootLockedByImmunity = ImmunityFramesRemaining[p] > 0f;
      bool previousShootLock = global::TowerFall.Player.ShootLock;
      if (shootLockedByImmunity)
      {
        global::TowerFall.Player.ShootLock = true;
      }

      orig(self);

      if (shootLockedByImmunity)
      {
        global::TowerFall.Player.ShootLock = previousShootLock;
        ImmunityFramesRemaining[p] = Math.Max(0f, ImmunityFramesRemaining[p] - Engine.TimeMult);
      }
    }

    private static void HurtBouncedOn_patch(On.TowerFall.Player.orig_HurtBouncedOn orig, global::TowerFall.Player self, int bouncerIndex)
    {
      if (bouncerIndex >= 0 &&
          bouncerIndex < ImmunityFramesRemaining.Length &&
          ImmunityFramesRemaining[bouncerIndex] > 0f)
      {
        return;
      }

      orig(self, bouncerIndex);
    }

    private static void OnPlayerDeath_patch(On.TowerFall.Session.orig_OnPlayerDeath orig, global::TowerFall.Session self, global::TowerFall.Player player, global::TowerFall.PlayerCorpse corpse, int playerIndex, DeathCause deathType, Vector2 position, int killerIndex)
    {
      // Check if we're in instant respawn mode
      ref int lives = ref LivesRemaining[playerIndex];

      if (lives > 1)
      {
        lives--;

        orig(self, player, corpse, playerIndex, deathType, position, killerIndex);

        Alarm.Set(corpse, 1, delegate
        {
          RespawnPlayer(self, playerIndex, player.Allegiance, player.TeamColor);
        });

        return;
      }
      else if (lives == 1)
      {
        lives--;
      }

      // Call original death handling (only if not instant respawn or last life)
      orig(self, player, corpse, playerIndex, deathType, position, killerIndex);
    }

    private static void RespawnPlayer(global::TowerFall.Session session, int playerIndex, Allegiance allegiance, Allegiance teamColor)
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

      var newPlayer = new global::TowerFall.Player(
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

      int immunityFrames = Math.Max(0, TFModFortRisePotoModule.Settings.handicapRespawnImmunitySeconds * FramesPerSecond);
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
