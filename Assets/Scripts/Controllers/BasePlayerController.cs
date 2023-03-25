using UnityEngine;

public class BasePlayerController : MonoBehaviour {
    [SerializeField] protected int maxHealth = 100;
    public int health { get; protected set; }
    protected string currentInput;
    public PlayerType playerType { get; protected set; }

    protected virtual void Awake() {
        this.health = this.maxHealth;
    }

    public void TakeDamage(int damage) {
        this.health -= damage;
        if (this.health <= 0) {
            this.health = 0;
            this.Die();
        }
        UserInterfaceManager.instance.UpdatePlayerHealth(this.health, this.maxHealth, PlayerType.Remote);
        Debug.Log($"{playerType.ToString().ToUpper()} PLAYER TOOK {damage} DAMAGE ({health}/{maxHealth})");
    }

    public virtual void Die() => Debug.Log($"{playerType.ToString().ToUpper()} PLAYER DIED");
}
