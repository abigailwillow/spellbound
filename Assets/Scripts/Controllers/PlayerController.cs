using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPun {
    [SerializeField] private int maxHealth = 100;
    /// <summary>
    /// The player's maximum health
    /// </summary>
    public int MaxHealth => this.maxHealth;
    [SerializeField] private Sprite[] sprites;
    public Sprite[] Sprites => this.sprites;
    /// <summary>
    /// The player's current health
    /// </summary>
    public int Health { get; private set; }
    public PlayerType PlayerType => this.photonView.IsMine ? PlayerType.Local : PlayerType.Remote;
    public string InputText { get; private set; } = string.Empty;
    /// <summary>
    /// List of all submitted strings in the current session
    /// </summary>
    public List<string> SubmittedStrings { get; private set; } = new List<string>();
    /// <summary>
    /// The last submitted string in the current session, if none then empty string
    /// </summary>
    public string LastSubmittedString => this.SubmittedStrings.Count > 0 ? this.SubmittedStrings[this.SubmittedStrings.Count - 1] : string.Empty;
    public PlayerController Opponent => this.PlayerType == PlayerType.Local ? gameManager.RemotePlayer : gameManager.LocalPlayer;
    private GameManager gameManager => GameManager.Instance;
    private WordDataList wordList => this.gameManager.WordList;
    private InputController input;
    private string exit = "EXIT";
    private SpriteRenderer spriteRenderer;
    private int spriteIndex;
    /// <summary>
    /// The index of the sprite to use, automatically sets and syncs the sprite
    /// </summary>
    /// <value></value>
    public int SpriteIndex {
        get => spriteIndex;
        set {
            if (spriteIndex == value) return;
            this.spriteIndex = value;
            this.GetComponent<SpriteRenderer>().sprite = this.sprites[value];
            this.SyncSpriteIndex();
        }
    }

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
    public Action<PlayerController, string, WordRelation> WordSubmitted;
    public Action<PlayerController, string> NameUpdated;
    # endregion

    private void Awake() {
        this.Health = this.MaxHealth;

        string playerPrefsNickname = PlayerPrefs.GetString("Nickname", "");
        string photonNickname = PhotonNetwork.NickName;
        PhotonNetwork.NickName = string.IsNullOrEmpty(photonNickname) ? playerPrefsNickname : photonNickname;
    }

    private void Start() {
        if (PlayerType == PlayerType.Local) {
            this.SpriteIndex = PlayerPrefs.GetInt("SpriteIndex", 0);
        }

        this.TryGetComponent<InputController>(out input);
        this.gameManager.AddPlayer(this);
    }

    /// <summary>
    /// Synchronizes the player sprite's index with the remote player, ensuring it will get received
    /// </summary>
    public void SyncSpriteIndex() => this.photonView.RPC(nameof(SyncSprite), RpcTarget.OthersBuffered, this.SpriteIndex);

    [PunRPC] private void SyncSprite(int spriteIndex) => this.SpriteIndex = spriteIndex;

    /// <summary>
    /// Adds a character to the player's input text, if the input is disabled it will only accept exit characters
    /// </summary>
    /// <param name="character">The character to append</param>
    public void TextInput(string character) {
        if (!this.input.enabled && character != this.exit.ElementAtOrDefault(this.InputText.Length).ToString()) return;
        this.photonView.RPC(nameof(RPCTextInput), RpcTarget.All, character);
    }

    [PunRPC] public void RPCTextInput(string character) {
        this.InputText += character;
        this.InputTextUpdated?.Invoke(this, this.InputText);

        Debug.Log($"[{this.PlayerType}] TextInput -> {character} ({this.InputText})");
    }

    /// <summary>
    /// Removes the last character from the player's input text, if there is a character to remove
    /// </summary>
    public void Backspace() => this.photonView.RPC(nameof(RPCBackspace), RpcTarget.All);

    [PunRPC] public void RPCBackspace() {
        if (this.InputText?.Length > 0) {
            this.InputText = this.InputText.Remove(this.InputText.Length - 1);
        }
        this.InputTextUpdated?.Invoke(this, this.InputText);

        Debug.Log($"[{this.PlayerType}] Backspace -> {this.InputText}");
    }

    /// <summary>
    /// Submits the player's current input
    /// </summary>
    public void Submit() => this.Submit(this.InputText);

    /// <summary>
    /// Submits the given input
    /// </summary>
    /// <param name="input">The input to submit</param>
    public void Submit(string input) => this.photonView.RPC(nameof(RPCSubmit), RpcTarget.All, input);

    [PunRPC] public void RPCSubmit(string input) {
        if (string.IsNullOrEmpty(input)) return;
        if (this.gameManager.GameState == GameState.Playing) {
            if (!this.wordList.Contains(input) && input != this.exit) { Debug.Log($"[{this.PlayerType}] Not a valid word ({input})"); return; }
            if (input == this.exit) { this.SubmitInput(input); return; }

            WordData word = this.wordList.Get(input);
            // WordRelation relation = this.GetWordRelation(input);

            this.SubmittedStrings.Add(input);
            // this.SubmitInput(input, relation);

            // int damage = this.CalculateDamage(input, relation);
            // if (relation == WordRelation.Synonym) {
            //     this.Heal(damage);
            // } else {
            //     this.Opponent.TakeDamage(damage);
            // }

            Debug.Log($"[{this.PlayerType}] Submit -> {input} (Synonyms: {string.Join(", ", word.Synonyms)} - Antonyms: {string.Join(", ", word.Antonyms)} - Related: {string.Join(", ", word.RelatedWords)})");
        } else {
            this.SubmitInput(input);
        }
    }

    private void SubmitInput(string input, WordRelation relation = WordRelation.None) {
        this.InputText = string.Empty;
        this.InputTextUpdated?.Invoke(this, this.InputText);
        this.WordSubmitted?.Invoke(this, input, relation);
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
        this.gameManager.SetPostGame(this.Opponent.PlayerType, WinReason.Health);

        Debug.Log($"[{this.PlayerType}] Died");
    }

    private void OnDestroy() => this.gameManager.PlayerDestroyed?.Invoke(this.PlayerType);

    public void StartTurn() {
        if (this.input) this.input.enabled = true;
        Debug.Log($"[{this.PlayerType}] Turn Started");
    }

    public void EndTurn() {
        if (this.input) this.input.enabled = false;
        this.InputText = string.Empty;
        this.InputTextUpdated?.Invoke(this, this.InputText);
        Debug.Log($"[{this.PlayerType}] Turn Ended");
    }

    public void ToggleInput(bool enabled) => this.input.enabled = enabled;
}
