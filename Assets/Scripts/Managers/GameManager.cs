using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks {
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameObject LocalPlayerPrefab;
    [SerializeField] private GameObject RemotePlayerPrefab;
    public List<PlayerController> Players = new List<PlayerController>(2);
    public PlayerController LocalPlayer => Players.Find(player => player.PlayerType == PlayerType.Local);
    public PlayerController RemotePlayer => Players.Find(player => player.PlayerType == PlayerType.Remote);

    # region Events
    /// <summary>
    /// Called when a player is instantiated
    /// </summary>
    public Action<PlayerController> OnPlayerInstantiated;
    /// <summary>
    /// Called when a player is destroyed
    /// </summary>
    public Action<PlayerType> OnPlayerDestroyed;
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
        this.InstantiatePlayer(this.LocalPlayerPrefab);
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2) {
            this.InstantiatePlayer(this.RemotePlayerPrefab);
        }
        Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");       
    }

    public override void OnPlayerEnteredRoom(Player remotePlayer) {
        this.InstantiatePlayer(this.RemotePlayerPrefab);
        Debug.Log($"Remote player joined the room");
    }

    public override void OnPlayerLeftRoom(Player remotePlayer) {
        this.DestroyPlayer(this.RemotePlayer);
        Debug.Log($"Remote player left the room");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log($"Could not join a random room ({message}), creating a new room");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 2 });
    }

    /// <summary>
    /// Instantiates a player and adds it to the list of players
    /// </summary>
    /// <param name="prefab">The prefab to instantiate, must correspond to either the LocalPlayerPrefab or RemotePlayerPrefab</param>
    private void InstantiatePlayer(GameObject prefab) {
        if (prefab != this.LocalPlayerPrefab && prefab != this.RemotePlayerPrefab) {
            Debug.LogError($"Prefab {prefab.name} is not a valid player prefab", this);
            return;
        }

        PlayerController player = Instantiate(prefab).GetComponent<PlayerController>();
        this.Players.Add(player);
        this.OnPlayerInstantiated?.Invoke(player);
    }

    /// <summary>
    /// Destroys a player and removes it from the list of players
    /// </summary>
    /// <param name="player">The player to be destroyed</param>
    private void DestroyPlayer(PlayerController player) {
        PlayerType playerType = player.PlayerType;
        this.Players.Remove(player);
        Destroy(player.gameObject);
        this.OnPlayerDestroyed?.Invoke(player.PlayerType);
    }
}
