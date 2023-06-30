using System.Globalization;
using UnityEngine;

public class SpriteMenuState : BaseMenuState {
    private string[] names = { "sara", "senna", "ruby", "dexter", "emmet", "oscar" };

    public SpriteMenuState(MenuStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) {
        for (int i = 0; i < this.names.Length; i++) {
            int index = i;
            this.AddInput(this.names[i], (player, input) => {
                this.SetSprite(player, input, index);
            });
        }
    }

    public override void Enter() {
        this.gameManager.UIManager.SetInstruction($"Please choose your desired sprite");
        this.gameManager.UIManager.ToggleCharacterPreview(true);
    }

    public override void Exit() => this.gameManager.UIManager.ToggleCharacterPreview(false);

    private void SetSprite(PlayerController player, string input, int index) {
        int maxSprites = this.gameManager.LocalPlayer.Sprites.Length;
        string name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input);
        if (index >= 0 && index < maxSprites) {
            this.gameManager.LocalPlayer.SpriteIndex = index;
            PlayerPrefs.SetInt("SpriteIndex", index);

            this.gameManager.UIManager.SetInstruction($"Your character is now {name}", () => this.stateMachine.SetMenuState(MenuState.Menu));

            Debug.Log($"Player {player.photonView.ViewID} changed their character to {name} ({index})");
        } else {
            this.gameManager.UIManager.SetInstruction($"Invalid character {input}", () => this.stateMachine.SetMenuState(MenuState.Sprite));
        }
    }
}