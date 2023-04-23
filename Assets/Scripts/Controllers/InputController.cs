using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class InputController : MonoBehaviour {
    private Binding binding;
    public Binding Binding {
        get => this.binding;
        set {
            this.binding = value;
            this.binding.SubmitAction.performed += callback => this.OnSubmit();
            this.binding.BackspaceAction.performed += callback => this.OnBackspace();
        }
    }
    private PlayerController playerController;

    private void Start() => this.playerController = this.GetComponent<PlayerController>();

    private void OnEnable() => Keyboard.current.onTextInput += OnTextInput;

    private void OnDisable() => Keyboard.current.onTextInput -= OnTextInput;

    public void OnTextInput(char character) {
        if (char.IsLetter(character)) {
            this.playerController.TextInput(char.ToUpper(character).ToString());
        }
    }

    public void OnSubmit() => this.playerController.Submit();

    public void OnBackspace() => this.playerController.Backspace();
}
