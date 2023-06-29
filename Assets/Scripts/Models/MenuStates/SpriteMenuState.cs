using System;
using UnityEngine;

public class SpriteMenuState : BaseMenuState {
    public SpriteMenuState(MenuStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override void Enter() {
        this.gameManager.UIManager.SetInstruction($"Please choose your desired sprite");
        this.gameManager.UIManager.ToggleCharacterPreview(true);
    }

    public override void Exit() => this.gameManager.UIManager.ToggleCharacterPreview(false);

    public override void OnInputSubmitted(PlayerController player, string input) {
        int maxSprites = this.gameManager.LocalPlayer.Sprites.Length;
        if (Char.TryParse(input, out char spriteChar) && spriteChar >= 'A' && spriteChar <= 'A' + maxSprites) {
            int index = spriteChar - 'A';
            this.gameManager.LocalPlayer.SpriteIndex = index;
            PlayerPrefs.SetInt("SpriteIndex", index);

            this.gameManager.UIManager.SetInstruction($"Your sprite is now {input}", () => this.stateMachine.SetMenuState(MenuState.Menu));

            Debug.Log($"Player {player.photonView.ViewID} changed their sprite to sprite {index}");
        } else {
            this.gameManager.UIManager.SetInstruction($"Invalid sprite {input}", () => this.stateMachine.SetMenuState(MenuState.Sprite));
        }
    }
}