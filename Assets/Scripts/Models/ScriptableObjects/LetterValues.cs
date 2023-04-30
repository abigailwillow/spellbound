using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LetterValues", menuName = "Spellbound/Letter Values")]
public class LetterValues : ScriptableObject {
    public List<LetterValue> Letters = new List<LetterValue>() {
        new LetterValue('a', 1),
        new LetterValue('b', 3),
        new LetterValue('c', 3),
        new LetterValue('d', 2),
        new LetterValue('e', 1),
        new LetterValue('f', 4),
        new LetterValue('g', 2),
        new LetterValue('h', 4),
        new LetterValue('i', 1),
        new LetterValue('j', 8),
        new LetterValue('k', 5),
        new LetterValue('l', 1),
        new LetterValue('m', 3),
        new LetterValue('n', 1),
        new LetterValue('o', 1),
        new LetterValue('p', 3),
        new LetterValue('q', 10),
        new LetterValue('r', 1),
        new LetterValue('s', 1),
        new LetterValue('t', 1),
        new LetterValue('u', 1),
        new LetterValue('v', 4),
        new LetterValue('w', 4),
        new LetterValue('x', 8),
        new LetterValue('y', 4),
        new LetterValue('z', 10)
    };

    public int GetValue(char letter) => Letters.Find(l => l.Letter == letter).Value;

    [System.Serializable] public class LetterValue {
        public char Letter;
        public int Value;

        public LetterValue(char letter, int value) {
            Letter = letter;
            Value = value;
        }
    }
}
