using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 360f; 
    [SerializeField] private Vector3 _axis = Vector3.forward;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Quaternion dif = Quaternion.FromToRotation(Vector3.forward, transform.forward);
        Quaternion rot = Quaternion.AngleAxis(_rotationSpeed * Time.fixedDeltaTime, _axis);
        transform.rotation = rot * transform.rotation;
    }
}
