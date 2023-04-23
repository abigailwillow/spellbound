using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks {
    public static GameManager Instance { get; private set; }
    public List<PlayerController> Players { get; private set; } = new List<PlayerController>(2);
    public PlayerController LocalPlayer => Players.Find(player => player.PlayerType == PlayerType.Local);
    public PlayerController RemotePlayer => Players.Find(player => player.PlayerType == PlayerType.Remote);
    public WordDataList WordList { get; private set; }
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Binding localPlayerBinding;

    # region Events
    /// <summary>
    /// Called when a player is instantiated
    /// </summary>
    public Action<PlayerController> PlayerInstantiated;
    /// <summary>
    /// Called when a player is destroyed
    /// </summary>
    public Action<PlayerType> PlayerDestroyed;
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

        PhotonNetwork.PrefabPool.Register(this.playerPrefab);
        PhotonNetwork.ConnectUsingSettings();

        WordList = WordDataList.Deserialize(Resources.Load<TextAsset>("Words"));
        Debug.Log($"Loaded {WordList.Words.Count} words");
        Debug.Log(string.Join(", ", WordList.Words.ConvertAll(word => word.Word)));
    }

    public override void OnConnectedToMaster() => PhotonNetwork.JoinRandomRoom();

    public override void OnConnected() => Debug.Log($"Connected to {PhotonNetwork.Server}");

    public override void OnPlayerEnteredRoom(Player remotePlayer) => Debug.Log($"Remote player joined the room");

    // TODO: Destroy room on player disconnect
    public override void OnPlayerLeftRoom(Player remotePlayer) => Debug.Log($"Remote player left the room");

    public override void OnJoinedRoom() {
        PhotonNetwork.Instantiate(this.playerPrefab.name, this.playerPrefab.transform.position, this.playerPrefab.transform.rotation).GetComponent<PlayerController>().gameObject.AddComponent<InputController>().Binding = this.localPlayerBinding;
        Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log($"Could not join a random room ({message}), creating a new room");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }

    public bool AddPlayer(PlayerController player) {
        bool valid = this.Players.Count < 2;
        if (valid) {
            this.Players.Add(player);
            this.PlayerInstantiated?.Invoke(player);
        }
        return valid;
    }
}
