using UnityEngine;

public class RemotePlayerController : MonoBehaviour {
    [SerializeField] private int MaxHealth = 100;
    public int Health { get; private set; }

    private void Awake() {
        GameManager.Instance.Opponent = this;
        this.Health = this.MaxHealth;
    }

    public void TakeDamage(int damage) {
        this.Health -= damage;
        if (this.Health <= 0) {
            this.Die();
        }
        UserInterfaceManager.instance.UpdatePlayerHealth(this.Health, this.MaxHealth, PlayerType.Remote);
        Debug.Log($"OPPONENT TOOK {damage} DAMAGE ({Health}/{MaxHealth})");
    }

    public void Die() => Debug.Log("OPPONENT DIED");
}
