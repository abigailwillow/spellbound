using System.Collections.Generic;
using UnityEngine;

public class MenuStateMachine {
    private BaseMenuState currentState;
    private Dictionary<MenuState, BaseMenuState> states;

    public MenuStateMachine(GameManager gameManager) {
        this.states = new Dictionary<MenuState, BaseMenuState>() {
            { MenuState.Menu, new MainMenuState(this, gameManager) },
            { MenuState.Name, new NameMenuState(this, gameManager) },
            { MenuState.Connecting, new ConnectingMenuState(this, gameManager) },
            { MenuState.Sprite, new SpriteMenuState(this, gameManager) },
            { MenuState.Tutorial, new TutorialMenuState(this, gameManager) }
        };
    }

    /// <summary>Exits the previous menu state, then enters the new menu state</summary>
    public void SetMenuState(MenuState menuState) {
        if (this.currentState != null) this.currentState.Exit();
        this.currentState = this.states[menuState];
        this.currentState.Enter();

        Debug.Log($"Menu state set to {menuState}");
    }

    public void Update() => this.currentState.Update();

    /// <summary>Handles the given input for the current menu state and calls the associated function</summary>
    public void HandleInput(PlayerController player, string input) => this.currentState.OnInputSubmitted(player, input);
}

/// <summary>The state of the menu, if the game state is also set to menu</summary>
public enum MenuState {
    None,
    Menu,
    Connecting,
    Name,
    Sprite,
    Tutorial
}