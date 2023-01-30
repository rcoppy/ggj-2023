using UnityEngine;
using System.Collections;
using TMPro;

namespace GGJ2022.Dialogue
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextIterator : MonoBehaviour
    {
        TextMeshProUGUI _text;

        [SerializeField]
        float _secondsToWait = 0.02f; // 20 milliseconds

        IEnumerator _coroutine;

        [SerializeField]
        bool _shouldIterate = true;

        public delegate void TextDoneIterating();
        public TextDoneIterating OnTextDoneIterating;

        // Use this for initialization
        void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        public void TriggerNewText(string text)
        {
            StopAllCoroutines();

            if (_shouldIterate)
            {
                _coroutine = IterateLetters(text);
                StartCoroutine(_coroutine);
            } else
            {
                _text.text = text; 
            }
        }

        IEnumerator IterateLetters(string text)
        {
            _text.text = "";

            for (int i = 0; i < text.Length; i++)
            {
                
                // webgl error workaround
                float endTime = Time.time + _secondsToWait;

                while (Time.time < endTime)
                {
                    yield return null;
                }

                _text.text += text[i];


                // this throws an index out of bounds error in webgl
                // yield return new WaitForSeconds(_secondsToWait);
            }

            OnTextDoneIterating?.Invoke();
        }
    }
}
