public class Submission {
    public static Submission PreviousSubmission { get; private set; }
    /// <summary>Whether or not the submission was valid</summary>
    public bool Valid => this.Word != null;
    /// <summary>The input string that was submitted</summary>
    public string Input { get; private set; }
    /// <summary>The word that was submitted</summary>
    public WordData Word { get; private set; }
    /// <summary>The relation of this word to the last played word</summary>
    /// <remarks>Defaults to WordRelation.None if the submission was invalid</remarks>
    public WordRelation Relation { get; private set; }
    /// <summary>The damage that should be dealt to the player</summary>
    /// <remarks>Zero if the submission was invalid</remarks>
    public int Damage { get; private set; }
    /// <summary>Whether or not the damage of this word should be healed instead</summary>
    /// <remarks>False if the submission was invalid</remarks>
    public bool Healing { get; private set; }
    private GameManager gameManager;

    public Submission(string input) {
        this.gameManager = GameManager.Instance;
        this.Input = input;
        this.Word = this.gameManager.WordList.Get(input);
        this.Relation = this.Word?.GetRelation(Submission.PreviousSubmission?.Input) ?? WordRelation.None;
        this.Damage = this.CalculateDamage();
        this.Healing = this.Relation == WordRelation.Synonym;
        PreviousSubmission = this;
    }

    /// <summary>Gets the relation of the given word to the previously played word</summary>
    /// <returns>The relation of the current input to the most previous submission</returns>
    private WordRelation GetWordRelation() {
        string previousInput = Submission.PreviousSubmission?.Input;
        WordData previousWord = this.gameManager.WordList.Get(Submission.PreviousSubmission?.Input);
        return previousWord != null ? previousWord.GetRelation(previousInput) : WordRelation.None;
    }

    /// <summary>Calculates the damage that should be dealt to the player</summary>
    private int CalculateDamage() {
        string previousInput = Submission.PreviousSubmission?.Input ?? string.Empty;
        int baseDamage = 0;
        int opponentDamage = 0;
        foreach (char letter in this.Input.ToLower()) baseDamage += this.gameManager.LetterValues.GetValue(letter);
        foreach (char letter in previousInput.ToLower()) opponentDamage += this.gameManager.LetterValues.GetValue(letter);
        return this.Relation switch {
            WordRelation.Synonym => baseDamage * 2,
            WordRelation.Antonym => opponentDamage * 2,
            WordRelation.Related => baseDamage * 2,
            _ => baseDamage
        };
    }
}