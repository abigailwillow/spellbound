using UnityEngine;
using Photon.Pun;

public class NameMenuState : BaseMenuState {
    public NameMenuState(MenuStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) { }

    public override void Enter() => this.gameManager.UIManager.SetInstruction("Please enter your name");

    public override void OnInputSubmitted(PlayerController player, string input) {
        PhotonNetwork.LocalPlayer.NickName = input;
        this.gameManager.LocalPlayer.NameUpdated?.Invoke(this.gameManager.LocalPlayer, input);
        PlayerPrefs.SetString("Nickname", input);
        this.gameManager.UIManager.SetInstruction($"Your name is now {input.ToUpper()}", () => this.stateMachine.SetMenuState(MenuState.Menu));

        Debug.Log($"Player {player.photonView.ViewID} changed their name to {input}");
    }
}