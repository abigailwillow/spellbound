using UnityEngine;
using Photon.Pun;

public class ConnectingMenuState : BaseMenuState {
    public ConnectingMenuState(MenuStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) {
        this.AddInput("cancel", (player, input) => {
            this.gameManager.SetGameState(GameState.Menu);
            PhotonNetwork.LeaveRoom();
        });
    }
    private string[] connectingMesages = new string[] { "Connecting...", "Finding opponent...", "Still looking...", "Almost there...", "You can type CANCEL to return..." };
    private float connectingMessageDelay = 10f;
    private float connectingMessageStartTime = 0f;

    public override void Enter() => this.connectingMessageStartTime = 0f;

    public override void Update() {
        if (this.connectingMessageStartTime <= Time.time) {
            this.connectingMessageStartTime = this.connectingMessageDelay + Time.time;
            this.gameManager.UIManager.SetInstruction(this.connectingMesages[Random.Range(0, this.connectingMesages.Length)]);
        }
    }
}