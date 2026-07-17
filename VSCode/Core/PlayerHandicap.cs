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
    public const int MaxVictoryHandicap = 1000; 
    public const int MaxLivesHandicap = 1000;

    private static readonly int[] VictoryHandicap = new int[8];
    private static readonly int[] LivesHandicap = new int[8]; 

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
    }

    public static void AdjustLives(int playerIndex, int delta)
    {
      if (playerIndex < 0 || playerIndex >= LivesHandicap.Length)
        return;

      LivesHandicap[playerIndex] = Calc.Clamp(LivesHandicap[playerIndex] + delta, 0, MaxLivesHandicap);
    }

    public static bool HasAnyHandicap()
    {
      for (int i = 0; i < LivesHandicap.Length; i++)
      {
        if (VictoryHandicap[i] > 0 || LivesHandicap[i] > 0)
          return true;
      }

      return false;
    }

    public static int GetStartingLives(int playerIndex)
    {
      return GetLivesHandicap(playerIndex);
    }

    //public static void ApplyVictoryHandicap(global::TowerFall.Session session)
    //{
    //  if (session == null)
    //    return;

    //  for (int scoreIndex = 0; scoreIndex < session.Scores.Length; scoreIndex++)
    //  {
    //    int bonus = 0;
    //    for (int playerIndex = 0; playerIndex < TFGame.Players.Length; playerIndex++) //todo 8
    //    {
    //      if (!TFGame.Players[playerIndex])
    //        continue;

    //      if (session.GetScoreIndex(playerIndex) == scoreIndex)
    //        bonus += GetVictoryHandicap(playerIndex);
    //    }

    //    if (bonus <= 0)
    //      continue;

    //    session.Scores[scoreIndex] += bonus;
    //    session.OldScores[scoreIndex] += bonus;
    //  }
    //}
  }
}
