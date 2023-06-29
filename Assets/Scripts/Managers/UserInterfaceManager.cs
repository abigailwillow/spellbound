using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceManager : MonoBehaviour {
    /// <summary>A list of both players' UI elements</summary>
    private List<PlayerElements> playerElementsList = new List<PlayerElements>(2);
    private VisualElement instructionPanel;
    private Label instructionLabel;
    private VisualElement[] spritePanels = new VisualElement[2];
    private Label timerLabel;
    private GameManager gameManager => GameManager.Instance;
    /// <summary>The delay between each character of the instruction being displayed</summary>
    [SerializeField, Range(0f, 1f)] private float instructionDelay = 0.05f;
    /// <summary>The delay between the instruction being set and the first character being displayed</summary>
    [SerializeField, Range(0f, 1f)] private float instructionStartDelay = 0.25f;
    /// <summary>The amount of time to wait after the instruction has finished displaying before the next instruction is displayed</summary>
    [SerializeField, Range(0f, 10f)] private float instructionTimeout = 2.5f;
    private List<string> instructions = new List<string>();
    private float? instructionStart = null;
    private Action instructionCallback = null;

    private void Awake() {
        // Get all of the UI elements
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        this.playerElementsList.Add(new PlayerElements(root.Query<VisualElement>("LocalPlayerPanel"), PlayerType.Local));
        this.playerElementsList.Add(new PlayerElements(root.Query<VisualElement>("RemotePlayerPanel"), PlayerType.Remote));

        this.instructionPanel = root.Query<VisualElement>("InstructionPanel");
        this.instructionLabel = this.instructionPanel.Query<Label>("InstructionLabel");

        this.spritePanels[0] = instructionPanel.Query<VisualElement>("SpritesF");
        this.spritePanels[1] = instructionPanel.Query<VisualElement>("SpritesM");

        this.timerLabel = root.Query<Label>("TimerLabel");

        // Hide player panels by default, they are re-enabled when the players are instantiated
        this.SetPlayerPanelActive(false, PlayerType.Local);
        this.SetPlayerPanelActive(false, PlayerType.Remote);

        // Get the application version and add it to the version label
        Label version = root.Query<Label>("VersionLabel");
        version.text += Application.isEditor ? "in Development" : $"{Application.version}";
    }

    private void Start() {
        this.gameManager.PlayerInstantiated += player => {
            this.SetPlayerPanelActive(true, player.PlayerType);
            this.UpdatePlayerHealth(player, player.Health);
            this.UpdatePlayerInput(player, player.InputText);
            this.WordSubmitted(player, new Submission(String.Empty));

            player.HealthUpdated += UpdatePlayerHealth;
            player.InputTextUpdated += UpdatePlayerInput;
            player.Submitted += WordSubmitted;
            player.NameUpdated += UpdatePlayerName;
            this.UpdatePlayerName(player, player.photonView.Owner.NickName);

            if (player.PlayerType == PlayerType.Local && this.gameManager.GameState == GameState.Menu) {
                player.ToggleInput(true);
            }
        };

        this.gameManager.PlayerDestroyed += playerType => {
            this.SetPlayerPanelActive(false, playerType);
        };

        this.gameManager.GameStateChanged += GameStateChanged;
        this.gameManager.TurnIncremented += TurnIncremented;
    }

    private void Update() {
        // If there are instructions to display and the start time is in the past
        if (this.instructions.Count > 0 && Time.time > this.instructionStart) {
            float? timeSinceStart = Time.time - instructionStart;
            int position = (int)(timeSinceStart / instructionDelay);
            float duration = this.instructionDelay * this.instructions[0].Length;

            // If the current instruction is finished
            if (position > this.instructions[0].Length) {
                // If the time has exceeded the duration of the instruction plus the timeout
                if (timeSinceStart > duration + this.instructionTimeout) {
                    this.instructionStart = this.instructions.Count > 1 ? Time.time + this.instructionStartDelay : null;
                    this.instructions.RemoveAt(0);
                    // If a callback exists and there are no more instructions
                    if (this.instructionCallback != null && this.instructions.Count < 1) {
                        this.instructionCallback?.Invoke();
                    }
                }
            } else {
                this.instructionLabel.text = this.instructions[0].Substring(0, position);
            }
        }

        this.timerLabel.text = $"{(this.gameManager.TurnStarted - Time.time) + 30:0.0}";
    }

    /// <summary>Toggle a specific player panel on or off</summary>    
    /// <param name="enabled">Whether to enable or disable the panel</param>
    /// <param name="playerType">Which player's panel to enable or disable</param>
    public void SetPlayerPanelActive(bool enabled, PlayerType playerType) {
        bool remotePlayerPresent = enabled && playerType == PlayerType.Remote;
        PlayerElements playerElements = this.GetPlayerElements(playerType);
        playerElements.Panel.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        this.instructionPanel.style.width = remotePlayerPresent ? Length.Percent(100) : Length.Percent(50);
        if (remotePlayerPresent) this.SetInstruction("");
    }

    /// <summary>Set multiple instruction texts to display, the callback will be called afterwards</summary>
    public void SetInstruction(Action callback, params string[] strings) {
        this.instructionLabel.text = string.Empty;
        this.instructionCallback = callback;
        this.instructions = strings.ToList();
        this.instructionStart = Time.time + this.instructionStartDelay;
    }

    /// <summary>Set an instruction text to display, the callback will be called afterwards</summary>
    public void SetInstruction(string text, Action callback = null) => this.SetInstruction(callback, text);

    /// <summary>Set an instruction text to display</summary>
    public void SetInstruction(params string[] strings) => this.SetInstruction(null, strings);

    /// <summary>Called when the player's health is updated</summary>
    private void UpdatePlayerHealth(PlayerController player, int health) {
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);
        playerElements.HealthFill.style.width = new StyleLength(Length.Percent((float)health / player.MaxHealth * 100));
        playerElements.HealthText.text = $"{health}/{player.MaxHealth}";
    }

    /// <summary>Called when the player's input text is updated</summary>
    private void UpdatePlayerInput(PlayerController player, string text) {
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);
        playerElements.InputText.text = text;
    }

    /// <summary>Called when a word is submitted</summary>
    private void WordSubmitted(PlayerController player, Submission submission) {
        // Clear history and input text, then update history with the last 5 submissions
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);
        playerElements.InputText.text = string.Empty;
        playerElements.InputHistory.text = string.Empty;
        Enumerable.Reverse(player.Submissions).Take(5).ToList().ForEach(submission => playerElements.InputHistory.text += $"{submission.Input}\n");

        // Display information about the damage and relation of the submission
        string damageText = submission.Relation == WordRelation.None ? 
            $"{submission.Damage} " :
            $"{submission.Relation.ToString().ToUpper()}! {submission.Damage} ";
        damageText += submission.Healing ? "HEALED" : "DAMAGE";
        damageText += submission.Relation != WordRelation.None ? $" (×2)" : string.Empty;
        this.SetInstruction(damageText, "");
    }

    /// <summary>Called when the player's name is updated</summary>
    private void UpdatePlayerName(PlayerController player, string name) => this.GetPlayerElements(player.PlayerType).Username.text = name;

    /// <summary>Get the player elements for the given player type</summary>
    private PlayerElements GetPlayerElements(PlayerType playerType) => this.playerElementsList.Find(playerElements => playerElements.PlayerType == playerType);

    /// <summary>Called when the game state is updated, sets the healthbar and timer to be visible or not</summary>
    private void GameStateChanged(GameState gameState) {
        bool isPlaying = gameState == GameState.Playing;
        this.GetPlayerElements(PlayerType.Local).HealthBar.visible = isPlaying;
        this.timerLabel.visible = isPlaying;
    }

    /// <summary>Called when the turn is incremented, updates the username of the current player to show whose turn it is</summary>
    private void TurnIncremented(PlayerController previousPlayer, PlayerController currentPlayer, int turn) {
        this.GetPlayerElements(previousPlayer.PlayerType).Username.text = previousPlayer.photonView.Owner.NickName;
        this.GetPlayerElements(currentPlayer.PlayerType).Username.text = $">{currentPlayer.photonView.Owner.NickName}<";
    }

    /// <summary>Toggle the character preview on or off</summary>
    public void ToggleCharacterPreview(bool enabled) {
        this.spritePanels[0].style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        this.spritePanels[1].style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
    }

    /// <summary>A collection of all elements in a player's user interface</summary>
    private class PlayerElements {
        public PlayerElements(VisualElement playerPanel, PlayerType playerType) {
            this.PlayerType = playerType;
            this.Panel = playerPanel;
            this.Username = playerPanel.Query<Label>("Username");
            this.HealthBar = playerPanel.Query<VisualElement>("HealthBar");
            this.HealthFill = playerPanel.Query<VisualElement>("HealthFill");
            this.HealthText = playerPanel.Query<Label>("HealthText");
            this.InputText = playerPanel.Query<Label>("InputText");
            this.InputHistory = playerPanel.Query<Label>("InputHistory");
        }

        public PlayerType PlayerType;
        public VisualElement Panel;
        public Label Username;
        public VisualElement HealthBar;
        public VisualElement HealthFill;
        public Label HealthText;
        public Label InputText;
        public Label InputHistory;
    }
}
