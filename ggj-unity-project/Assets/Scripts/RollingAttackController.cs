using System;
using System.Collections;
using System.Collections.Generic;
using GGJ2022;
using GGJ2022.Audio;
using GGJ2022.EnemyAI;
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
    [SerializeField] private float _targetMultiplier = 2f; 
    
     
    
    // Start is called before the first frame update
    void OnEnable()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _playerHealth = GetComponent<Health>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_isRolling)
        {
            if (Time.time - _rollStartTime > _maxRollTime)
            {
                _isRolling = false;
                _playerHealth.IsInvincible = false;
                OnRollEnd?.Invoke();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_isRolling) return;

        _isRolling = false;
        _playerHealth.IsInvincible = false;

        SFXAudioEventDriver.StaticFireSFXEvent("SnailImpact");
        if (collision.gameObject.CompareTag("Targetable"))
        {
            int damage = LevelStateManager.Instance.GetDamageRatio();
            collision.gameObject.GetComponent<EnemyState>()?.DoDamage(damage);
            collision.gameObject.GetComponent<Health>()?.DoDamage(damage);
            
            _rigidbody.AddRelativeForce(0.75f * _rollImpulse * (collision.contacts[0].normal + Vector3.up).normalized, ForceMode.Impulse);
        }
        
        OnRollEnd?.Invoke(); 
    }

    private Health _playerHealth;
    
    public void TriggerRoll(Vector3 direction, bool wasTargeted = false)
    {
        if (_isRolling) return;

        _isRolling = true; 
        _rollStartTime = Time.time;

        _playerHealth.IsInvincible = true; 

        var force = wasTargeted ? _targetMultiplier * _rollImpulse : _rollImpulse; 
        
        _rigidbody.AddRelativeForce(force * direction.normalized, ForceMode.Impulse);
    }
}
