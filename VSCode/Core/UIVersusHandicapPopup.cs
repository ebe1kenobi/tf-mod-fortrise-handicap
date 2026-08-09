using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TFModFortRiseHandicap
{
  public class UIVersusHandicapPopup : Entity
  {
    private readonly BorderButton ownerButton;
    private readonly List<int> activePlayers = new List<int>();
    private int selectedPlayerIndex;
    private int selectedField;

    public UIVersusHandicapPopup(BorderButton ownerButton)
    {
      this.ownerButton = ownerButton;
      Position = new Vector2(160f, 120f);
      RefreshActivePlayers();
    }

    public UIVersusHandicapPopup(MainMenu ownerButton)
    {
      //this.ownerButton = ownerButton;
      Position = new Vector2(160f, 120f);
      RefreshActivePlayers();
    }

    //public static bool IsOpen { get; private set; }
    public static UIVersusHandicapPopup Current;

    public static bool IsOpen => Current != null && Current.Scene == Engine.Instance.Scene;


    private void RefreshActivePlayers()
    {
      activePlayers.Clear();
      for (int i = 0; i < TFGame.Players.Length; i++) //todo 8 ok length
      {
        if (TFGame.Players[i])
          activePlayers.Add(i);
      }

      if (activePlayers.Count == 0)
        activePlayers.Add(0);

      if (selectedPlayerIndex >= activePlayers.Count)
        selectedPlayerIndex = activePlayers.Count - 1;
    }

    public override void Added()
    {
      base.Added();
      Current = this;
      if (ownerButton != null)
        ownerButton.Selected = false;
      // Bloque les entrees du menu en arriere-plan tant que la popup est ouverte :
      // - MainMenu.Update ne traite plus MenuInput.Back (retour ecran precedent)
      // - VersusBeginButton.Update ne traite plus MenuInput.Start (lancement du match)
      // Meme pattern que le popup vanilla ClearAllData et que UISpeedRunPopup.
      MainMenu menu = Scene as MainMenu;
      if (menu != null)
        menu.CanAct = false;
      Sounds.ui_pause.Play(160f);
    }

    public override void Removed()
    {
      base.Removed();

      // Les reglages ne sont ecrits sur disque qu'en sortant du menu Options du
      // jeu : sans cet appel, une valeur changee ici serait perdue en quittant.
      TFModFortRiseHandicapModule.SaveSettingsNow();

      if (Current == this)
        Current = null;
      Sounds.ui_unpause.Play(160f);
      MenuInput.Clear();
      MainMenu menu = Scene as MainMenu;
      if (menu != null)
        menu.CanAct = true;
      if (ownerButton != null)
        ownerButton.Selected = true;
    }

    public override void Update()
    {
      base.Update();
      MenuInput.Update();
      RefreshActivePlayers();

      if (activePlayers.Count == 0)
        return;

      int playerIndex = activePlayers[selectedPlayerIndex];

      if (MenuInput.Up && selectedPlayerIndex > 0)
      {
        selectedPlayerIndex--;
        Sounds.ui_move1.Play(160f, 1f);
        return;
      }

      if (MenuInput.Down && selectedPlayerIndex < activePlayers.Count - 1)
      {
        selectedPlayerIndex++;
        Sounds.ui_move1.Play(160f, 1f);
        return;
      }

      // Trois champs desormais : victoires, vies, puis immunite. Cette derniere
      // est globale, pas per-joueur : elle vit dans les reglages du module.
      if (MenuInput.Alt)
      {
        selectedField = (selectedField + 1) % 3;
        Sounds.ui_move2.Play(160f, 1f);
        return;
      }

      if (MenuInput.Left)
      {
        Adjust(playerIndex, -1);
        return;
      }

      if (MenuInput.Right)
      {
        Adjust(playerIndex, 1);
        return;
      }

      if (MenuInput.Confirm || MenuInput.Back)
        RemoveSelf();
    }

    private void Adjust(int playerIndex, int delta)
    {
      if (selectedField == 0)
        PlayerHandicap.AdjustVictories(playerIndex, delta);
      else if (selectedField == 1)
        PlayerHandicap.AdjustLives(playerIndex, delta);
      else
        PlayerHandicap.AdjustImmunity(delta);

      Sounds.ui_click.Play(160f, 1f);
    }

    public override void Render()
    {
      Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
      Draw.OutlineTextCentered(TFGame.Font, "HANDICAP", Position + new Vector2(0f, -72f), Color.White, 2f);
      Draw.TextCentered(TFGame.Font, "LEFT/RIGHT: ADJUST", Position + new Vector2(0f, -58f), Color.Gray);
      Draw.TextCentered(TFGame.Font, "UP/DOWN: PLAYER  ALT: FIELD", Position + new Vector2(0f, -46f), Color.Gray);

      float rowY = Position.Y - 24f;
      for (int i = 0; i < activePlayers.Count; i++)
      {
        int playerIndex = activePlayers[i];
        bool selected = i == selectedPlayerIndex;
        Color rowColor = selected ? Calc.HexToColor("F87858") : Color.White;
        string prefix = selected ? "> " : "  ";
        string playerLabel = prefix + "P" + (playerIndex + 1);
        Vector2 rowPos = new Vector2(Position.X - 70f, rowY);

        //Draw.Text(TFGame.Font, playerLabel, rowPos, ArcherData.GetColorA(playerIndex, Allegiance.Neutral));
        //Draw.Text(TFGame.Font, "V:", rowPos + new Vector2(28f, 0f), rowColor);
        //Draw.Text(TFGame.Font, PlayerHandicap.GetVictoryHandicap(playerIndex).ToString(), rowPos + new Vector2(40f, 0f), selected && selectedField == 0 ? Calc.HexToColor("FFEC5E") : rowColor);
        //Draw.Text(TFGame.Font, "L:", rowPos + new Vector2(58f, 0f), rowColor);
        //Draw.Text(TFGame.Font, PlayerHandicap.GetLivesHandicap(playerIndex).ToString(), rowPos + new Vector2(70f, 0f), selected && selectedField == 1 ? Calc.HexToColor("FFEC5E") : rowColor);

        Draw.Text(TFGame.Font, playerLabel, rowPos, ArcherData.GetColorA(playerIndex, Allegiance.Neutral));
        Draw.Text(TFGame.Font, "VICTORY:", rowPos + new Vector2(28f, 0f), rowColor);
        Draw.Text(TFGame.Font, PlayerHandicap.GetVictoryHandicap(playerIndex).ToString(), rowPos + new Vector2(72f, 0f), selected && selectedField == 0 ? Calc.HexToColor("FFEC5E") : rowColor);
        Draw.Text(TFGame.Font, "LIFE:", rowPos + new Vector2(94f, 0f), rowColor);
        Draw.Text(TFGame.Font, PlayerHandicap.GetLivesHandicap(playerIndex).ToString(), rowPos + new Vector2(122f, 0f), selected && selectedField == 1 ? Calc.HexToColor("FFEC5E") : rowColor);


        //if (PlayerHandicap.GetVictoryHandicap(playerIndex) > 0)
        //{
        //  Draw.TextureCentered(
        //      TFGame.Atlas["versus/skull"],
        //      rowPos + new Vector2(92f, 4f),
        //      Color.White * (selected ? 1f : 0.7f),
        //      Vector2.One * 0.5f,
        //      0f);
        //}

        rowY += 14f;
      }

      // L'immunite est commune a tous les joueurs : une seule ligne, sous la liste,
      // plutot qu'une colonne repetee a l'identique sur chaque ligne.
      bool immunitySelected = selectedField == 2;
      Color immunityColor = immunitySelected ? Calc.HexToColor("FFEC5E") : Color.White;
      Draw.TextCentered(TFGame.Font,
          (immunitySelected ? "> " : "  ") + "IMMUNITY (ALL): " + PlayerHandicap.GetImmunitySeconds() + "S",
          Position + new Vector2(0f, 50f), immunityColor);

      Draw.TextCentered(TFGame.Font, "VICTORY = VICTORIES", Position + new Vector2(0f, 66f), Color.Gray);
      Draw.TextCentered(TFGame.Font, "LIFE = NUMBER OF LIVES", Position + new Vector2(0f, 78f), Color.Gray);
      Draw.TextCentered(TFGame.Font, "CONFIRM / BACK: CLOSE", Position + new Vector2(0f, 92f), Color.Gray);
    }
  }
}
