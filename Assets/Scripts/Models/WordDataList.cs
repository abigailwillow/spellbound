using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class WordDataList {
    [SerializeField] private List<WordData> words;
    public List<WordData> Words => this.words;
    public WordDataList(params WordData[] words) => this.words = new List<WordData>(words);

    public TextAsset Serialize() => new TextAsset(JsonUtility.ToJson(this));

    public static WordDataList Deserialize(TextAsset json) => JsonUtility.FromJson<WordDataList>(json.text);

    public WordData Get(string word) => this.words.Find(w => w.Word.ToLower() == word.ToLower());

    public bool Contains(string word) => this.words.Exists(w => w.Word.ToLower() == word.ToLower());
}