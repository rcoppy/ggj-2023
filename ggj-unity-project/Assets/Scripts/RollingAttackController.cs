using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingAttackController : MonoBehaviour
{
    public delegate void RollEnd();

    public RollEnd OnRollEnd;

    private Rigidbody _rigidbody;
    private float _rollStartTime;
    private bool _isRolling = false;

    public bool IsRolling => _isRolling; 
    
    [SerializeField] private float _rollImpulse = 10f;
    [SerializeField] private float _maxRollTime = 1f; 
    
    
    
    // Start is called before the first frame update
    void OnEnable()
    {
        _rigidbody = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_isRolling)
        {
            if (Time.time - _rollStartTime > _maxRollTime)
            {
                _isRolling = false;
                OnRollEnd?.Invoke();
            }
        }
        else
        {
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isRolling) return;

        _isRolling = false;
        OnRollEnd?.Invoke(); 
    }

    public void TriggerRoll(Vector3 direction)
    {
        if (_isRolling) return;

        _isRolling = true; 
        _rollStartTime = Time.time; 
        _rigidbody.AddRelativeForce(_rollImpulse * direction.normalized, ForceMode.Impulse);
    }
}
