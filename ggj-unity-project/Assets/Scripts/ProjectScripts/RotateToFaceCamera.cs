using System.Collections;
using System.Collections.Generic;
using GGJ2022;
using UnityEngine;

public class RotateToFaceCamera : MonoBehaviour
{
    // [SerializeField]
    Transform _targetCamera;

    [SerializeField]
    Vector3 _baseEulerRotation; 

    public Transform TargetCamera
    {
        set { _targetCamera = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        _targetCamera = TransformCamManager.instance.TargetCamera.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (_targetCamera != null)
        {
            //var rotation = Quaternion.FromToRotation(transform.forward, _targetCamera.forward) *
            //               Quaternion.FromToRotation(transform.up, _targetCamera.up);

            //transform.rotation *= rotation

            var intermediate = Quaternion.LookRotation(-1f * _targetCamera.forward, _targetCamera.up);

            // preserve the up axis
            var correction = Quaternion.FromToRotation(intermediate * Vector3.up, Vector3.up);

            transform.rotation = correction * intermediate * Quaternion.Euler(_baseEulerRotation);


            //transform.LookAt(_targetCamera, Vector3.up);
            //transform.rotation *= _originalLocalRotation; 
        }
    }
}
