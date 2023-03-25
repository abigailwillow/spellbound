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

    public void UpdatePlayerInput(string text) => localPlayerElements.inputText.text = text;

    private class PlayerElements {
        public PlayerElements(VisualElement playerPanel) {
            this.healthBar = playerPanel.Query<VisualElement>("HealthBar");
            this.healthFill = playerPanel.Query<VisualElement>("HealthFill");
            this.healthText = playerPanel.Query<Label>("HealthText");
            this.inputText = playerPanel.Query<Label>("InputText");
        }

        public VisualElement healthBar;
        public VisualElement healthFill;
        public Label healthText;
        public Label inputText;
    }
}
