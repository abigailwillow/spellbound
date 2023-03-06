using UnityEngine;

public class OpponentController : MonoBehaviour {
    public int MaxHealth = 100;
    public int Health { get; private set; }

    private void Awake() {
        GameManager.Instance.Opponent = this;
        this.Health = this.MaxHealth;
    }

    public void TakeDamage(int damage) {
        Debug.Log($"OPPONENT TOOK {damage} DAMAGE ({Health}/{MaxHealth})");
        UserInterfaceManager.Instance.OpponentHealthBar.fillAmount = (float) this.Health / this.MaxHealth;
        UserInterfaceManager.Instance.OpponentHealthText.text = $"{this.Health}/{this.MaxHealth}";
        this.Health -= damage;
        if (this.Health <= 0) {
            this.Die();
        }
    }

    public void Die() => Debug.Log("OPPONENT DIED");
}
