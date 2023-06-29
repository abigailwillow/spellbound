/// <summary>Whether this player is local or remote</summary>
public enum PlayerType {
    None,
    Local,
    Remote
}

/// <summary>The type of word</summary>
public enum WordType {
    Noun,
    Pronoun,
    Verb,
    Adjective,
    Adverb,
    Preposition,
    Conjunction,
    Interjection
}

/// <summary>The state of the game</summary>
public enum GameState {
    None,
    Menu,
    Connecting,
    Playing,
    PostGame
}

/// <summary>The state of the menu, if the game state is also set to menu</summary>
public enum MenuState {
    None,
    Menu,
    Name,
    Sprite,
    Tutorial
}

/// <summary>The reason a player won</summary>
public enum WinReason {
    None,
    Health,
    Time,
    Disconnect
}

/// <summary>The relation of a word to another word</summary>
public enum WordRelation {
    None,
    Synonym,
    Antonym,
    Related
}