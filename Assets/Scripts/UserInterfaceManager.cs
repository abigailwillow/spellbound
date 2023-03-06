using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserInterfaceManager : MonoBehaviour {
    public static UserInterfaceManager Instance { get; private set; }
    public Image PlayerHealthBar;
    public TextMeshProUGUI PlayerHealthText;
    public Image OpponentHealthBar;
    public TextMeshProUGUI OpponentHealthText;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            Instance = this;
        }
    }
}
