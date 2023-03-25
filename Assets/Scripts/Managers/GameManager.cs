using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    private LocalPlayerController player;
    public LocalPlayerController Player { get => player; set => player = player == null ? value : player; }
    private RemotePlayerController opponent;
    public RemotePlayerController Opponent { get => opponent; set => opponent = opponent == null ? value : opponent; }

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(this.gameObject);
            return;
        } else {
            Instance = this;
        }
    }
}
