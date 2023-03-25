using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    private string currentInput;

    private void Awake() {
        GameManager.Instance.Player = this;
    }

    private void Update() {
        if (Keyboard.current.enterKey.wasPressedThisFrame) {
            this.Submit();
        }
    }

    private void OnEnable() {
        Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable() {
        Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnTextInput(char character) {
        if (Char.IsLetter(character)) {
            this.TypeCharacter(Char.ToUpper(character));
        } else if (character == '\b') {
            this.Backspace();
        }
    }

    /// <summary>
    /// Add character to current input
    /// </summary>
    /// <param name="character">The character to add</param>
    public void TypeCharacter(char character) {
            currentInput += character;
            Debug.Log($"INPUT ({character}) -> {currentInput}");
            this.UpdateInputText();
    }

    /// <summary>
    /// Remove last character from current input, if possible
    /// </summary>
    /// <returns>True if last character was removed, otherwise false</returns>
    public bool Backspace() {
        bool canBackspace = currentInput.Length > 0;
        if (canBackspace) {
            currentInput = currentInput.Remove(currentInput.Length - 1);
            Debug.Log($"BACKSPACE -> {currentInput}");
        }
        this.UpdateInputText();
        return canBackspace;
    }

    /// <summary>
    /// Submit the current input and clear it
    /// </summary>
    public void Submit() {
        Debug.Log($"SUBMIT -> ({currentInput})");
        GameManager.Instance.Opponent.TakeDamage(currentInput.Length);
        currentInput = "";
        this.UpdateInputText();
    }

    private void UpdateInputText() => UserInterfaceManager.instance.UpdatePlayerInput(currentInput);
}
