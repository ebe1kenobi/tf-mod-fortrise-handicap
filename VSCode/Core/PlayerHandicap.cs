using Monocle;
using TowerFall;

namespace TFModFortRisePoto
{
  /// <summary>
  /// Per-player handicap configured from the versus match settings screen.
  /// Victory handicap adds starting score (skulls/coins on round results).
  /// Lives handicap adds extra lives in multi-life modes.
  /// </summary>
  public static class PlayerHandicap
  {
    public const int MaxVictoryHandicap = 10; //todo adjust  with match length
    public const int MaxLivesHandicap = 10; //todo adjust with settings life new settings

    private static readonly int[] VictoryHandicap = new int[8]; // todo 8
    private static readonly int[] LivesHandicap = new int[8];  // todo 8

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
      if (playerIndex < 0 || playerIndex >= VictoryHandicap.Length) //todo 8
        return;

      VictoryHandicap[playerIndex] = Calc.Clamp(VictoryHandicap[playerIndex] + delta, 0, MaxVictoryHandicap); //todo adjust with match length
    }

    public static void AdjustLives(int playerIndex, int delta)
    {
      if (playerIndex < 0 || playerIndex >= LivesHandicap.Length)
        return;

      LivesHandicap[playerIndex] = Calc.Clamp(LivesHandicap[playerIndex] + delta, 0, MaxLivesHandicap);
    }

    public static bool HasAnyHandicap()
    {
      for (int i = 0; i < 8; i++) //todo 8
      {
        if (VictoryHandicap[i] > 0 || LivesHandicap[i] > 0)
          return true;
      }

      return false;
    }

    public static int GetStartingLives(int playerIndex)
    {
      //todo return 1 if not mode or not liveinit popup > 0
      return GetLivesHandicap(playerIndex); //todo correction plantage
    }

    public static void ApplyVictoryHandicap(global::TowerFall.Session session)
    {
      if (session == null)
        return;

      for (int scoreIndex = 0; scoreIndex < session.Scores.Length; scoreIndex++)
      {
        int bonus = 0;
        for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++) //todo 8
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
