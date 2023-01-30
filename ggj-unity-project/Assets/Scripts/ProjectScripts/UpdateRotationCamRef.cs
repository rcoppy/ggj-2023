using UnityEngine;
using System.Collections;

namespace GGJ2022
{
    public class UpdateRotationCamRef : MonoBehaviour
    {
        RotateToFaceCamera _rotator;

        // Use this for initialization
        void Start()
        {
            _rotator = GetComponent<RotateToFaceCamera>();

            CameraManager.instance.OnTransitionEnded += SetCam;
            CameraManager.instance.OnTransitionStarted += SetCam;
        }

        private void OnEnable()
        {
            CameraManager.instance.OnTransitionEnded += SetCam;
            CameraManager.instance.OnTransitionStarted += SetCam;
        }

        private void OnDisable()
        {
            CameraManager.instance.OnTransitionEnded -= SetCam;
            CameraManager.instance.OnTransitionStarted -= SetCam;
        }

        void SetCam(Camera cam)
        {
            _rotator.TargetCamera = cam.transform;
        }
    }
}
