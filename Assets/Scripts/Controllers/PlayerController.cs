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
    public List<Submission> Submissions { get; private set; } = new List<Submission>();
    public PlayerController Opponent => this.PlayerType == PlayerType.Local ? gameManager.RemotePlayer : gameManager.LocalPlayer;
    private GameManager gameManager => GameManager.Instance;
    private InputController input;
    private int spriteIndex;
    private string exit = "EXIT";
    /// <summary>
    /// The index of the sprite to use, automatically sets and syncs the sprite
    /// </summary>
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
    public Action<PlayerController, Submission> Submitted;
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

    private void OnDestroy() => this.gameManager.PlayerDestroyed?.Invoke(this.PlayerType);

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
            Submission submission = new Submission(input);

            if (input == this.exit) { this.SubmitSubmission(submission); return; }
            if (!submission.Valid) { Debug.Log($"[{this.PlayerType}] Not a valid word ({input})"); return; }

            this.SubmitSubmission(submission);

            if (submission.Healing) {
                this.Heal(submission.Damage);
            } else {
                this.Opponent.Damage(submission.Damage);
            }

            Debug.Log(
                $"[{this.PlayerType}] Submit -> {input} (" +
                $"Synonyms: {string.Join(", ", submission.Word.Synonyms)} - " +
                $"Antonyms: {string.Join(", ", submission.Word.Antonyms)} - " +
                $"Related: {string.Join(", ", submission.Word.RelatedWords)})"
            );
        } else {
            this.Submit(input);
        }
    }

    /// <summary>
    /// Submits the given submission, calls the appropriate events and adds the submission to the list of submissions if applicable
    /// </summary>
    /// <param name="submission">The submission to submit</param>
    private void SubmitSubmission(Submission submission) {
        this.InputText = string.Empty;
        this.InputTextUpdated?.Invoke(this, submission.Input);
        this.Submitted?.Invoke(this, submission);
        if (this.gameManager.GameState == GameState.Playing) this.Submissions.Add(submission);
    }

    /// <summary>
    /// Damages the player by the given amount, if the player's health reaches zero they will die
    /// </summary>
    /// <param name="damage">The amount of damage to inflict</param>
    public void Damage(int damage) {
        this.Health = Mathf.Max(0, this.Health - damage);
        if (this.Health <= 0) {
            this.Die();
        }
        this.HealthUpdated?.Invoke(this, this.Health);

        Debug.Log($"[{this.PlayerType}] Damaged -> {damage} ({this.Health}/{this.MaxHealth})");
    }

    /// <summary>
    /// Heals the player by the given amount, if the player's health exceeds their maximum health it will be set to their maximum health
    /// </summary>
    /// <param name="amount">The amount to heal by</param>
    public void Heal(int amount) {
        this.Health = Mathf.Min(this.MaxHealth, this.Health + amount);
        this.HealthUpdated?.Invoke(this, this.Health);

        Debug.Log($"[{this.PlayerType}] Healed -> {amount} ({this.Health}/{this.MaxHealth})");
    }

    /// <summary>
    /// Kills the player, setting the game state to post game
    /// </summary>
    private void Die() {
        this.gameManager.SetPostGame(this.Opponent.PlayerType, WinReason.Health);

        Debug.Log($"[{this.PlayerType}] Died");
    }

    /// <summary>
    /// Starts this player's turn, enabling input
    /// </summary>
    public void StartTurn() {
        if (this.input) this.input.enabled = true;
        Debug.Log($"[{this.PlayerType}] Turn Started");
    }

    /// <summary>
    /// Ends this player's turn, disables input, and clears the input text
    /// </summary>
    public void EndTurn() {
        if (this.input) this.input.enabled = false;
        this.InputText = string.Empty;
        this.InputTextUpdated?.Invoke(this, this.InputText);
        Debug.Log($"[{this.PlayerType}] Turn Ended");
    }

    /// <summary>
    /// Enable or disable the player's input
    /// </summary>
    public void ToggleInput(bool enabled) => this.input.enabled = enabled;
}
