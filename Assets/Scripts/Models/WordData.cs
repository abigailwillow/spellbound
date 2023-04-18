using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class WordData {
    /// <summary>
    /// The word itself
    /// </summary>
    public string Word => word;
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
        Synonyms.ForEach(synonym => synonym.ToLower());
        this.antonyms = antonyms;
        Antonyms.ForEach(antonym => antonym.ToLower());
        this.relatedWords = relatedWords;
        RelatedWords.ForEach(relatedWord => relatedWord.ToLower());
    }
}