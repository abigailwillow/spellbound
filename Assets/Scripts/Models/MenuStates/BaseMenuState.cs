using System;
using System.Collections.Generic;

public abstract class BaseMenuState {
    protected MenuStateMachine stateMachine;
    protected GameManager gameManager;
    private Dictionary<string, Action<PlayerController, string>> inputActions = new Dictionary<string, Action<PlayerController, string>>();

    public BaseMenuState(MenuStateMachine stateMachine, GameManager gameManager) {
        this.stateMachine = stateMachine;
        this.gameManager = gameManager;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

    public virtual void OnInputSubmitted(PlayerController player, string input) {
        input = input.ToLower();
        if (this.inputActions.ContainsKey(input)) {
            this.inputActions[input](player, input);
        } else {
            this.gameManager.UIManager.SetInstruction($"Invalid command {input}", () => this.Enter());
        }
    }
    public void AddInput(string input, Action<PlayerController, string> action) => this.inputActions.Add(input.ToLower(), action);
}