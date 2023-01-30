using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

[RequireComponent(typeof(SphereCoords))]
public class FollowCamera3D : MonoBehaviour
{

    [SerializeField]
    Transform _target; // used for cartesian tracking

    [SerializeField]
    bool _trackRotationToTarget = true; 

    [SerializeField]
    float _lookAheadFactor = 0.67f; // multiplier of sphere coords radius

    [SerializeField] private float _lookAheadSeconds = 1.5f; 
    
    [SerializeField]
    float _lookLerp = 0.007f; 

    Vector3 _lookTarget; // used for angular tracking

    [SerializeField]
    float _angularLerpFactor = 0.08f; // degrees

    
    [SerializeField]
    Vector3 _safeZoneExtent;

    [SerializeField]
    Vector3 _safezoneOffset;


    [SerializeField]
    Vector3 _outerZoneExtent;

    [SerializeField]
    Vector3 _outerZoneOffset;


    bool _moving = false;

    Vector3 _velocity;
    // Quaternion _angularVelocity; 

    [SerializeField]
    float _friction = 0.1f;

    [SerializeField]
    float _acceleration = 12f;

    [SerializeField]
    float _catchupDampingFactor = 3.5f; 

    [SerializeField]
    float _maxSpeed = 20f;

    [SerializeField]
    bool _killAcceleration = true;

    [SerializeField]
    bool _scaleBoundsWithAspect = true;

    float _aspectScaling = 1f;

    float _baseAspectScaling = 1f;

    float _baseAspectRatio = 16f / 9f;

    Vector3 _originPosition; 

    //[SerializeField]
    //AdaptiveAspectRatio _aspectTracker;

    //AdaptCameraSizeToAspect _aspectAdapter;

    Vector3 _lastTargetPosition;

    [SerializeField]
    Vector3 _cameraForward = Vector3.zero; 

    SphereCoords _sphereCoords;

    // moving average velocity
    Vector3[] _velocityFrames;
    int _velFrameId;
    Vector3 _averageVelocity; 

    private void HandleAspectUpdate(float ratio)
    {
        _aspectScaling = _baseAspectScaling * ratio / _baseAspectRatio; 
    }

    private void OnEnable()
    {
        //if (_aspectTracker)
        //{
        //    _aspectTracker.OnAspectUpdated += HandleAspectUpdate;
        //}
    }

    private void OnDisable()
    {
        //if (_aspectTracker)
        //{
        //    _aspectTracker.OnAspectUpdated -= HandleAspectUpdate;
        //}
    }

    private void Awake()
    {
        _velocity = Vector3.zero;
        _velocityFrames = new Vector3[30];
        _velFrameId = 0;

        for (int i = 0; i < _velocityFrames.Length; i++)
        {
            _velocityFrames[i] = Vector3.zero; 
        }

        _lookTarget = _target.position; 
        _lastTargetPosition = _target.position;
        _originPosition = transform.position; 

        //if (_aspectTracker)
        //{
        //    _baseAspectRatio = _aspectTracker.AspectRatio;
        //}

        //_aspectAdapter = GetComponent<AdaptCameraSizeToAspect>();

        _sphereCoords = GetComponent<SphereCoords>();
    }

    void UpdateAverageVelocity()
    {
        _velFrameId %= _velocityFrames.Length;

        _velocityFrames[_velFrameId] = GetInstantaneousTargetVelocity();

        Vector3 total = Vector3.zero;

        for (int i = 0; i < _velocityFrames.Length; i++)
        {
            total += _velocityFrames[i];
        }

        _averageVelocity = total / _velocityFrames.Length;
        _velFrameId++; 
    }

