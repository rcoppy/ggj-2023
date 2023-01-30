using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

namespace GGJ2022
{
    // useful for scripting zone-based events in-editor
    public class TriggerMapper : MonoBehaviour
    {
        [SerializeField]
        UnityEvent OnTriggerActivated;

        [SerializeField]
        UnityEvent OnTriggerDeactivated;

        [Header("Which transforms activate trigger?")]
        [SerializeField]
        List<Transform> _triggeringTransforms;
        
        // prevent double trigger (if player object has multiple colliders)
        HashSet<Transform> _currentTransforms;

        private void Awake()
        {
            _currentTransforms = new HashSet<Transform>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_currentTransforms.Contains(other.transform) && 
                _triggeringTransforms.Contains(other.transform))
            {
                _currentTransforms.Add(other.transform);
                OnTriggerActivated?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_currentTransforms.Contains(other.transform))
            {
                _currentTransforms.Remove(other.transform);
                OnTriggerDeactivated?.Invoke();
            }
        }
    }
}