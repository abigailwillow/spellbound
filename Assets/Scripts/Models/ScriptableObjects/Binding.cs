using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Binding", menuName = "Spellbound/Binding")]
public class Binding : ScriptableObject {
    /// <summary>The binding for submitting, for example enter</summary>
    public InputAction SubmitAction;
    /// <summary>The binding for removing the last character</summary>
    public InputAction BackspaceAction;

    public void OnEnable() {
        this.SubmitAction.Enable();
        this.BackspaceAction.Enable();
    }

    public void OnDisable() {
        this.SubmitAction.Disable();
        this.BackspaceAction.Disable();
    }
}
