using UnityEngine;

public class OpponentController : MonoBehaviour {
    public float MaxHealth = 100f;
    public float Health { get; private set; }

    private void Start() {
        this.Health = this.MaxHealth;
    }

    public void TakeDamage(float damage) {
        this.Health -= damage;
        if (this.Health <= 0) {
            this.Die();
        }
    }

    public void Die() => Debug.Log("OPPONENT DIED");
}
