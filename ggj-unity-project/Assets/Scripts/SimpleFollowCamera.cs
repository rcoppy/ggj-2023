using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleFollowCamera : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] private float _yawAngle = 0f;
    [SerializeField] private float _pitchAngle = 0f;
    [SerializeField] private float _yawSensitivity = 1f;
    [SerializeField] private bool _invertYawSteering = false;
    [SerializeField] private float _locationSnapCoefficient = 4f;
    [SerializeField] private float _occlusionCheckOffset = 2f;
    [SerializeField] private LayerMask _occluderLayer; 
    
    private Vector3 _offset;
    private Vector3 _originalForward;
    private Quaternion _originalRotation;
    private float _yawAxis = 0f; 
    
    LayerMask _trackLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        _trackLayer = _occluderLayer; // LayerMask.GetMask(_occluderLayerName);
        _offset = transform.position - _target.position;
        _originalForward = _target.forward;
        _originalRotation = Quaternion.FromToRotation(_target.forward, transform.forward); 
    }

    bool GetIsTargetVisible()
    {
        var origin = transform.position + -1 * _occlusionCheckOffset * transform.up; 
        var delta = _target.position - origin;
        Debug.DrawLine(origin, origin + delta);
        return !Physics.Raycast(origin, delta.normalized, delta.magnitude, _trackLayer);
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        _yawAngle += (_invertYawSteering ? -1f : 1f) * 180f * _yawSensitivity * Time.fixedDeltaTime * _yawAxis;
        var yawRot = Quaternion.AngleAxis(_yawAngle, _target.up);

        var visible = GetIsTargetVisible(); 
        var pitchDir = visible ? 0f : 1f; 
        _pitchAngle += 90f * Time.fixedDeltaTime * pitchDir;
        _pitchAngle = Mathf.Clamp(_pitchAngle, -20f, 80f); 
        var pitchRot = Quaternion.AngleAxis(_pitchAngle, _target.right);

        if (visible)
        {
            _pitchAngle *= 0.995f; 
        }
        else
        {
            Debug.Log("not visible");
        }
        
        transform.position = Vector3.Lerp(transform.position,
            _target.position + yawRot * pitchRot * Quaternion.FromToRotation(_originalForward, _target.forward) * _offset,
            Time.fixedDeltaTime * _locationSnapCoefficient);

        // var worldRot = Quaternion.LookRotation(Vector3.forward, Vector3.up);
        var localRot = yawRot * pitchRot * Quaternion.LookRotation(_originalRotation * _target.forward, Vector3.up);
        var clampedRot = Quaternion.LookRotation(_target.position - transform.position, Vector3.up);
        var targetRot = Quaternion.Slerp(localRot, clampedRot, 0.3f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * 2f);

        // transform.rotation *= Quaternion.FromToRotation(transform.up, Vector3.up); 
    }

    public void UpdateYawAxis(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
             _yawAxis = context.action.ReadValue<Vector2>().x;
        } else if (context.canceled)
        {
            _yawAxis = 0f; 
        }
    }

    public void SetYawAngle(float angle)
    {
        _yawAngle = angle; 
    }
}
