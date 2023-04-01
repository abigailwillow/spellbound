using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks {
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject LocalPlayerPrefab;
    [SerializeField] private GameObject RemotePlayerPrefab;
    public PlayerController LocalPlayer { get; private set; }
    public PlayerController RemotePlayer { get; private set; }

    # region Events
    public delegate void PlayerInstantiatedHandler(PlayerController player);
    public event PlayerInstantiatedHandler OnPlayerInstantiated;
    public delegate void PlayerDestroyedHandler(PlayerController player);
    public event PlayerDestroyedHandler OnPlayerDestroyed;
    # endregion

    private void Awake() {
        # region Singleton
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            Instance = this;
        }
        # endregion

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinRandomRoom();

    public override void OnConnected() => Debug.Log($"Connected to {PhotonNetwork.Server}");

    public override void OnJoinedRoom() {
        this.InstantiatePlayer(PlayerType.Local);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) {
            this.InstantiatePlayer(PlayerType.Remote);
        }
        Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");       
    }

    public override void OnPlayerEnteredRoom(Player remotePlayer) {
        this.InstantiatePlayer(PlayerType.Remote);
        Debug.Log($"Remote player joined the room");
    }

    public override void OnPlayerLeftRoom(Player remotePlayer) {
        this.DestroyPlayer(PlayerType.Remote);
        Debug.Log($"Remote player left the room");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log($"Could not join a random room ({message}), creating a new room");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }

    private void InstantiatePlayer(PlayerType playerType) {
        if (playerType == PlayerType.Local) {
            this.LocalPlayer = Instantiate(LocalPlayerPrefab).GetComponent<PlayerController>();
            this.OnPlayerInstantiated?.Invoke(this.LocalPlayer);
        } else if (playerType == PlayerType.Remote) {
            this.RemotePlayer = Instantiate(RemotePlayerPrefab).GetComponent<PlayerController>();
            this.OnPlayerInstantiated?.Invoke(this.RemotePlayer);
        }
    }

    private void DestroyPlayer(PlayerType playerType) {
        if (playerType == PlayerType.Local) {
            this.OnPlayerDestroyed?.Invoke(this.LocalPlayer);
            Destroy(this.LocalPlayer.gameObject);
            this.LocalPlayer = null;
        } else if (playerType == PlayerType.Remote) {
            this.OnPlayerDestroyed?.Invoke(this.RemotePlayer);
            Destroy(this.RemotePlayer.gameObject);
            this.RemotePlayer = null;
        }
    }
}
