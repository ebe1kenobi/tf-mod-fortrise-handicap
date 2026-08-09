using Monocle;
using TowerFall;

namespace TFModFortRiseHandicap
{
  /// <summary>
  /// Per-player handicap configured from the versus match settings screen.
  /// Victory handicap adds starting score (skulls/coins on round results).
  /// Lives handicap adds extra lives in multi-life modes.
  /// </summary>
  public static class PlayerHandicap
  {
    public const int MaxVictoryHandicap = 1000;
    public const int MaxLivesHandicap = 1000;
    public const int MaxImmunitySeconds = 10;

    private static readonly int[] VictoryHandicap = new int[8];
    private static readonly int[] LivesHandicap = { 1,1,1,1,1,1,1,1}; 

    public static int GetVictoryHandicap(int playerIndex)
    {
      if (playerIndex < 0 || playerIndex >= VictoryHandicap.Length)
        return 0;

      return VictoryHandicap[playerIndex];
    }

    public static int GetLivesHandicap(int playerIndex)
    {
      if (playerIndex < 0 || playerIndex >= LivesHandicap.Length)
        return 0;

      return LivesHandicap[playerIndex];
    }

    public static void AdjustVictories(int playerIndex, int delta)
    {
      if (playerIndex < 0 || playerIndex >= VictoryHandicap.Length)
        return;

      VictoryHandicap[playerIndex] = Calc.Clamp(VictoryHandicap[playerIndex] + delta, 0, MaxVictoryHandicap);
      SyncSettings();
    }

    public static void AdjustLives(int playerIndex, int delta)
    {
      if (playerIndex < 0 || playerIndex >= LivesHandicap.Length)
        return;

      LivesHandicap[playerIndex] = Calc.Clamp(LivesHandicap[playerIndex] + delta, 1, MaxLivesHandicap);
      SyncSettings();
    }

    /// <summary>Delai d'immunite, valeur globale rangee dans les reglages du module.</summary>
    public static int GetImmunitySeconds()
    {
      var settings = TFModFortRiseHandicapModule.Settings;
      return settings != null ? settings.handicapRespawnImmunitySeconds : 0;
    }

    public static void AdjustImmunity(int delta)
    {
      SetImmunity(GetImmunitySeconds() + delta);
    }

    public static void SetImmunity(int value)
    {
      var settings = TFModFortRiseHandicapModule.Settings;
      if (settings == null)
        return;

      settings.handicapRespawnImmunitySeconds = Calc.Clamp(value, 0, MaxImmunitySeconds);
    }

    /// <summary>
    /// Applique la meme valeur a tous les joueurs. C'est ce que font les reglages du
    /// module, qui n'ont pas de notion de joueur.
    /// </summary>
    public static void SetVictoriesForAll(int value)
    {
      int clamped = Calc.Clamp(value, 0, MaxVictoryHandicap);
      for (int i = 0; i < VictoryHandicap.Length; i++)
        VictoryHandicap[i] = clamped;
    }

    public static void SetLivesForAll(int value)
    {
      int clamped = Calc.Clamp(value, 1, MaxLivesHandicap);
      for (int i = 0; i < LivesHandicap.Length; i++)
        LivesHandicap[i] = clamped;
    }

    /// <summary>
    /// Remonte les valeurs vers les reglages du module, uniquement quand tous les
    /// joueurs partagent la meme : un reglage global ne peut pas representer quatre
    /// valeurs differentes, et l'ecraser afficherait une valeur trompeuse.
    /// </summary>
    public static void SyncSettings()
    {
      var settings = TFModFortRiseHandicapModule.Settings;
      if (settings == null)
        return;

      int common = Common(VictoryHandicap);
      if (common >= 0 && settings.handicapVictories != common)
        settings.handicapVictories = common;

      common = Common(LivesHandicap);
      if (common >= 0 && settings.handicapLives != common)
        settings.handicapLives = common;
    }

    /// <summary>
    /// Valeur partagee par les joueurs ACTIFS, ou -1 si elles different.
    ///
    /// Ne considerer que les joueurs actifs est essentiel : la popup ne liste
    /// qu'eux, alors que le tableau compte huit entrees. Regler les deux joueurs
    /// d'une partie laissait les six autres a leur valeur par defaut, donc jamais de
    /// valeur commune, et le reglage du module n'etait jamais mis a jour.
    /// </summary>
    private static int Common(int[] values)
    {
      int first = -1;

      for (int i = 0; i < values.Length && i < TFGame.Players.Length; i++)
      {
        if (!TFGame.Players[i])
          continue;

        if (first < 0)
          first = values[i];
        else if (values[i] != first)
          return -1;
      }

      return first;
    }

    public static bool HasAnyHandicap()
    {
      for (int i = 0; i < LivesHandicap.Length; i++)
      {
        if (VictoryHandicap[i] > 0 || LivesHandicap[i] > 1)
          return true;
      }

      return false;
    }

    public static int GetStartingLives(int playerIndex)
    {
      return GetLivesHandicap(playerIndex);
    }

    /// <summary>
    /// Applique l'avance en victoires au demarrage du match (une seule fois).
    /// On met a jour Scores ET OldScores pour que l'ecran de fin de manche
    /// n'anime pas un gain de points qui n'a pas eu lieu.
    /// </summary>
    public static void ApplyVictoryHandicap(Session session)
    {
      if (session == null)
        return;

      for (int scoreIndex = 0; scoreIndex < session.Scores.Length; scoreIndex++)
      {
        int bonus = 0;
        for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++)
        {
          if (!TFGame.Players[playerIndex])
            continue;

          if (session.GetScoreIndex(playerIndex) == scoreIndex)
            bonus += GetVictoryHandicap(playerIndex);
        }

        if (bonus <= 0)
          continue;

        session.Scores[scoreIndex] += bonus;
        session.OldScores[scoreIndex] += bonus;
      }
    }
  }
}
