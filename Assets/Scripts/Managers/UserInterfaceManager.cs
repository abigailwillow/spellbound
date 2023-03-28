using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceManager : MonoBehaviour {
    public static UserInterfaceManager instance { get; private set; }
    private PlayerElements localPlayerElements;
    private PlayerElements remotePlayerElements;

    private void Awake() {
        #region Singleton
        if (instance != null && instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            instance = this;
        }
        # endregion

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        localPlayerElements = new PlayerElements(root.Query<VisualElement>("LocalPlayerPanel"));
        remotePlayerElements = new PlayerElements(root.Query<VisualElement>("RemotePlayerPanel"));
    }

    public void UpdatePlayerHealth(int health, int maxHealth, PlayerType playerType) {
        PlayerElements playerElements = playerType == PlayerType.Local ? localPlayerElements : remotePlayerElements;
        playerElements.healthFill.style.width = new StyleLength(Length.Percent((float) health / maxHealth * 100));
        playerElements.healthText.text = $"{health}/{maxHealth}";
    }

    public void UpdatePlayerInput(string text, PlayerType playerType) {
        PlayerElements playerElements = playerType == PlayerType.Local ? localPlayerElements : remotePlayerElements;
        playerElements.inputText.text = text;
    }

    /// <summary>
    /// Toggle a specific player panel on or off
    /// </summary>
    /// <param name="enabled">Whether to enable or disable the panel</param>
    /// <param name="playerType">Which player's panel to enable or disable</param>
    public void SetPlayerPanelActive(bool enabled, PlayerType playerType) {
        PlayerElements playerElements = playerType == PlayerType.Local ? localPlayerElements : remotePlayerElements;
        playerElements.panel.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private class PlayerElements {
        public PlayerElements(VisualElement playerPanel) {
            this.panel = playerPanel;
            this.healthBar = playerPanel.Query<VisualElement>("HealthBar");
            this.healthFill = playerPanel.Query<VisualElement>("HealthFill");
            this.healthText = playerPanel.Query<Label>("HealthText");
            this.inputText = playerPanel.Query<Label>("InputText");
        }

        public VisualElement panel;
        public VisualElement healthBar;
        public VisualElement healthFill;
        public Label healthText;
        public Label inputText;
    }
}
