using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks {
    public static GameManager instance { get; private set; }
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject remotePlayerPrefab;
    public LocalPlayerController localPlayer { get; private set; }
    public RemotePlayerController remotePlayer { get; private set; }
    private UserInterfaceManager userInterfaceManager;

    private void Awake() {
        # region Singleton
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            instance = this;
        }
        # endregion

        PhotonNetwork.ConnectUsingSettings();
    }

    private void Start() {
        this.userInterfaceManager = UserInterfaceManager.instance;
        this.userInterfaceManager.SetPlayerPanelActive(false, PlayerType.Remote);
        this.userInterfaceManager.SetPlayerPanelActive(true, PlayerType.Local);
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinRandomRoom();

    public override void OnConnected() => Debug.Log($"Connected to {PhotonNetwork.Server}");

    public override void OnJoinedRoom() {
        this.localPlayer = Instantiate(localPlayerPrefab).GetComponent<LocalPlayerController>();
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) {
            this.InstantiateRemotePlayer();
        }
        Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");       
    }

    public override void OnPlayerEnteredRoom(Player remotePlayer) {
        this.InstantiateRemotePlayer();
        Debug.Log($"Remote player joined the room");
    }

    public override void OnPlayerLeftRoom(Player remotePlayer) {
        this.DestroyRemotePlayer();
        Debug.Log($"Remote player left the room");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log($"Could not join a random room ({message}), creating a new room");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }

    public void InstantiateRemotePlayer() {
        this.remotePlayer = Instantiate(remotePlayerPrefab).GetComponent<RemotePlayerController>();
        this.userInterfaceManager.SetPlayerPanelActive(true, PlayerType.Remote);
    }

    public void DestroyRemotePlayer() {
        Destroy(this.remotePlayer.gameObject);
        this.remotePlayer = null;
        this.userInterfaceManager.SetPlayerPanelActive(false, PlayerType.Remote);
    }
}
