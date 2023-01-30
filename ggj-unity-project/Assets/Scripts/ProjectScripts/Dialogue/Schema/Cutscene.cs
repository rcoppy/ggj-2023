using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace GGJ2022.Dialogue.Schema
{
    [Serializable]
    public class Cutscene : ScriptableObject
    {
        public string Title;
        public List<Speech> Exchanges;
        public UnityEvent Pre;
        public UnityEvent Post;

        public void Initialize(string title)
        {
            Title = title;
            Exchanges = new List<Speech>();
            Pre = new UnityEvent();
            Post = new UnityEvent();
        }
    }
}