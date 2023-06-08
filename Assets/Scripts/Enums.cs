public enum PlayerType {
    None,
    Local,
    Remote
}

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

public enum GameState {
    None,
    Menu,
    Connecting,
    Playing,
    PostGame
}

public enum MenuState {
    None,
    Menu,
    Name
}

public enum WinReason {
    None,
    Health,
    Time,
    Disconnect
}