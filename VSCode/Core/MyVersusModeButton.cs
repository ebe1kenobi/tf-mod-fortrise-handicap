using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TFModFortRisePoto
{
  internal class MyVersusModeButton
  {
    internal static void Load()
    {
      On.TowerFall.VersusModeButton.Update += Update_patch;
      On.TowerFall.VersusModeButton.Render += Render_patch;
    }

    internal static void Unload()
    {
      On.TowerFall.VersusModeButton.Update -= Update_patch;
      On.TowerFall.VersusModeButton.Render -= Render_patch;
    }

    private static bool AnyPlayerArrowsPressed()
    {
      for (int i = 0; i < TFGame.PlayerInputs.Length; i++)
      {
        PlayerInput input = TFGame.PlayerInputs[i];
        if (input != null && input.GetState().ArrowsPressed)
          return true;
      }

      return false;
    }

    private static void OpenHandicapPopup(global::TowerFall.VersusModeButton self)
    {
      if (UIVersusHandicapPopup.IsOpen || self.Scene == null)
        return;

      Sounds.ui_click.Play(160f, 1f);
      self.Scene.Add(new UIVersusHandicapPopup(self));
    }

    private static void Update_patch(On.TowerFall.VersusModeButton.orig_Update orig, global::TowerFall.VersusModeButton self)
    {
      if (MainMenu.VersusMatchSettings.Mode != Modes.HeadHunters
          && MainMenu.VersusMatchSettings.Mode != Modes.LastManStanding
          && MainMenu.VersusMatchSettings.Mode != Modes.TeamDeathmatch) {
        orig(self);
        return;
      }
      if (self.Selected && !UIVersusHandicapPopup.IsOpen && AnyPlayerArrowsPressed())
      {
        OpenHandicapPopup(self);
        return;
      }

      orig(self);
    }

    private static void Render_patch(On.TowerFall.VersusModeButton.orig_Render orig, global::TowerFall.VersusModeButton self)
    {
      orig(self);

      if (!self.Selected || UIVersusHandicapPopup.IsOpen)
        return;

      if (MainMenu.VersusMatchSettings.Mode != Modes.HeadHunters
          && MainMenu.VersusMatchSettings.Mode != Modes.LastManStanding
          && MainMenu.VersusMatchSettings.Mode != Modes.TeamDeathmatch)
      {
        return;
      }

      Vector2 hintPos = self.Position + new Vector2(0f, 22f);
      Draw.OutlineTextCentered(TFGame.Font, "Y: HANDICAP/LIFE", hintPos, Calc.HexToColor("FFEC5E"), 1f);

      if (!PlayerHandicap.HasAnyHandicap())
        return;
    }
  }
}
