using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Binding", menuName = "Spellbound/Binding")]
public class Binding : ScriptableObject {
    public InputAction SubmitAction;
    public InputAction BackspaceAction;

    private void OnEnable() {
        this.SubmitAction.Enable();
        this.BackspaceAction.Enable();
    }

    private void OnDisable() {
        this.SubmitAction.Disable();
        this.BackspaceAction.Disable();
    }
}
