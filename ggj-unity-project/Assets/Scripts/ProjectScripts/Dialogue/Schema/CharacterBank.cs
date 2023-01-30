using UnityEngine;
using System.Collections.Generic;
using System;

namespace GGJ2022.Dialogue.Schema
{
    [Serializable]
    public class CharacterBank : ScriptableObject
    {
        // dictionary can't be serialized without a lot of work :(
        Dictionary<string, Character> _characterMap;

        [SerializeField]
        List<Character> _characterList; 

        public Dictionary<string, Character> CharacterMap
        {
            get { return _characterMap; }
        }

        public void Initialize(List<Character> characters)
        {
            _characterList = characters; 
        }

        // call this on first access at runtime
        public void RefreshMap()
        {
            _characterMap = new Dictionary<string, Character>();

            foreach (var c in _characterList)
            {
                _characterMap[c.Name] = c;
            }
        }
    }
}
    

