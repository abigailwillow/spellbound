using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Binding", menuName = "Spellbound/Binding")]
public class Binding : ScriptableObject {
    public InputAction SubmitAction;
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
