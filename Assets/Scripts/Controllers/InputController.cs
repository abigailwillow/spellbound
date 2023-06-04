using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class InputController : MonoBehaviour {
    private Binding binding;
    public Binding Binding {
        get => this.binding;
        set {
            this.binding = value;
            this.binding.SubmitAction.performed += this.OnSubmit;
            this.binding.BackspaceAction.performed += this.OnBackspace;
            this.binding?.OnEnable();
        }
    }
    private PlayerController playerController;
    [SerializeField, Range(0f, 1f)] private float backspaceDelay = 0.5f;
    [SerializeField, Range(0f, 0.1f)] private float backspaceRepeat = 0.03f;
    private float backspaceTime = 0f;

    private void Awake() => TouchScreenKeyboard.Open("");

    private void Start() => this.playerController = this.GetComponent<PlayerController>();

    private void Update() {
        if (this.binding.BackspaceAction.IsPressed() && Time.time > this.backspaceTime) {
            this.backspaceTime = Time.time + this.backspaceRepeat;
            this.playerController.Backspace();
        }
    }

    private void OnEnable() {
        Keyboard.current.onTextInput += OnTextInput;
        this.Binding?.OnEnable();
    }

    private void OnDisable() {
        Keyboard.current.onTextInput -= OnTextInput;
        this.Binding?.OnDisable();
    }

    private void OnDestroy() {
        Keyboard.current.onTextInput -= OnTextInput;
        this.Binding.SubmitAction.performed -= this.OnSubmit;
        this.Binding.BackspaceAction.performed -= this.OnBackspace;
    }

    public void OnTextInput(char character) {
        if (char.IsLetter(character)) this.playerController.TextInput(character.ToString().ToUpper());
    }

    public void OnSubmit(InputAction.CallbackContext _) => this.playerController.Submit();

    public void OnBackspace(InputAction.CallbackContext _) {
        this.backspaceTime = Time.time + this.backspaceDelay;
        this.playerController.Backspace();
    }
}
