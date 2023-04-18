using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class WordDataList {
    [SerializeField] private List<WordData> words;
    public List<WordData> Words => words;
    public WordDataList(params WordData[] words) => this.words = new List<WordData>(words);

    public static WordDataList Deserialize(TextAsset json) => JsonUtility.FromJson<WordDataList>(json.text);

    public static TextAsset Serialize(WordDataList wordList) => new TextAsset(JsonUtility.ToJson(wordList));
}