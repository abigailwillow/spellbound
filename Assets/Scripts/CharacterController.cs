using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour {
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
            case '\n':
                // TODO: Handle submit
                break;
            default:
                currentInput += character;
                Debug.Log($"INPUT ({character}) -> {currentInput}");
                break;
        }
    }
}
