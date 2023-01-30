using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace GGJ2022 {
    public class UpdatePlayerCameraRefs : MonoBehaviour
    {
        PlayerInput _playerInput;
        RelativeCharacterController _controller;

        // Use this for initialization
        void Start()
        {
            _playerInput = GetComponent<PlayerInput>();
            _controller = GetComponent<RelativeCharacterController>();

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
            _playerInput.camera = cam;
            _controller.ReferenceCamera = cam;
        }
    }
}
