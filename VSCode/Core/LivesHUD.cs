using System;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TFModFortRiseHandicap
{
  /// <summary>
  /// La barre de vies, au-dessus de chaque archer.
  ///
  /// **Pourquoi une entite, et non un patch de <c>Player.HUDRender</c>.** C'est la
  /// qu'elle vivait, en postfix, et c'est pour cela qu'elle avait disparu : HUDRender
  /// tient en trois lignes, donc le JIT la recopie chez son appelant et le patch n'a
  /// plus rien a intercepter. Le meme piege a coute le decompte de PlayTag et la barre
  /// de vies du mod Respawn.
  ///
  /// Une entite ne fait pas ce pari : Monocle appelle son <c>Render</c> parce qu'elle
  /// est dans la scene, pas parce qu'on a reussi a s'accrocher a une methode.
  ///
  /// **Portee.** Elle n'est ajoutee qu'au debut d'une manche ou le handicap s'applique,
  /// et elle revérifie a chaque image. Rien n'est dessine dans les autres parties ni
  /// dans le jeu de base.
  /// </summary>
  public class LivesHUD : Entity
  {
    /// <summary>Devant les archers.</summary>
    public const int DEPTH = -120;

    private const float SEGMENT = 3f;
    private const float GAP = 1f;

    /// <summary>Au-dela, la barre deborde de l'archer : on ecrit le nombre.</summary>
    private const int MAX_SEGMENTS = 8;

    public LivesHUD() : base(Vector2.Zero)
    {
      Depth = DEPTH;
    }

    public override void Render()
    {
      base.Render();

      Level level = Scene as Level;

      if (level?.Session == null
          || !TFModFortRiseHandicapModule.IsHandicapMode(level.Session.MatchSettings))
      {
        return;
      }

      foreach (Entity entity in level[GameTags.Player])
      {
        var player = entity as Player;

        if (player == null || player.Dead)
        {
          continue;
        }

        Draw(player);
      }
    }

    private static void Draw(Player player)
    {
      int playerIndex = player.PlayerIndex;

      int maxLives = Math.Max(1, PlayerHandicap.GetStartingLives(playerIndex));
      int lives = Math.Max(0, MyRespawnPlayer.LivesRemaining[playerIndex]);

      // Une seule vie : la barre n'apprendrait rien.
      if (maxLives <= 1)
      {
        return;
      }

      // Accroupi, l'archer est plus bas et la barre lui rentrerait dedans ; invisible,
      // l'annoncer trahirait sa position.
      if (player.State == Player.PlayerStates.Ducking || player.Invisible)
      {
        return;
      }

      if (maxLives > MAX_SEGMENTS)
      {
        Monocle.Draw.OutlineTextCentered(TFGame.Font, lives.ToString(),
            player.Position + new Vector2(0f, -22f), Color.White, 1f);
        return;
      }

      float barWidth = maxLives * SEGMENT + (maxLives - 1) * GAP;
      Vector2 barPos = (player.Position + new Vector2(-barWidth * 0.5f, -12.5f)).Floor();

      Monocle.Draw.Rect(barPos.X - 1f, barPos.Y - 1f, barWidth + 2f, SEGMENT + 2f, Color.Black * 0.75f);

      for (int i = 0; i < maxLives; i++)
      {
        Color c = i < lives ? Color.LimeGreen : new Color(45, 45, 45);
        Monocle.Draw.Rect(barPos.X + i * (SEGMENT + GAP), barPos.Y, SEGMENT, SEGMENT, c);
      }
    }
  }
}
