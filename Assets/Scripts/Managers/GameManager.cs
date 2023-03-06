using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    private PlayerController player;
    public PlayerController Player { get => player; set => player = player == null ? value : player; }
    private OpponentController opponent;
    public OpponentController Opponent { get => opponent; set => opponent = opponent == null ? value : opponent; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            Instance = this;
        }
    }
}
