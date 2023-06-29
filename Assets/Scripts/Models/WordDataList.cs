using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class WordDataList {
    [SerializeField] private List<WordData> words;
    /// <summary>A list of all the words in this data set</summary>
    public List<WordData> Words => this.words;
    public WordDataList(params WordData[] words) => this.words = new List<WordData>(words);

    /// <summary>Serializes this word data list to a JSON string</summary>
    public TextAsset Serialize() => new TextAsset(JsonUtility.ToJson(this));

    /// <summary>Deserializes a JSON string to a word data list</summary>
    public static WordDataList Deserialize(TextAsset json) => JsonUtility.FromJson<WordDataList>(json.text);

    /// <summary>Gets the word data for the given string</summary>
    public WordData Get(string word) => this.words.Find(w => w.Word.ToLower() == word.ToLower());

    /// <summary>Returns true if the given word is in this data set</summary>
    public bool Contains(string word) => this.words.Exists(w => w.Word.ToLower() == word.ToLower());
}