    void UpdateYaw()
    {
        // TODO: work in progress
        if ((_lookTarget - _target.position).magnitude > 1f)
        {
            Vector3 toTarget = _lookTarget - _target.position;
            Vector3 toCamera = transform.position - _target.position;

            float angleDifference = Vector3.Angle(toTarget, toCamera);
            Vector3 linearDifference = toCamera - toTarget;

            float sign = -1f * Mathf.Sign(Vector3.Dot(transform.right, linearDifference));

            if (angleDifference > 10f)
            {
                _sphereCoords.yaw -= sign * angleDifference * Time.deltaTime;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAverageVelocity();

        float scaleFactor = GetScalingFromCamera();

        var outerBounds = new Bounds(_originPosition + scaleFactor * _outerZoneOffset, scaleFactor * _outerZoneExtent);
        var innerBounds = new Bounds(_originPosition + scaleFactor * _safezoneOffset, scaleFactor * _safeZoneExtent);

        var playerPos = _target.transform.position + _safezoneOffset;

        if (!_moving && !outerBounds.Contains(playerPos))
        {
            _moving = true;

            // reset velocity
            if (_killAcceleration)
            {
                _velocity = Vector3.zero;
            }

            //Vector3 targetPos = _target.position;
            //targetPos.z = transform.position.z;

            //var direction = (targetPos - transform.position).normalized;
            //_velocity = direction * _maxSpeed; 

        }
        else if (_moving && innerBounds.Contains(playerPos))
        {
            _moving = false;
        }

        if (_moving)
        {
            // Vector3 targetVelocity = _averageVelocity;
            // float damping = 1f;
            //
            // // if camera is moving opposite the player
            // // slow it down
            // if (Vector3.Dot(targetVelocity, _velocity) < 0f)
            // {
            //     damping = 1 / _catchupDampingFactor;
            // }
            //
            // Vector3 targetPos = playerPos; // _target.position;
            // // targetPos.z = transform.position.z;
            //
            // var direction = (targetPos - _originPosition).normalized;
            // var acceleration = direction * _acceleration;
            //
            // _velocity *= damping;
            //
            // _velocity += Time.deltaTime * acceleration;

            _velocity = Vector3.Lerp(_velocity, _averageVelocity, _lookLerp); 


        }
        else
        {
            // if velocity is large and inner bounds are tiny
            // camera can yo-yo dizzyingly

            // also experimented with scaling acceleration / max velocity
            // by reciprocal of scale factor but results were too snappy

            if (_velocity.magnitude > innerBounds.extents.magnitude)
            {
                _velocity *= 0.3f;
            }
            else
            {
                _velocity *= (1f - _friction);
            }
        }

        _velocity = Vector3.ClampMagnitude(_velocity, _maxSpeed);

        _originPosition += _velocity * Time.deltaTime;

        _lastTargetPosition = _target.position;

        transform.position = _originPosition + _sphereCoords.GetRectFromSphere();

        if (_trackRotationToTarget)
        {
            // camera rotation

            var constructedVelocity = _averageVelocity;
            constructedVelocity.y = 0; 
            
            Vector3 lookPos = _target.position + _lookAheadSeconds * constructedVelocity;

            // var vel = _averageVelocity;
            // var axis = transform.right;

            // var dot = Vector3.Dot(axis, vel);

            // if (Mathf.Abs(dot) > 0.6f)
            //{
            //    // todo: editor-expose the lookahead distance? 
            //    float dist = _lookAheadFactor * _sphereCoords.radius;

            //    // character move direction
            //    float sign = dot < 0f ? -1f : 1f;

            //    lookPos += sign * dist * axis; 
            //}

            // lookPos += vel; 

            _lookTarget = Vector3.Lerp(_lookTarget, lookPos, _lookAheadFactor);

            Quaternion temp = transform.rotation;
            transform.LookAt(_lookTarget, Vector3.up);
            Quaternion targetRotation = transform.rotation;

            transform.rotation = Quaternion.Slerp(temp, targetRotation, _angularLerpFactor);
            
            // UpdateYaw();
            

        }

        if (_averageVelocity.magnitude > 0.8f && Vector3.Dot(transform.forward, _averageVelocity) < -0.7f)
        {
            float angleDifference = Vector3.Angle(_averageVelocity, transform.forward) % 180f;
            Vector3 linearDifference = _averageVelocity - transform.forward;

            float sign = Mathf.Sign(Vector3.Dot(transform.right, linearDifference));

            // if (angleDifference > 90f)
            // {
            //     _sphereCoords.yaw += sign * angleDifference * Time.deltaTime;
            // }

            //if ((_target.position - transform.position).magnitude < 2f)
            //{
                //_sphereCoords.yaw += 100f * Time.deltaTime;
            //}
        }
    }

    Vector3 GetInstantaneousTargetVelocity()
    {
        return (_target.position - _lastTargetPosition) / Time.deltaTime; 
    }

    float GetScalingFromCamera()
    {
        float scaleFactor = 1f;

        // scaling based on aspect ratio
        //if (_scaleBoundsWithAspect && _aspectTracker)
        //{
        //    scaleFactor = _aspectScaling;

        //    if (_aspectAdapter)
        //    {
        //        scaleFactor /= _aspectAdapter.PortraitModeScaleFactor;
        //    }
        //}

        return scaleFactor; 
    }

    // editor visualization
    void OnDrawGizmos()
    { 
        Color lineColor = Color.yellow;

        float scaleFactor = GetScalingFromCamera();

        Vector3 pos = transform.position;

        if (Application.isPlaying)
        {
            pos = _originPosition; 
        }

        Gizmos.color = lineColor;
        Gizmos.DrawWireCube(pos + scaleFactor * _safezoneOffset, scaleFactor * _safeZoneExtent);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos + scaleFactor * _outerZoneOffset, scaleFactor * _outerZoneExtent);

    }
}
