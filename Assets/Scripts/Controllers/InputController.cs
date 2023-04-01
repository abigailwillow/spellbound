using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class InputController : MonoBehaviour {
    [SerializeField] private InputAction SubmitAction;
    [SerializeField] private InputAction BackspaceAction;
    private PlayerController PlayerController;

    private void Awake() {
        SubmitAction.performed += callback => this.OnSubmit();
        BackspaceAction.performed += callback => this.OnBackspace();
    }

    private void Start() {
        this.PlayerController = this.GetComponent<PlayerController>();
    }

    private void OnEnable() {
        Keyboard.current.onTextInput += OnTextInput;
        SubmitAction.Enable();
        BackspaceAction.Enable();
    }

    private void OnDisable() {
        Keyboard.current.onTextInput -= OnTextInput;
        SubmitAction.Disable();
        BackspaceAction.Disable();
    }

    public void OnTextInput(char character) {
        if (char.IsLetter(character)) {
            this.PlayerController.TextInput(char.ToUpper(character).ToString());
        }
    }

    public void OnSubmit() => this.PlayerController.Submit();

    public void OnBackspace() => this.PlayerController.Backspace();
}
