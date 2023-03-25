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

        localPlayerElements = new PlayerElements(
            root.Query<VisualElement>("PlayerHealthBar"),
            root.Query<VisualElement>("PlayerHealthFill"),
            root.Query<Label>("PlayerHealthText"),
            root.Query<Label>("PlayerInputText")
        );

        Debug.Log(localPlayerElements);

        // remotePlayerElements = new PlayerInterfaceElements(
        //     root.Query<VisualElement>("OpponentHealthBar"),
        //     root.Query<VisualElement>("OpponentHealthFill"),
        //     root.Query<Label>("OpponentHealthText"),
        //     root.Query<Label>("OpponentInputText")
        // );
    }

    public void UpdatePlayerHealth(int health, int maxHealth, PlayerType playerType) {
        PlayerElements playerElements = playerType == PlayerType.Local ? localPlayerElements : remotePlayerElements;
        playerElements.healthFill.style.width = new StyleLength(Length.Percent((float) health / maxHealth * 100));
        playerElements.healthText.text = $"{health}/{maxHealth}";
    }

    public void UpdatePlayerInput(string text) => localPlayerElements.inputText.text = text;

    private struct PlayerElements {
        public PlayerElements(VisualElement healthBar, VisualElement healthFill, Label healthText, Label inputText) {
            this.healthBar = healthBar;
            this.healthFill = healthFill;
            this.healthText = healthText;
            this.inputText = inputText;
        }

        public VisualElement healthBar;
        public VisualElement healthFill;
        public Label healthText;
        public Label inputText;
    }
}
