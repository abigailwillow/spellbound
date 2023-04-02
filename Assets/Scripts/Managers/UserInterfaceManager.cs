using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceManager : MonoBehaviour {
    private List<PlayerElements> PlayerElementsList = new List<PlayerElements>(2);
    private GameManager GameManager => GameManager.Instance;

    private void Awake() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        this.PlayerElementsList.Add(new PlayerElements(root.Query<VisualElement>("LocalPlayerPanel"), PlayerType.Local));
        this.PlayerElementsList.Add(new PlayerElements(root.Query<VisualElement>("RemotePlayerPanel"), PlayerType.Remote));

        this.SetPlayerPanelActive(false, PlayerType.Local);
        this.SetPlayerPanelActive(false, PlayerType.Remote);

        Label version = root.Query<Label>("VersionLabel");
        version.text += $"{Application.version}";
    }

    private void Start() {
        GameManager.PlayerInstantiated += player => {
            this.SetPlayerPanelActive(true, player.PlayerType);
            this.UpdatePlayerHealth(player, player.Health);
            this.UpdatePlayerInput(player, player.InputText);
            this.UpdatePlayerHistory(player, string.Empty);

            player.HealthUpdated += UpdatePlayerHealth;
            player.InputTextUpdated += UpdatePlayerInput;
            player.InputSubmitted += UpdatePlayerHistory;

            this.GetPlayerElements(player.PlayerType).Username.text = $"Player {player.photonView.ViewID}";
        };

        GameManager.PlayerDestroyed += playerType => {
            this.SetPlayerPanelActive(false, playerType);
        };
    }

    /// <summary>
    /// Toggle a specific player panel on or off
    /// </summary>
    /// <param name="enabled">Whether to enable or disable the panel</param>
    /// <param name="playerType">Which player's panel to enable or disable</param>
    public void SetPlayerPanelActive(bool enabled, PlayerType playerType) {
        PlayerElements playerElements = this.GetPlayerElements(playerType);
        playerElements.Panel.style.visibility = enabled ? Visibility.Visible : Visibility.Hidden;
    }

    private void UpdatePlayerHealth(PlayerController player, int health) {
        PlayerElements playerElements = this.GetPlayerElements(player.PlayerType);
        playerElements.HealthFill.style.width = new StyleLength(Length.Percent((float) health / player.MaxHealth * 100));
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

    private PlayerElements GetPlayerElements(PlayerType playerType) => this.PlayerElementsList.Find(playerElements => playerElements.PlayerType == playerType);

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
