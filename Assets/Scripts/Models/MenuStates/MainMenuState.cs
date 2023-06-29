using Photon.Pun;

public class MainMenuState : BaseMenuState {
    public MainMenuState(MenuStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) {
        this.AddInput("start", (player, input) => {
            if (PhotonNetwork.LocalPlayer.NickName != string.Empty) {
                this.gameManager.SetGameState(GameState.Connecting);
                PhotonNetwork.LeaveRoom();
            } else {
                this.gameManager.UIManager.SetInstruction("Please pick a name first", () => this.Enter());
            }
        });
        this.AddInput("name", (player, input) => this.stateMachine.SetMenuState(MenuState.Name));
        this.AddInput("tutorial", (player, input) => this.stateMachine.SetMenuState(MenuState.Tutorial));
        this.AddInput("sprite", (player, input) => this.stateMachine.SetMenuState(MenuState.Sprite));
    }

    public override void Enter() {
        this.gameManager.UIManager.SetInstruction("Type START to start the game\nType NAME to change your name\nType SPRITE to change your sprite\nType TUTORIAL to play the tutorial");
    }
}