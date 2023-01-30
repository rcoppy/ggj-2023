using UnityEngine;
using System.Collections;

namespace GGJ2022
{
    // instead of manipulating a camera directly,
    // wrap it in a transform and move that transform.
    [RequireComponent(typeof(CameraShaker))]
    public class TransformCamManager : MonoBehaviour
    {
        [Tooltip("transform to manipulate")]
        [SerializeField]
        private GameObject _targetTransform;

        public GameObject TargetCamera
        {
            get { return _targetTransform; }
        }

        [SerializeField]
        bool _useEasing = true; 

        [SerializeField]
        private float _defaultTransitionTime = 0.8f;


        public Transform startCam; // fallback if previousCam is undefined

        private bool isTransitioning = false;
        private Transform activeCam;
        private Transform previousCam;

        private Transform transitionStartTransform;

        public static TransformCamManager instance = null;              //Static instance of CameraManager which allows it to be accessed by any other script.

        public delegate void TransitionStarted(Transform newCam);
        public delegate void TransitionEnded(Transform finalCam);

        public TransitionStarted OnTransitionStarted;
        public TransitionEnded OnTransitionEnded;

        CameraShaker _cameraShaker; 

        #region getters/setters

        public bool GetIsTransitioning()
        {
            return isTransitioning;
        }

        public Transform GetActiveCam()
        {
            return activeCam;
        }

        public Transform GetPreviousCam()
        {
            return previousCam;
        }
        #endregion

        #region monobehaviour 
        void Awake()
        {
            #region singleton

            instance = this;
            //Check if instance already exists
            // if (instance == null)
            // {
            //
            //     //if not, set instance to this
            //     instance = this;
            // }
            //
            // //If instance already exists and it's not this:
            // else if (instance != this)
            // {
            //
            //     //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a CameraManager.
            //     Destroy(gameObject);
            //     return;
            // }
            // Sets this to not be destroyed when reloading scene
            // DontDestroyOnLoad(gameObject);
            #endregion

            // user can define their own camera if they want, otherwise we use this default
            //if (transitionCam == null)
            //{
            //    transitionCam = gameObject.AddComponent<Transform>();
            //}

            if (startCam == null)
            {
                print("define a default camera in the inspector");
            }

            

            activeCam = startCam;

            SwitchToCam(startCam);



        }


        private void Start()
        {
            _cameraShaker = instance.GetComponent<CameraShaker>();
            _cameraShaker.TargetTransform = _targetTransform.transform;
        }

        private void Update()
        {
            // track the target to the active transform
            // (it's possible that the active transform isn't static)
            if (!isTransitioning && !_cameraShaker.IsShaking)
            {
                _targetTransform.transform.SetPositionAndRotation(activeCam.transform.position,
                                                                  activeCam.transform.rotation);

            }
        }

        #endregion

        #region methods
        public void InstantSwitchToCam(Transform cam)
        {
            // instantaneous cam switch, no time delayed lerp

            // check camera is real
            if (cam != null)
            {
                previousCam = activeCam;
                activeCam = cam;

                _targetTransform.transform.SetPositionAndRotation(activeCam.transform.position,
                                                                  activeCam.transform.rotation); 

                // make sure manager state is 'not transitioning' 
                isTransitioning = false;

                OnTransitionEnded?.Invoke(activeCam);
            }
            else
            {
                print("transform to switch to doesn't exist");
            }
        }

        public void SwitchToCam(Transform cam)
        {
            TimedSwitchToCam(cam, _defaultTransitionTime);
        }

        public void TimedSwitchToCam(Transform cam, float duration)
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
                    transitionStartTransform = _targetTransform.transform;
                    OnTransitionEnded?.Invoke(_targetTransform.transform);
                    // don't modify previousCam--leave it
                }
                else
                {
                    previousCam = activeCam;

                    if (previousCam == null)
                    {
                        previousCam = startCam;
                    }

                    // transitionStartTransform = _targetTransform.transform;
                }

                isTransitioning = true;

                previousCam = activeCam;
                activeCam = cam;

                OnTransitionStarted?.Invoke(_targetTransform.transform);

                StartCoroutine(SwitchingCamRoutine(duration));
            }
            else
            {
                print("camera to switch to doesn't exist");
            }
        }

        private IEnumerator SwitchingCamRoutine(float duration)
        {
            Debug.Log($"{duration} second transition starting at {Time.time}");

            float timeStart = Time.time;
            float timeEnd = timeStart + duration;

            Vector3 startPos = _targetTransform.transform.position;
            Quaternion startRot = _targetTransform.transform.rotation;

            while (Time.time < timeEnd)
            {
                float t = 1f - (timeEnd - Time.time) / duration;

                // percentage of half a unit circle rotation
                float theta = (t + 0.005f) * Mathf.PI;

                // what's the amplitude of the sign curve? (varies 0 to 1)
                float percent = !_useEasing ? t : Mathf.Clamp(0.5f * (Mathf.Sin(theta - 0.5f * Mathf.PI) + 1f), 0f, 1f);

                _targetTransform.transform.position = Vector3.Lerp(startPos, activeCam.transform.position, percent);
                _targetTransform.transform.rotation = Quaternion.Lerp(startRot, activeCam.transform.rotation, percent);
                yield return null;
            }

            isTransitioning = false;

            OnTransitionEnded?.Invoke(activeCam);
        }

        public void TriggerCameraShake(float duration=0.35f)
        {
            _cameraShaker.TriggerCameraShake(duration);
        }

        #endregion

    }
}