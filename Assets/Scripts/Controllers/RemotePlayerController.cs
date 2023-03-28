using UnityEngine;
using Photon.Pun;

public class RemotePlayerController : BasePlayerController {
    protected override void Awake() {
        this.playerType = PlayerType.Remote;
        base.Awake();
    }

    [PunRPC]
    public override void Submit(string input) {
        base.Submit(input);
        UserInterfaceManager.instance.UpdatePlayerInput(input, PlayerType.Remote);
    }
}
