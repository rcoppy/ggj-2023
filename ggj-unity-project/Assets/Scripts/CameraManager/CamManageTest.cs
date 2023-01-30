using UnityEngine;
using System.Collections;

public class CamManageTest : MonoBehaviour {

    public Camera camToSwitchTo; 

	// Use this for initialization
	void Start () {
        CameraManager.instance.SwitchToCam(camToSwitchTo, 1f);
    }
}
