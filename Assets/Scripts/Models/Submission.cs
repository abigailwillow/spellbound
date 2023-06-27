public class Submission {
    public static Submission PreviousSubmission { get; private set; }
    /// <summary>
    /// Whether or not the submission was valid
    /// </summary>
    public bool Valid => this.Word != null;
    /// <summary>
    /// The input string that was submitted
    /// </summary>
    public string Input { get; private set; }
    /// <summary>
    /// The word that was submitted
    /// </summary>
    public WordData Word { get; private set; }
    /// <summary>
    /// The relation of this word to the last played word
    /// </summary>
    /// <remarks>Defaults to WordRelation.None if the submission was invalid</remarks>
    public WordRelation Relation { get; private set; }
    /// <summary>
    /// The damage that should be dealt to the player
    /// </summary>
    /// <remarks>Null if the submission was invalid</remarks>
    public int? Damage { get; private set; }
    /// <summary>
    /// Whether or not the damage of this word should be healed instead
    /// </summary>
    /// <remarks>Null if the submission was invalid</remarks>
    public bool? Healing { get; private set; }
    private GameManager gameManager;

    public Submission(string input, string lastInput) {
        this.gameManager = GameManager.Instance;
        this.Input = input;
        this.Relation = this.GetWordRelation(input);
        this.Damage = this.CalculateDamage(input, this.Relation);
    }

    /// <summary>
    /// Gets the relation of the given word to the previously played word
    /// </summary>
    /// <param name="input">The currently submitted input</param>
    /// <returns>The relation of the current input to the most previous submission</returns>
    private WordRelation GetWordRelation(string input) {
        WordData previousWord = this.gameManager.WordList.Get(Submission.PreviousSubmission?.Input);
        return previousWord != null ? previousWord.GetWordRelation(input) : WordRelation.None;
    }

    /// <summary>
    /// Calculates the damage that should be dealt to the player
    /// </summary>
    /// <param name="input"></param>
    /// <param name="relation"></param>
    /// <returns></returns>
    private int CalculateDamage(string input, WordRelation relation) {
        string previousInput = Submission.PreviousSubmission?.Input;
        int baseDamage = 0;
        int opponentDamage = 0;
        foreach (char letter in input.ToLower()) baseDamage += this.gameManager.LetterValues.GetValue(letter);
        foreach (char letter in previousInput.ToLower()) opponentDamage += this.gameManager.LetterValues.GetValue(letter);
        return relation switch {
            WordRelation.Synonym => baseDamage * 2,
            WordRelation.Antonym => opponentDamage * 2,
            WordRelation.Related => baseDamage * 2,
            _ => baseDamage
        };
    }
}