using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    [SerializeField] public DamagePayload Damage;
    private void OnTriggerEnter(Collider other)
    {
        Health h = other.GetComponent<Health>();
        if (h != null)
        {
           h.GetDamaged(Damage); 
        }
    }
}

public class DamagePayload
{
    public int DamageAmount;
}
