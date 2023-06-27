using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class WordData {
    /// <summary>
    /// The word itself
    /// </summary>
    public string Word => word.ToLower();
    [SerializeField] private string word;
    /// <summary>
    /// The type of word (noun, verb, adjective, etc.)
    /// </summary>
    public WordType Type => type;
    [SerializeField] private WordType type;
    /// <summary>
    /// The category of the word (animals, food, technology, etc.)
    /// </summary>
    public string Category => category;
    [SerializeField] private string category;
    /// <summary>
    /// A list of synonyms for the word
    /// </summary>
    public List<string> Synonyms => synonyms;
    [SerializeField] private List<string> synonyms;
    /// <summary>
    /// A list of antonyms for the word
    /// </summary>
    public List<string> Antonyms => antonyms;
   [SerializeField] private List<string> antonyms;
    /// <summary>
    /// A list of related words
    /// </summary>
    public List<string> RelatedWords => relatedWords;
    [SerializeField] private List<string> relatedWords;

    public WordData(string key, WordType type, string category, List<string> synonyms, List<string> antonyms, List<string> relatedWords) {
        this.word = key.ToLower();
        this.type = type;
        this.category = category.ToLower();
        this.synonyms = synonyms;
        this.Synonyms.ForEach(synonym => synonym.ToLower());
        this.antonyms = antonyms;
        this.Antonyms.ForEach(antonym => antonym.ToLower());
        this.relatedWords = relatedWords;
        this.RelatedWords.ForEach(relatedWord => relatedWord.ToLower());
    }

    /// <summary>
    /// Returns true if the word is a synonym of this word
    /// </summary>
    public bool IsSynonym(string word) => this.synonyms.Exists(synonym => synonym.ToLower() == word.ToLower());

    /// <summary>
    /// Returns true if the word is an antonym of this word
    /// </summary>
    public bool IsAntonym(string word) => this.antonyms.Exists(antonym => antonym.ToLower() == word.ToLower());

    /// <summary>
    /// Returns true if the word is related to this word
    /// </summary>
    public bool IsRelated(string word) => this.relatedWords.Exists(relatedWord => relatedWord.ToLower() == word.ToLower());

    /// <summary>
    /// Get the relation of this word to the given word
    /// </summary>
    /// <param name="word">The word to check the relation against</param>
    public WordRelation GetRelation(string word) {
        return word switch {
            _ when this.IsSynonym(word) => WordRelation.Synonym,
            _ when this.IsAntonym(word) => WordRelation.Antonym,
            _ when this.IsRelated(word) => WordRelation.Related,
            _ => WordRelation.None
        };
    }
}