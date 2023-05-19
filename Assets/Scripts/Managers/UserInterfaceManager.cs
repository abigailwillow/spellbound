using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceManager : MonoBehaviour {
    private List<PlayerElements> playerElementsList = new List<PlayerElements>(2);
    private VisualElement instructionPanel;
    private Label instructionLabel;
    private GameManager gameManager => GameManager.Instance;
    [SerializeField, Range(0f, 1f)] private float INSTRUCTION_ANIMATION_DELAY = 0.05f;
    private string instructionText = string.Empty;
    private float? instructionStartTime = null;

    private void Awake() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        this.playerElementsList.Add(new PlayerElements(root.Query<VisualElement>("LocalPlayerPanel"), PlayerType.Local));
        this.playerElementsList.Add(new PlayerElements(root.Query<VisualElement>("RemotePlayerPanel"), PlayerType.Remote));

        this.instructionPanel = root.Query<VisualElement>("InstructionPanel");
        this.instructionLabel = this.instructionPanel.Query<Label>("InstructionLabel");

        this.SetPlayerPanelActive(false, PlayerType.Local);
        this.SetPlayerPanelActive(false, PlayerType.Remote);

        Label version = root.Query<Label>("VersionLabel");
        version.text += Application.isEditor ? "in Development" : $"{Application.version}";
    }

    private void Start() {
        this.gameManager.PlayerInstantiated += player => {
            this.SetPlayerPanelActive(true, player.PlayerType);
            this.UpdatePlayerHealth(player, player.Health);
            this.UpdatePlayerInput(player, player.InputText);
            this.UpdatePlayerHistory(player, string.Empty);

            player.HealthUpdated += UpdatePlayerHealth;
            player.InputTextUpdated += UpdatePlayerInput;
            player.InputSubmitted += UpdatePlayerHistory;
            this.UpdatePlayerName(player, player.photonView.Owner.NickName);

            if (player.PlayerType == PlayerType.Local && this.gameManager.GameState == GameState.Menu) {
                player.ToggleInput(true);
            }
        };

        this.gameManager.PlayerDestroyed += playerType => {
            this.SetPlayerPanelActive(false, playerType);
        };

        this.gameManager.GameStateChanged += GameStateUpdated;
    }

    private void Update() {
        if (this.instructionStartTime != null && Time.time > instructionStartTime) {
            float? timeSinceStart = Time.time - instructionStartTime;
            int position = (int)(timeSinceStart / INSTRUCTION_ANIMATION_DELAY);
            this.instructionLabel.text = this.instructionText.Substring(0, position);
            if (position >= this.instructionText.Length) {
                this.instructionStartTime = null;
            }
        }
    }

    /// <summary>
    /// Toggle a specific player panel on or off
    /// </summary>
    /// <param name="enabled">Whether to enable or disable the panel</param>
    /// <param name="playerType">Which player's panel to enable or disable</param>
    public void SetPlayerPanelActive(bool enabled, PlayerType playerType) {
        PlayerElements playerElements = this.GetPlayerElements(playerType);
        playerElements.Panel.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
        this.instructionPanel.style.display = (enabled && playerType == PlayerType.Remote) ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void SetInstructionText(string text) {
        this.instructionText = text;
        this.instructionStartTime = Time.time;
    }


    private void UpdatePlayerHealth(PlayerController player, int health) {
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);
        playerElements.HealthFill.style.width = new StyleLength(Length.Percent((float)health / player.MaxHealth * 100));
        playerElements.HealthText.text = $"{health}/{player.MaxHealth}";
    }

    private void UpdatePlayerInput(PlayerController player, string text) {
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);

        playerElements.InputText.text = text;
    }

    private void UpdatePlayerHistory(PlayerController player, string word) {
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);
        playerElements.InputHistory.text = string.Empty;
        Enumerable.Reverse(player.SubmittedStrings).Take(5).ToList().ForEach(word => playerElements.InputHistory.text += $"{word}\n");
    }

    private void UpdatePlayerName(PlayerController player, string name) {
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);
        playerElements.Username.text = name;
    }

    private PlayerElements GetPlayerElements(PlayerType playerType) => this.playerElementsList.Find(playerElements => playerElements.PlayerType == playerType);

    private void GameStateUpdated(GameState gameState) {
        switch (gameState) {
            case GameState.Menu:
                this.SetInstructionText("Pick a name to start");
                break;
            case GameState.Connecting:
                this.SetInstructionText("Connecting...");
                break;
            case GameState.Playing:
                this.SetInstructionText("");
                break;
            case GameState.PostGame:
                this.SetInstructionText("Game Over");
                break;
        }
    }

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
