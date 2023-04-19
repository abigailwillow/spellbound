using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun {
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => this.maxHealth;
    public int Health { get; private set; }
    [SerializeField] private PlayerType playerType;
    public PlayerType PlayerType => this.playerType;
    public string InputText { get; private set; }
    public List<string> SubmittedStrings { get; private set; } = new List<string>();
    private PlayerController Opponent => this.PlayerType == PlayerType.Local ? GameManager.RemotePlayer : GameManager.LocalPlayer;
    private GameManager GameManager => GameManager.Instance;

    # region Events
    /// <summary>
    /// Called when the player's input text is updated
    /// </summary>
    public Action<PlayerController, string> InputTextUpdated;
    /// <summary>
    /// Called when the player's health is updated
    /// </summary>
    public Action<PlayerController, int> HealthUpdated;
    /// <summary>
    /// Called when the player submits a word
    /// </summary>
    public Action<PlayerController, string> InputSubmitted;
    # endregion

    private void Awake() {
        this.Health = this.MaxHealth;
        this.photonView.ViewID = PhotonNetwork.IsMasterClient ^ this.PlayerType == PlayerType.Local ? 2 : 1;
    }
    
    public void TextInput(string character) => this.photonView.RPC(nameof(RPCTextInput), RpcTarget.All, character);

    [PunRPC] public void RPCTextInput(string character) {
        this.InputText += character;
        this.InputTextUpdated?.Invoke(this, this.InputText);
        Debug.Log($"[{this.PlayerType}] TextInput -> {character} ({this.InputText})");
    }

    public void Backspace() => this.photonView.RPC(nameof(RPCBackspace), RpcTarget.All);

    [PunRPC] public void RPCBackspace() {
        if (this.InputText?.Length > 0) {
            this.InputText = this.InputText.Remove(this.InputText.Length - 1);
        }
        this.InputTextUpdated?.Invoke(this, this.InputText);

        Debug.Log($"[{this.PlayerType}] Backspace -> {this.InputText}");
    }

    public void Submit() => this.Submit(this.InputText);

    public void Submit(string input) => this.photonView.RPC(nameof(RPCSubmit), RpcTarget.All, input);

    [PunRPC] public void RPCSubmit(string input) {
        if (string.IsNullOrEmpty(input)) return;
        if (!this.GameManager.WordList.Contains(input)) { Debug.Log($"Not a valid word ({input})"); return; }
        
        this.SubmittedStrings.Add(input);
        this.InputSubmitted?.Invoke(this, input);

        this.InputText = string.Empty;
        this.InputTextUpdated?.Invoke(this, this.InputText);

        this.Opponent.TakeDamage(input.Length);

        Debug.Log($"[{this.PlayerType}] Submit -> {input}");
    }

    public void TakeDamage(int damage) {
        this.Health = Mathf.Max(0, this.Health - damage);
        if (this.Health <= 0) {
            this.Die();
        }
        this.HealthUpdated?.Invoke(this, this.Health);

        Debug.Log($"[{this.PlayerType}] Damage -> {damage} ({this.Health}/{this.MaxHealth})");
    }

    private void Die() => Debug.Log($"[{this.PlayerType}] Died");
}
