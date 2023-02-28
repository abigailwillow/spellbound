using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CharacterController : MonoBehaviour {
    public TextMeshProUGUI inputText;
    private string currentInput;

    private void OnEnable() {
        Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable() {
        Keyboard.current.onTextInput -= OnTextInput;
    }

    private void OnTextInput(char character) {
        switch (character) {
            case '\b':
                if (currentInput.Length > 0) {
                    currentInput = currentInput.Remove(currentInput.Length - 1);
                }
                Debug.Log($"BACKSPACE -> {currentInput}");
                break;
            case '\t':
            case '\r':
            case ' ':
            case '\n': // TODO: Handle submit
                break; // TODO: Create list of allowed characters instead of ignoring certain ones
            default:
                currentInput += character;
                Debug.Log($"INPUT ({character}) -> {currentInput}");
                break;
        }
    }
}
