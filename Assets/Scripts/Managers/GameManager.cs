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
    public readonly int MAX_PLAYERS = 2;
    public GameState GameState { get; private set; } = GameState.None;
    private MenuState menuState = MenuState.None;
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject userInterfacePrefab;
    [Header("Miscellaneous")]
    [SerializeField] private Binding localPlayerBinding;
    public LetterValues LetterValues;
    private UserInterfaceManager uiManager;
    private int turnCount = -1;
    private string[] connectingMesages = new string[] { "Connecting...", "Finding opponent...", "Still looking...", "Almost there...", "You can type CANCEL to return..." };
    private float connectingMessageDelay = 10f;
    private float connectingMessageStartTime = 0f;
    private PlayerType winner = PlayerType.None;
    private WinReason winReason = WinReason.None;
    [SerializeField] private int maxTurnTime = 30;
    public float TurnStarted { get; private set; } = 0;
    private bool turnSkipped = false;

    # region Events
    /// <summary>
    /// Called when a player is instantiated
    /// </summary>
    public Action<PlayerController> PlayerInstantiated;
    /// <summary>
    /// Called when a player is destroyed
    /// </summary>
    public Action<PlayerType> PlayerDestroyed;
    public Action<GameState> GameStateChanged;
    /// <summary>
    /// Called when the next turn is started. The first parameter is the previous player, the second parameter is the current player, and the third parameter is the turn count
    /// </summary>
    public Action<PlayerController, PlayerController, int> TurnIncremented;
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

        WordList = WordDataList.Deserialize(Resources.Load<TextAsset>("ExtendedWords"));
        Debug.Log($"Loaded {WordList.Words.Count} words");
        Debug.Log($"Valid Words: {string.Join(", ", WordList.Words.ConvertAll(word => word.Word))}");

        List<string> debugValues = new List<string>();
        foreach (char letter in "abcdefghijklmnopqrstuvwxyz") debugValues.Add($"{letter}: {this.LetterValues.GetValue(letter)}");
        Debug.Log($"Letter Values: {string.Join(", ", debugValues)}");

        this.uiManager = Instantiate(this.userInterfacePrefab).GetComponent<UserInterfaceManager>();

        if (PlayerPrefs.GetInt("CompletedTutorial", 0) == 0) {
            this.SetMenuState(MenuState.Tutorial);
        }
    }

    private void Update() {
        // Position players
        float cameraHalfWidth = (Camera.main.orthographicSize * Camera.main.aspect) / 2;
        this.Players.ForEach(player => {
            if (!player) return;
            player.transform.position = new Vector3(cameraHalfWidth * (player.PlayerType == PlayerType.Local ? -1 : 1), 0, 0);
        });

        // Show randomized connecting messages
        if (this.GameState == GameState.Connecting) {
            if (this.connectingMessageStartTime <= Time.time) {
                this.connectingMessageStartTime = this.connectingMessageDelay + Time.time;
                this.uiManager.SetInstruction(this.connectingMesages[UnityEngine.Random.Range(0, this.connectingMesages.Length)]);
            }
        }

        // Check if in game and the turn timer has run out
        if (this.GameState == GameState.Playing && Time.time > this.TurnStarted + this.maxTurnTime) {
            if (!this.turnSkipped) {
                this.turnSkipped = true;
                this.NextTurn();
                this.uiManager.SetInstruction("Turn skipped, timer ran out", "");
            } else {
                this.SetPostGame(PlayerType.None, WinReason.Time);
            }
        }
    }

    public override void OnConnectedToMaster() {
        if (this.GameState == GameState.Connecting) {
            PhotonNetwork.JoinRandomRoom();
        } else if (this.GameState == GameState.PostGame) {
            PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 1, IsOpen = false });
            string loser = this.winner == PlayerType.Local ? "your opponent" : "you";
            string result = this.winner == PlayerType.Local ? "WON" : "LOST";

            string reason = this.winReason switch {
                WinReason.Disconnect => "disconnected",
                WinReason.Health => "ran out of health",
                WinReason.Time => "ran out of time",
                _ => "won"
            };

            this.uiManager.SetInstruction($"You {result} because {loser} {reason}", () => this.SetGameState(GameState.Menu));
        } else {
            PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = 1, IsOpen = false });
            this.SetGameState(GameState.Menu);
        }
    }

    public override void OnConnected() => Debug.Log($"Connected to {PhotonNetwork.Server} ({PhotonNetwork.CloudRegion.ToUpper()})");

    public override void OnPlayerEnteredRoom(Player remotePlayer) => Debug.Log($"Remote player joined the room");

    public override void OnPlayerLeftRoom(Player remotePlayer) {
        if (this.GameState == GameState.Playing) this.SetPostGame(PlayerType.Local, WinReason.Disconnect);

        Debug.Log($"Remote player left the room");
    }

    public override void OnLeftRoom() {
        this.Players.Clear();
        this.turnCount = -1;
        this.turnSkipped = false;
        if (this.GameState == GameState.Playing) this.SetPostGame(PlayerType.Remote, WinReason.Disconnect);
    }

    public override void OnJoinedRoom() {
        PlayerController player = PhotonNetwork.Instantiate(this.playerPrefab.name, this.playerPrefab.transform.position, this.playerPrefab.transform.rotation).GetComponent<PlayerController>();
        player.gameObject.AddComponent<InputController>().Binding = this.localPlayerBinding;

        Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message) {
        Debug.Log($"Could not join a random room ({message}), creating a new room");
        PhotonNetwork.CreateRoom(null, new Photon.Realtime.RoomOptions { MaxPlayers = (byte)this.MAX_PLAYERS, IsOpen = true });
    }

    public bool AddPlayer(PlayerController player) {
        bool valid = this.Players.Count < this.MAX_PLAYERS;
        if (valid) {
            this.Players.Add(player);
            this.Players.Sort((a, b) => a.photonView.ViewID - b.photonView.ViewID);
            this.PlayerInstantiated?.Invoke(player);
            player.WordSubmitted += this.InputSubmitted;

            if (this.Players.Count == MAX_PLAYERS) {
                this.SetGameState(GameState.Playing);
                this.NextTurn();
            }

            Debug.Log($"Player {player.photonView.ViewID} joined the game");
        }
        return valid;
    }

    private void InputSubmitted(PlayerController player, string input, WordRelation relation) {
        if (this.GameState == GameState.Menu) {
            if (this.menuState == MenuState.Tutorial) {
                if (input.ToLower() == "spellbound") {
                    this.uiManager.SetInstruction(() => this.SetMenuState(MenuState.Name),
                        "You just cast your first spell!",
                        "That's all there is to it\nYou now know how to play Spellbound",
                        "One last thing, you have to pick a name before you start playing\nYou can change it at any time",
                        "Have fun casting spells!"
                    );
                    PlayerPrefs.SetInt("CompletedTutorial", 1);
                }
            } else {
                switch (input.ToLower()) {
                    case "start":
                        if (PhotonNetwork.LocalPlayer.NickName != string.Empty) {
                            this.SetGameState(GameState.Connecting);
                            PhotonNetwork.LeaveRoom();
                        } else {
                            this.uiManager.SetInstruction("Please pick a name first", () => this.SetMenuState(MenuState.Menu));
                        }
                        break;
                    case "name":
                        this.SetMenuState(MenuState.Name);
                        break;
                    case "tutorial":
                        this.SetMenuState(MenuState.Tutorial);
                        break;
                    case "sprite":
                        this.SetMenuState(MenuState.Sprite);
                        break;
                    default:
                        if (this.menuState == MenuState.Name) {
                            PhotonNetwork.LocalPlayer.NickName = input;
                            this.LocalPlayer.NameUpdated?.Invoke(this.LocalPlayer, input);
                            PlayerPrefs.SetString("Nickname", input);
                            this.uiManager.SetInstruction($"Your name is now {input.ToUpper()}", () => this.SetMenuState(MenuState.Menu));

                            Debug.Log($"Player {player.photonView.ViewID} changed their name to {input}");
                        } else if (this.menuState == MenuState.Sprite) {
                            int maxSprites = this.LocalPlayer.Sprites.Length;
                            if (Char.TryParse(input, out char spriteChar) && spriteChar >= 'A' && spriteChar <= 'A' + maxSprites) {
                                int index = spriteChar - 'A';
                                this.LocalPlayer.SpriteIndex = index;
                                PlayerPrefs.SetInt("SpriteIndex", index);

                                this.uiManager.SetInstruction($"Your sprite is now {input}", () => this.SetMenuState(MenuState.Menu));
                                this.uiManager.ToggleCharacterPreview(false);

                                Debug.Log($"Player {player.photonView.ViewID} changed their sprite to sprite {index}");
                            } else {
                                this.uiManager.SetInstruction($"Invalid sprite {input}", () => this.SetMenuState(MenuState.Sprite));
                                break;
                            }
                        } else {
                            this.uiManager.SetInstruction($"Invalid command {input}", () => this.SetMenuState(MenuState.Menu));
                        }
                        break;
                }
            }
        } else if (this.GameState == GameState.Playing) {
            if (input.ToLower() == "exit" && player.PlayerType == PlayerType.Local) {
                this.SetPostGame(PlayerType.Remote, WinReason.Disconnect);
            } else {
                this.NextTurn();
            }
        } else if (this.GameState == GameState.Connecting) {
            if (input.ToLower() == "cancel") {
                this.SetGameState(GameState.Menu);
                PhotonNetwork.LeaveRoom();
            }
        }
    }

    public void NextTurn() {
        if (this.GameState != GameState.Playing) return;
        this.turnCount++;
        this.TurnStarted = Time.time;
        PlayerController lastPlayer = this.turnCount > 0 ? this.Players[(this.turnCount - 1) % this.MAX_PLAYERS] : this.Players[1];
        PlayerController currentPlayer = this.turnCount > 0 ? this.Players[this.turnCount % this.MAX_PLAYERS] : this.Players[0];
        lastPlayer.EndTurn();
        currentPlayer.StartTurn();

        this.TurnIncremented?.Invoke(lastPlayer, currentPlayer, this.turnCount);

        Debug.Log($"End of turn {this.turnCount - 1}, starting turn {this.turnCount} (Player {currentPlayer.photonView.ViewID})");
    }

    private void SetGameState(GameState gameState) {
        this.GameState = gameState;
        this.GameStateChanged?.Invoke(gameState);

        if (gameState == GameState.Menu) this.SetMenuState(MenuState.Menu);

        Debug.Log($"Game State changed to {gameState}");
    }

    private void SetMenuState(MenuState menuState) {
        this.menuState = menuState;
        switch (menuState) {
            case MenuState.Menu:
                this.uiManager.SetInstruction("Type START to start the game\nType NAME to change your name\nType SPRITE to change your sprite\nType TUTORIAL to play the tutorial");
                break;
            case MenuState.Name:
                this.uiManager.SetInstruction("Please enter your name");
                break;
            case MenuState.Sprite:
                this.uiManager.SetInstruction($"Please choose your desired sprite");
                this.uiManager.ToggleCharacterPreview(true);
                break;
            case MenuState.Tutorial:
                this.uiManager.SetInstruction(
                    "Welcome to Spellbound!",
                    "In this game players take turns spelling words to cast spells on each other",
                    "The longer the word, and less common the letters, the more damage your spell will inflict",
                    "Words with relations to other words have special effects",
                    "Synonyms heal you\nAntonyms deflect incoming spells\nRelated words amplify your spells",
                    "You can type EXIT during a game to return to the menu",
                    "Type SPELLBOUND to get started"
                );
                break;
        }

        Debug.Log($"Menu State changed to {menuState}");
    }

    /// <summary>
    /// Sets the game state to post game, sets the winner and win reason, and leaves the room
    /// </summary>
    /// <param name="winner">Whether the local or remote player won</param>
    /// <param name="winReason">The reason why that player won</param>
    public void SetPostGame(PlayerType winner, WinReason winReason) {
        this.winner = winner;
        this.winReason = winReason;
        this.SetGameState(GameState.PostGame);
        PhotonNetwork.LeaveRoom();
    }
}
