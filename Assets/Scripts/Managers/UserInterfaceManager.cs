using UnityEngine;
using UnityEngine.UIElements;

public class UserInterfaceManager : MonoBehaviour {
    public static UserInterfaceManager Instance { get; private set; }
    private VisualElement playerHealthBar;
    private VisualElement playerHealthFill;
    private Label playerHealthText;
    private Label playerInputText;
    private VisualElement opponentHealthBar;
    private VisualElement opponentHealthFill;
    private Label opponentHealthText;

    private void Awake() {
        #region Singleton
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            Instance = this;
        }
        # endregion

        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        playerHealthBar = root.Query<VisualElement>("PlayerHealthBar");
        playerHealthFill = playerHealthBar.Query<VisualElement>("PlayerHealthFill");
        playerHealthText = playerHealthFill.Query<Label>("PlayerHealthText");
        playerInputText = root.Query<Label>("PlayerInputText");
    }

    public void UpdatePlayerHealth(int health, int maxHealth) {
        playerHealthFill.style.width = new StyleLength(Length.Percent((float) health / maxHealth * 100));
        playerHealthText.text = $"{health}/{maxHealth}";
    }
    
    public void UpdateOpponentHealth(int health, int maxHealth) {
        opponentHealthFill.style.width = new StyleLength(Length.Percent((float) health / maxHealth * 100));
        opponentHealthText.text = $"{health}/{maxHealth}";
    }

    public void UpdatePlayerInput(string text) => playerInputText.text = text;
}
