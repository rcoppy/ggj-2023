using System;
using System.Collections;
using System.Collections.Generic;
using GGJ2022;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{

    [SerializeField] private bool ShouldDestroyOnDeath = true;
    [SerializeField] private bool ShouldCountTowardsBodycount = true;

    public bool IsInvincible = false;
    
    
    public int StartingHealth = 10;
    private int _currentHealth;

    public UnityEvent OnDeath;

    public UnityEvent OnDamaged;
    private bool _isDead = false;
    // Start is called before the first frame update
    void OnEnable()
    {
        _currentHealth = StartingHealth;
    }

    public void DoDamage(int dmg)
    {
        if (_isDead || IsInvincible) return;
        OnDamaged?.Invoke();
         
        _currentHealth= Math.Max(0, _currentHealth - dmg);
        if (_currentHealth <= 0)
        {
           OnDeath?.Invoke();
           _isDead = true;
           if (ShouldCountTowardsBodycount)
           {
               LevelStateManager.Instance.IncrementBodyCount();
           }
           if (ShouldDestroyOnDeath)
           {
               Destroy(gameObject);
           }
           
        }
    } 
}
