using UnityEngine;

public class RemotePlayerController : BasePlayerController {
    protected override void Awake() {
        base.Awake();
        this.playerType = PlayerType.Remote;
    }
}
