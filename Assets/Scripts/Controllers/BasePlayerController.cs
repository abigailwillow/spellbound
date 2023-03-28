using UnityEngine;
using Photon.Pun;

public class BasePlayerController : MonoBehaviourPun {
    [SerializeField] protected int maxHealth = 100;
    public int health { get; protected set; }
    protected string currentInput;
    public PlayerType playerType { get; protected set; }

    protected virtual void Awake() {
        this.health = this.maxHealth;
        this.photonView.ViewID = PhotonNetwork.IsMasterClient ^ this.playerType == PlayerType.Local ? 2 : 1;
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

    /// <summary>
    /// Submit the current input and clear it
    /// </summary>
    public virtual void Submit(string input) {
        Debug.Log($"SUBMIT -> ({input})");
        // GameManager.instance.remotePlayer.TakeDamage(input.Length);
    }

    public virtual void Die() => Debug.Log($"{playerType.ToString().ToUpper()} PLAYER DIED");
}
