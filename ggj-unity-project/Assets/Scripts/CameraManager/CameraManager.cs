using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{

    public Camera startCam; // fallback if previousCam is undefined

    private bool isTransitioning = false; 
    private Camera activeCam;
    private Camera previousCam;

    Camera[] _camerasInScene; 

    [SerializeField]
    [Tooltip("assigning a user-defined transition camera is optional")]
    private Camera transitionCam;

    private Transform transitionStartTransform;  
   
    public static CameraManager instance = null;              //Static instance of CameraManager which allows it to be accessed by any other script.

    public delegate void TransitionStarted(Camera newCam);
    public delegate void TransitionEnded(Camera finalCam);

    public TransitionStarted OnTransitionStarted;
    public TransitionEnded OnTransitionEnded;

    #region getters/setters

    public bool GetIsTransitioning()
    {
        return isTransitioning; 
    }

    public Camera GetActiveCam()
    {
        return activeCam; 
    }

    public Camera GetPreviousCam()
    {
        return previousCam;
    }
    #endregion

    #region monobehaviour 
    void Awake()
    {
        #region singleton
        //Check if instance already exists
        if (instance == null)

            //if not, set instance to this
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a CameraManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
        #endregion

        // user can define their own camera if they want, otherwise we use this default
        if (transitionCam == null)
        {
            transitionCam = gameObject.AddComponent<Camera>();
        }

        if (startCam == null)
        {
            print("define a default camera in the inspector");
        }

        _camerasInScene = FindObjectsOfType<Camera>();

        foreach (var cam in _camerasInScene)
        {
            cam.enabled = false;
        }

        activeCam = startCam;

        SwitchToCam(startCam);

    }

    #endregion

    #region methods
    public void SwitchToCam(Camera cam)
    {
        // instantaneous cam switch, no time delayed lerp

        // check camera is real
        if (cam != null)
        {
            previousCam = activeCam;
            activeCam = cam;

            // turn off old camera
            if (previousCam != null)
            {
                previousCam.enabled = false;
            }

            // turn on new camera
            activeCam.enabled = true;

            // make sure transition camera is off 
            if (transitionCam != null)
            {
                transitionCam.enabled = false;
            }

            // make sure manager state is 'not transitioning' 
            isTransitioning = false;

            OnTransitionEnded?.Invoke(activeCam);
        }
        else
        {
            print("camera to switch to doesn't exist");
        }
    }

    public void TimedSwitchToCam(Camera cam, float duration)
    {
        // this wrapper is for legacy compatibility reasons
        SwitchToCam(cam, duration);
    }

    public void SwitchToCam(Camera cam, float duration)
    {
        // time-delayed cam switch using coroutine

        // check camera is real
        if (cam != null)
        {
            // a transition might already be happening--need to handle that 
            if (isTransitioning)
            {
                // we're interrupting an in-progress transition 
                // so stop coroutine if running
                StopCoroutine("SwitchingCamRoutine");

                // set start transform to transitionCamera's current position 
                transitionStartTransform = transitionCam.transform;
                OnTransitionEnded?.Invoke(transitionCam);
                // don't modify previousCam--leave it
            }
            else
            {
                previousCam = activeCam;

                if (previousCam == null)
                {
                    previousCam = startCam; 
                }

                transitionStartTransform = previousCam.transform;
            }

            isTransitioning = true;

            previousCam = activeCam;
            activeCam = cam;

            // turn off old camera
            if (previousCam != null)
            {
                previousCam.enabled = false;
            }

            // turn on transition camera
            transitionCam.enabled = true;

            // make sure active (target) camera is off 
            activeCam.enabled = false;

            OnTransitionStarted?.Invoke(transitionCam);

            StartCoroutine(SwitchingCamRoutine(duration));
        }
        else
        {
            print("camera to switch to doesn't exist");
        }
    }

    private IEnumerator SwitchingCamRoutine(float duration)
    {
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            transitionCam.transform.position = Vector3.Slerp(transitionStartTransform.position, activeCam.transform.position, timeElapsed/duration);
            transitionCam.transform.rotation = Quaternion.Slerp(transitionStartTransform.rotation, activeCam.transform.rotation, timeElapsed / duration);
            yield return null;
        }

        isTransitioning = false;
        activeCam.enabled = true;
        transitionCam.enabled = false;

        OnTransitionEnded?.Invoke(activeCam);
    }
    #endregion

}