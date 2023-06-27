using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController))]
public class InputController : MonoBehaviour {
    private Binding binding;
    /// <summary>The input bindings to use for this input controller, automatically sets up all bindings</summary>
    public Binding Binding { get => this.binding; set => this.SetupBinding(value); }
    /// <summary>The delay before backspace starts repeating</summary>
    [SerializeField, Range(0f, 1f)] private float backspaceDelay = 0.5f;
    /// <summary>The delay between backspace repeats</summary>
    [SerializeField, Range(0f, 0.1f)] private float backspaceRepeat = 0.03f;
    private PlayerController playerController;
    private float backspaceTime = 0f;

    private void Start() => this.playerController = this.GetComponent<PlayerController>();

    private void Update() {
        if (this.binding.BackspaceAction.IsPressed() && Time.time > this.backspaceTime) {
            this.backspaceTime = Time.time + this.backspaceRepeat;
            this.playerController.Backspace();
        }
    }

    private void OnDestroy() {
        Keyboard.current.onTextInput -= OnTextInput;
        this.Binding.SubmitAction.performed -= this.OnSubmit;
        this.Binding.BackspaceAction.performed -= this.OnBackspace;
    }

    /// <summary>Sets up the binding for the input controller and enables its necessary components</summary>
    /// <param name="binding">The bindings to use</param>
    private void SetupBinding(Binding binding) {
        this.binding = binding;
        Keyboard.current.onTextInput += OnTextInput;
        this.binding.SubmitAction.performed += this.OnSubmit;
        this.binding.BackspaceAction.performed += this.OnBackspace;
        this.binding?.OnEnable();
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
