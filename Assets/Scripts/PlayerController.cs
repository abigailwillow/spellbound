using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class PlayerController : MonoBehaviour {
    public TextMeshProUGUI inputText;
    private string currentInput;

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
        } else if (character == '\n') {
            this.Submit();
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
        // TODO: Submit current input and clear it
    }

    private void UpdateInputText() {
        inputText.text = currentInput;
    }
}
