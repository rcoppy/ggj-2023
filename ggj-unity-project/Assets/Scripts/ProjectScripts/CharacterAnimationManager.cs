using UnityEngine;
using UnityEngine.InputSystem; 
using System.Collections;

namespace GGJ2022
{
    [RequireComponent(typeof(RelativeCharacterController))]
    public class CharacterAnimationManager : MonoBehaviour
    {
        bool _isActionPlaying = false;
        string _currentAction = "";
        string _lastAction = ""; 

        Coroutine _coroutine; 

        bool _inputActionLock = false; 

        RelativeCharacterController _controller;

        [SerializeField]
        Animator _animator;

        private Rigidbody _rigidbody;

        public bool IsActionPlaying
        {
            get { return _isActionPlaying; }
        }

        public string CurrentAction
        {
            get { return _currentAction; }
        }

        public delegate void ActionStarted(string action);
        public ActionStarted OnActionStarted;

        public delegate void ActionEnded(string action);
        public ActionEnded OnActionEnded;

        public void DoAction(string action)
        {
            if (!_isActionPlaying && !_inputActionLock)
            {
                _lastAction = _currentAction; 
                _isActionPlaying = true;
                _currentAction = action; 
                _animator.SetTrigger(action);
                OnActionStarted?.Invoke(action);
                _inputActionLock = true;

                Debug.Log("input locked"); 
            }
        }

        // actions will triple fire if you don't invoke this
        public void LockAction(InputAction.CallbackContext context) {
            if (context.started)
            {
                _inputActionLock = true; 
            }

            if (context.canceled)
            {
                _inputActionLock = false; 
            }
        }

        IEnumerator LockFrameDelay() {
            Debug.Log("start lock delay"); 
            yield return null;

            _inputActionLock = true;
            Debug.Log("input locked"); 
        }

        // Use this for initialization
        void Awake()
        {
            
            _controller = GetComponent<RelativeCharacterController>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            OnActionStarted += HandleActionStarted;
            OnActionEnded += HandleActionEnded;

            _controller.OnJumpStarted.AddListener(HandleJumpStart);
            _controller.OnJumpEnded.AddListener(HandleJumpEnd);

            _controller.OnWalkStarted.AddListener(HandleWalkStart);
            _controller.OnWalkEnded.AddListener(HandleWalkEnd);

            Dialogue.DialogueManager.Instance.OnDialogueStarted += HandleDialogueStart;
            Dialogue.DialogueManager.Instance.OnDialogueEnded += HandleDialogueEnd;
        }

        private void OnDisable()
        {
            OnActionStarted -= HandleActionStarted;
            OnActionEnded -= HandleActionEnded;

            _controller.OnJumpStarted.RemoveListener(HandleJumpStart);
            _controller.OnJumpEnded.RemoveListener(HandleJumpEnd);

            _controller.OnWalkStarted.RemoveListener(HandleWalkStart);
            _controller.OnWalkEnded.RemoveListener(HandleWalkEnd);

            Dialogue.DialogueManager.Instance.OnDialogueStarted -= HandleDialogueStart;
            Dialogue.DialogueManager.Instance.OnDialogueEnded -= HandleDialogueEnd;
        }

        void HandlePlayerDeath()
        {
            TransformCamManager.instance.TriggerCameraShake(0.3f);
        }

        void  HandleWalkStart()
        {
            _animator.SetBool("IsWalking", true);
        }

        void HandleWalkEnd()
        {
            _animator.SetBool("IsWalking", false);
        }

        void HandleJumpStart()
        {
            _animator.SetTrigger("Jump");
        }

        void HandleJumpEnd()
        {
            _animator.SetTrigger("EndJump");
        }

        void HandleDialogueStart(Dialogue.Schema.Cutscene cutscene)
        {
            _controller.SetIsInputEnabled(false);
        }

        void HandleDialogueEnd()
        {
            _controller.SetIsInputEnabled(true);
        }


        bool AnimatorIsPlaying()
        {
            return _animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1;
        }

        // https://answers.unity.com/questions/362629/how-can-i-check-if-an-animation-is-being-played-or.html
        bool AnimatorIsPlaying(string stateName)
        {
            return AnimatorIsPlaying() && _animator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
        }

        // Update is called once per frame
        void Update()
        {
            var elapsedTime = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f;
            var isFinished = (elapsedTime > 0.92f);

            if (_isActionPlaying && isFinished)
            {
                _isActionPlaying = false;

                // _inputActionLock = false;
                Debug.Log(elapsedTime);
                Debug.Log("released input lock");

                OnActionEnded?.Invoke(_currentAction);
            }
            
            // sprite flipping
            var child = _animator.transform;
            float raw = Mathf.Abs(child.localScale.x);

            var reference = TransformCamManager.instance.TargetCamera.transform.right;
            float dot = Vector3.Dot(reference, _controller.GetIntendedSpatialDirection());
            float sign = Mathf.Sign(dot);

            if (Mathf.Sign(child.localScale.x) != sign && Mathf.Abs(dot) > 0.075f)
            {
                child.localScale = new Vector3(sign * raw, child.localScale.y, child.localScale.z);
            }
        }

        void HandleActionStarted(string action)
        {
            _controller.SetIsInputEnabled(false);
        }

        void HandleActionEnded(string action)
        {
            _controller.SetIsInputEnabled(true);
        }
    }
}