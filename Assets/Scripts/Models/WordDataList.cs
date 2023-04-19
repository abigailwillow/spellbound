using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class WordDataList {
    [SerializeField] private List<WordData> words;
    public List<WordData> Words => this.words;
    public WordDataList(params WordData[] words) => this.words = new List<WordData>(words);

    public static WordDataList Deserialize(TextAsset json) => JsonUtility.FromJson<WordDataList>(json.text);

    public TextAsset Serialize() => new TextAsset(JsonUtility.ToJson(this));

    public WordData Get(string word) => this.words.Find(w => w.Word.ToLower() == word.ToLower());

    public WordData GetRandom() => this.words[UnityEngine.Random.Range(0, this.words.Count)]; 

    public bool Contains(string word) => this.words.Exists(w => w.Word.ToLower() == word.ToLower());
}