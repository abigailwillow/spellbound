using UnityEngine;

public class TutorialMenuState : BaseMenuState {

    public TutorialMenuState(MenuStateMachine stateMachine, GameManager gameManager) : base(stateMachine, gameManager) {
        this.AddInput("spellbound", (player, input) => {
            this.gameManager.UIManager.SetInstruction(() => this.stateMachine.SetMenuState(MenuState.Name),
                "You just cast your first spell!",
                "That's all there is to it\nYou now know how to play Spellbound",
                "One last thing, you have to pick a name before you start playing\nYou can change it at any time",
                "Have fun casting spells!"
            );
            PlayerPrefs.SetInt("CompletedTutorial", 1);
        });
    }

    public override void Enter() {
        this.gameManager.UIManager.SetInstruction(
            "Welcome to Spellbound!",
            "In this game players take turns spelling words to cast spells on each other",
            "The longer the word, and less common the letters, the more damage your spell will inflict",
            "Words with relations to other words have special effects",
            "Synonyms heal you\nAntonyms deflect incoming spells\nRelated words amplify your spells",
            "You can type EXIT during a game to return to the menu",
            "Type SPELLBOUND to get started"
        );
    }
}