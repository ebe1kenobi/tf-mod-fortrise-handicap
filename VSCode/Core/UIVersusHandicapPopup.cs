using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using TowerFall;

namespace TFModFortRisePoto
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

    public static bool IsOpen { get; private set; }

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
      IsOpen = true;
      if (ownerButton != null)
        ownerButton.Selected = false;

      Sounds.ui_pause.Play(160f);
    }

    public override void Removed()
    {
      base.Removed();
      IsOpen = false;
      Sounds.ui_unpause.Play(160f);
      MenuInput.Clear();

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

      if (MenuInput.Alt)
      {
        selectedField = 1 - selectedField;
        Sounds.ui_move2.Play(160f, 1f);
        return;
      }

      if (MenuInput.Left)
      {
        if (selectedField == 0)
          PlayerHandicap.AdjustVictories(playerIndex, -1);
        else
          PlayerHandicap.AdjustLives(playerIndex, -1);

        Sounds.ui_click.Play(160f, 1f);
        return;
      }

      if (MenuInput.Right)
      {
        if (selectedField == 0)
          PlayerHandicap.AdjustVictories(playerIndex, 1);
        else
          PlayerHandicap.AdjustLives(playerIndex, 1);

        Sounds.ui_click.Play(160f, 1f);
        return;
      }

      if (MenuInput.Confirm || MenuInput.Back)
        RemoveSelf();
    }

    public override void Render()
    {
      Draw.Rect(0, 0, 320, 240, Color.Black * 0.7f);
      Draw.OutlineTextCentered(TFGame.Font, "HANDICAP", Position + new Vector2(0f, -72f), Color.White, 2f);
      Draw.TextCentered(TFGame.Font, "LEFT/RIGHT: AJUSTER", Position + new Vector2(0f, -58f), Color.Gray);
      Draw.TextCentered(TFGame.Font, "UP/DOWN: JOUEUR  ALT: CHAMP", Position + new Vector2(0f, -46f), Color.Gray);

      float rowY = Position.Y - 24f;
      for (int i = 0; i < activePlayers.Count; i++)
      {
        int playerIndex = activePlayers[i];
        bool selected = i == selectedPlayerIndex;
        Color rowColor = selected ? Calc.HexToColor("F87858") : Color.White;
        string prefix = selected ? "> " : "  ";
        string playerLabel = prefix + "P" + (playerIndex + 1);
        Vector2 rowPos = new Vector2(Position.X - 70f, rowY);

        Draw.Text(TFGame.Font, playerLabel, rowPos, ArcherData.GetColorA(playerIndex, Allegiance.Neutral));
        Draw.Text(TFGame.Font, "V:", rowPos + new Vector2(28f, 0f), rowColor);
        Draw.Text(TFGame.Font, PlayerHandicap.GetVictoryHandicap(playerIndex).ToString(), rowPos + new Vector2(40f, 0f), selected && selectedField == 0 ? Calc.HexToColor("FFEC5E") : rowColor);
        Draw.Text(TFGame.Font, "L:", rowPos + new Vector2(58f, 0f), rowColor);
        Draw.Text(TFGame.Font, PlayerHandicap.GetLivesHandicap(playerIndex).ToString(), rowPos + new Vector2(70f, 0f), selected && selectedField == 1 ? Calc.HexToColor("FFEC5E") : rowColor);

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

      Draw.TextCentered(TFGame.Font, "V = VICTOIRES (TETES DE MORT)", Position + new Vector2(0f, 62f), Color.Gray);
      Draw.TextCentered(TFGame.Font, "L = VIES SUPPLEMENTAIRES", Position + new Vector2(0f, 74f), Color.Gray);
      Draw.TextCentered(TFGame.Font, "CONFIRMER / RETOUR: FERMER", Position + new Vector2(0f, 88f), Color.Gray);
    }
  }
}
