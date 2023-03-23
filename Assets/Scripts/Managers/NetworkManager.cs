using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks {
    public static NetworkManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            Instance = this;
        }

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinRandomRoom();

    public override void OnConnected() => Debug.Log($"Connected to {PhotonNetwork.Server}");

    public override void OnJoinedRoom() => Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");

    public override void OnPlayerEnteredRoom(Player opponent) => Debug.Log($"Opponent joined the room");

    public override void OnPlayerLeftRoom(Player opponent) => Debug.Log($"Opponent left the room");

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log($"Could not join a random room ({message}), creating a new room");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }
}
