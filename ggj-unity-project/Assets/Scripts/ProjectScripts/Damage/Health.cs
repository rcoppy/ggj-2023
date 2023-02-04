using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    public int StartingHealth;
    private int _currentHealth;
    
    // Start is called before the first frame update
    void OnEnable()
    {
        _currentHealth = StartingHealth;
    }

    public void GetDamaged(DamagePayload dmg)
    {
        
    } 
}
