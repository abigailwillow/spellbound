using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun {
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth => this.maxHealth;
    public int Health { get; private set; }
    public PlayerType PlayerType => this.photonView.IsMine ? PlayerType.Local : PlayerType.Remote;
    public string InputText { get; private set; } = string.Empty;
    public List<string> SubmittedStrings { get; private set; } = new List<string>();
    public string LastSubmittedString => this.SubmittedStrings.Count > 0 ? this.SubmittedStrings[this.SubmittedStrings.Count - 1] : string.Empty;
    private PlayerController opponent => this.PlayerType == PlayerType.Local ? gameManager.RemotePlayer : gameManager.LocalPlayer;
    private GameManager gameManager => GameManager.Instance;
    private WordDataList wordList => this.gameManager.WordList;
    private InputController input;
    private string exit = "EXIT";


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
    public Action<PlayerController, string> NameUpdated; 
    # endregion

    private void Awake() {
        this.Health = this.MaxHealth;

        string playerPrefsNickname = PlayerPrefs.GetString("Nickname", "");
        string photonNickname = PhotonNetwork.NickName;
        PhotonNetwork.NickName = string.IsNullOrEmpty(photonNickname) ? playerPrefsNickname : photonNickname;
    }

    private void Start() {
        this.TryGetComponent<InputController>(out input);
        this.gameManager.AddPlayer(this);
    }
    
    public void TextInput(string character) {
        if (!this.input.enabled && character != this.exit.ElementAtOrDefault(this.InputText.Length).ToString()) return;
        this.photonView.RPC(nameof(RPCTextInput), RpcTarget.All, character);
    }

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
        if (this.gameManager.GameState == GameState.Playing) {
            if (!this.wordList.Contains(input) && input != this.exit) { Debug.Log($"[{this.PlayerType}] Not a valid word ({input})"); return; }
            if (input == this.exit) { this.SubmitInput(input); return; }

            WordData word = this.wordList.Get(input);
            WordRelation relation = this.GetWordRelation(input);
            
            this.SubmittedStrings.Add(input);
            this.SubmitInput(string.Empty);

            int damage = this.CalculateDamage(input, relation);
            if (relation == WordRelation.Synonym) {
                this.Heal(damage);
            } else {
                this.opponent.TakeDamage(damage);
            }

            Debug.Log($"[{this.PlayerType}] Submit -> {input} (Synonyms: {string.Join(", ", word.Synonyms)} - Antonyms: {string.Join(", ", word.Antonyms)} - Related: {string.Join(", ", word.RelatedWords)})");
        } else {
            this.SubmitInput(input);
        }
    }

    private void SubmitInput(string input) {
        this.InputText = string.Empty;
        this.InputTextUpdated?.Invoke(this, this.InputText);
        this.InputSubmitted?.Invoke(this, input);
    }

    public void TakeDamage(int damage) {
        this.Health = Mathf.Max(0, this.Health - damage);
        if (this.Health <= 0) {
            this.Die();
        }
        this.HealthUpdated?.Invoke(this, this.Health);

        Debug.Log($"[{this.PlayerType}] Damaged -> {damage} ({this.Health}/{this.MaxHealth})");
    }

    public void Heal(int amount) {
        this.Health = Mathf.Min(this.MaxHealth, this.Health + amount);
        this.HealthUpdated?.Invoke(this, this.Health);

        Debug.Log($"[{this.PlayerType}] Healed -> {amount} ({this.Health}/{this.MaxHealth})");
    }

    private void Die() {
        this.gameManager.SetPostGame(this.opponent.PlayerType, WinReason.Health);

        Debug.Log($"[{this.PlayerType}] Died");
    }

    private void OnDestroy() => this.gameManager.PlayerDestroyed?.Invoke(this.PlayerType);

    public void StartTurn() {
        if (this.input) this.input.enabled = true;
        Debug.Log($"[{this.PlayerType}] Turn Started");
    }

    public void EndTurn() {
        if (this.input) this.input.enabled = false;
        Debug.Log($"[{this.PlayerType}] Turn Ended");
    }

    public void ToggleInput(bool enabled) => this.input.enabled = enabled;

    private WordRelation GetWordRelation(string word) {
        if (this.wordList.Contains(this.opponent.LastSubmittedString)) {
            WordData opponentWord = this.wordList.Get(this.opponent.LastSubmittedString);
            WordRelation relation = word switch {
                _ when opponentWord.IsSynonym(word) => WordRelation.Synonym,
                _ when opponentWord.IsAntonym(word) => WordRelation.Antonym,
                _ when opponentWord.IsRelated(word) => WordRelation.Related,
                _ => WordRelation.None
            };

            Debug.Log($"[{this.PlayerType}] {relation.ToString().ToUpper()} -> {word}");

            return relation;
        }
        return WordRelation.None;
    }

    private int CalculateDamage(string word, WordRelation relation) {
        int baseDamage = this.gameManager.CalculateDamage(word);
        int opponentDamage = this.gameManager.CalculateDamage(this.opponent.LastSubmittedString);
        int damage = relation switch {
            WordRelation.Synonym => baseDamage * 2,
            WordRelation.Antonym => opponentDamage * 2,
            WordRelation.Related => baseDamage * 2,
            _ => baseDamage
        };
        return damage;
    }
}
