using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun {
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => this.maxHealth;
    public int Health { get; private set; }
    public PlayerType PlayerType => this.photonView.IsMine ? PlayerType.Local : PlayerType.Remote;
    public string InputText { get; private set; }
    public List<string> SubmittedStrings { get; private set; } = new List<string>();
    public string LastSubmittedString => this.SubmittedStrings.Count > 0 ? this.SubmittedStrings[this.SubmittedStrings.Count - 1] : string.Empty;
    private PlayerController opponent => this.PlayerType == PlayerType.Local ? gameManager.RemotePlayer : gameManager.LocalPlayer;
    private GameManager gameManager => GameManager.Instance;
    private WordDataList wordList => this.gameManager.WordList;

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
        this.gameManager.AddPlayer(this);
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
        if (!this.wordList.Contains(input)) { Debug.Log($"[{this.PlayerType}] Not a valid word ({input})"); return; }

        // if (this.wordList.Get(this.opponent.LastSubmittedString).IsSynonym(input)) Debug.Log($"[{this.PlayerType}] SYNONYM -> {input}");
        // if (this.wordList.Get(this.opponent.LastSubmittedString).IsAntonym(input)) Debug.Log($"[{this.PlayerType}] ANTONYM -> {input}");
        // if (this.wordList.Get(this.opponent.LastSubmittedString).IsRelated(input)) Debug.Log($"[{this.PlayerType}] RELATED -> {input}");
        
        this.SubmittedStrings.Add(input);
        this.InputSubmitted?.Invoke(this, input);

        this.InputText = string.Empty;
        this.InputTextUpdated?.Invoke(this, this.InputText);

        this.opponent.TakeDamage(input.Length);

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

    private void OnDestroy() => this.gameManager.PlayerDestroyed?.Invoke(this.PlayerType);
}
