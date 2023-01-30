using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamReleaseBehavior : MonoBehaviour {

    // release camera to previous cam

    public float transitionTime = 0.5f;

    public void ReleaseCam()
    {
        CameraManager.instance.SwitchToCam(CameraManager.instance.GetPreviousCam(), transitionTime);
    }

}
