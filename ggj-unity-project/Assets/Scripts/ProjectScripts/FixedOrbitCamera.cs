using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCoords))]
public class FixedOrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform _focus;

    [SerializeField]
    Transform _trackedTarget;

    [SerializeField]
    float _safeAngle = 35f; // degrees

    [SerializeField]
    float _minDistance = 2f; // meters away from focus before orbit trigger

    SphereCoords _sphereCoords;

    // Use this for initialization
    void Start()
    {
        _sphereCoords = GetComponent<SphereCoords>();

        transform.position = _focus.transform.position + _sphereCoords.GetRectFromSphere();

        transform.LookAt(_focus, Vector3.up);

    }

    // Update is called once per frame
    void Update()
    {
        if ((_trackedTarget.position - _focus.position).magnitude > _minDistance)
        {
            Vector3 toTarget = _trackedTarget.position - _focus.position;
            Vector3 toCamera = transform.position - _focus.position;

            float angleDifference = Vector3.Angle(toTarget, toCamera);
            Vector3 linearDifference = toCamera - toTarget;

            float sign = -1f * Mathf.Sign(Vector3.Dot(transform.right, linearDifference));

            if (angleDifference > _safeAngle)
            {
                _sphereCoords.yaw += sign * angleDifference * Time.deltaTime;
            }

            transform.position = _focus.transform.position + _sphereCoords.GetRectFromSphere();

            transform.LookAt(_focus, Vector3.up);
            // transform.rotation *= Quaternion.FromToRotation(transform.forward, _focus.position - transform.position);
        }
    }


}
