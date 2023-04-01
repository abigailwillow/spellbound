using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceManager : MonoBehaviour {
    private PlayerElements LocalPlayerElements;
    private PlayerElements RemotePlayerElements;
    private GameManager GameManager => GameManager.Instance;

    private void Awake() {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        this.LocalPlayerElements = new PlayerElements(root.Query<VisualElement>("LocalPlayerPanel"));
        this.RemotePlayerElements = new PlayerElements(root.Query<VisualElement>("RemotePlayerPanel"));
        this.SetPlayerPanelActive(true, PlayerType.Local);
        this.SetPlayerPanelActive(false, PlayerType.Remote);
    }

    private void Start() {
        GameManager.OnPlayerInstantiated += player => {
            this.SetPlayerPanelActive(true, player.PlayerType);
            this.UpdatePlayerHealth(player, player.Health);
            this.UpdatePlayerInput(player, player.InputText);

            player.OnHealthUpdated += UpdatePlayerHealth;
            player.OnInputTextUpdated += UpdatePlayerInput;
        };

        GameManager.OnPlayerDestroyed += player => {
            this.SetPlayerPanelActive(false, player.PlayerType);
            player.OnHealthUpdated -= UpdatePlayerHealth;
            player.OnInputTextUpdated -= UpdatePlayerInput;
        };
    }

    private void UpdatePlayerHealth(PlayerController player, int health) {
        PlayerElements playerElements = player.PlayerType == PlayerType.Local ? LocalPlayerElements : RemotePlayerElements;
        playerElements.HealthFill.style.width = new StyleLength(Length.Percent((float) health / player.MaxHealth * 100));
        playerElements.HealthText.text = $"{health}/{player.MaxHealth}";
    }

    private void UpdatePlayerInput(PlayerController player, string text) {
        PlayerElements playerElements = player.PlayerType == PlayerType.Local ? LocalPlayerElements : RemotePlayerElements;
        playerElements.InputText.text = text;
    }

    /// <summary>
    /// Toggle a specific player panel on or off
    /// </summary>
    /// <param name="enabled">Whether to enable or disable the panel</param>
    /// <param name="playerType">Which player's panel to enable or disable</param>
    public void SetPlayerPanelActive(bool enabled, PlayerType playerType) {
        PlayerElements playerElements = playerType == PlayerType.Local ? LocalPlayerElements : RemotePlayerElements;
        playerElements.Panel.style.display = enabled ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private class PlayerElements {
        public PlayerElements(VisualElement playerPanel) {
            this.Panel = playerPanel;
            this.HealthBar = playerPanel.Query<VisualElement>("HealthBar");
            this.HealthFill = playerPanel.Query<VisualElement>("HealthFill");
            this.HealthText = playerPanel.Query<Label>("HealthText");
            this.InputText = playerPanel.Query<Label>("InputText");
        }

        public VisualElement Panel;
        public VisualElement HealthBar;
        public VisualElement HealthFill;
        public Label HealthText;
        public Label InputText;
    }
}
