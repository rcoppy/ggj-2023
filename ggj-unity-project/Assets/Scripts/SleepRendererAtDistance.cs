using System;
using System.Collections;
using System.Collections.Generic;
using GGJ2022;
using Unity.VisualScripting;
using UnityEngine;

public class SleepRendererAtDistance : MonoBehaviour
{
    // [SerializeField] private GameObject _referenceRenderer;
    // [SerializeField] private float _lengthCoefficient = 1f;
    [SerializeField] private float _secondsPerCheck = 0.3f;
    [SerializeField] private float _distance = 50f;
    
    private float _timeOfLastCheck = 0f;
    private GameObject _player;
    private Renderer _myRenderer;
    private Renderer[] _childRenderers; 
    
    private void Awake()
    {
        _myRenderer = GetComponent<Renderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // _length = _referenceRenderer.GetComponent<Renderer>().bounds.size.x;
        _timeOfLastCheck = Time.time;
        _player = LevelStateManager.Instance.PlayerObject;
        _childRenderers = GetComponentsInChildren<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - _timeOfLastCheck > _secondsPerCheck)
        {
            var flag = (_player.transform.position - transform.position).sqrMagnitude < _distance * _distance;

            if (_myRenderer)
            {
                _myRenderer.enabled = flag;
            }

            foreach (var r in _childRenderers)
            {
                r.enabled = flag; 
            }
            
            _timeOfLastCheck = Time.time; 
        }
    }
}
