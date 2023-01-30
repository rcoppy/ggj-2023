using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSetBehavior : MonoBehaviour {

    // user sets these in-editor
    public float transitionTime = 0.5f;
    public Camera cam; 

    // setcam is invoked as a callback
    public void SetCam()
    {
        CameraManager.instance.SwitchToCam(cam, transitionTime);
    }
}
