using System;
using UnityEngine;
using UnityEngine.XR;
using GGJ2022.Audio;
using UnityEngine.Events;

namespace GGJ2022.EnemyAI
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyAnimationManager : MonoBehaviour
    {

        [SerializeField] private UnityEvent OnDied;
        [SerializeField] private UnityEvent OnDamaged;
        [SerializeField] private UnityEvent OnStartedAttacking;
        [SerializeField] private UnityEvent OnStoppedAttacking;
        
        [SerializeField] private UnityEvent OnSawPlayer;
        [SerializeField] private UnityEvent OnStartedFleeing;
        [SerializeField] private UnityEvent OnStartedMoving;
        [SerializeField] private UnityEvent OnStoppedMoving;

        [SerializeField] private bool _trackPuppetToTarget = false; 
        [SerializeField] GameObject[] _puppets;
        [SerializeField] private float _puppetYRotOffset = 0f; 
        [SerializeField] private EnemyProps _props;

        private Rigidbody _rigidbody;
        private EnemyState _enemyState;
        
        
        
        // there is a child nested in this gameobject holding the actual visual character representation
        // i.e. 'the puppet'
        Animator _puppetAnimator; 
        
        

        void UpdatePuppet()
        {
            var vel = _trackPuppetToTarget ? _enemyState.AttackTarget.position - transform.position 
                : _rigidbody.velocity;
            
            vel.y = 0;

            if (vel.magnitude > 0.1f)
            {
                var target = Quaternion.AngleAxis(_puppetYRotOffset, transform.up) *
                             Quaternion.LookRotation(vel, transform.up);

                foreach (GameObject p in _puppets)
                {
                    p.transform.rotation = Quaternion.Slerp(p.transform.rotation, target, 0.08f);
                }
            }
        }

        private void FixedUpdate()
        {
            if (_enemyState.IsAwake)
            {
                UpdatePuppet();
            }
        }
        
        private void Awake()
        {
            _enemyState = GetComponent<EnemyState>();
            _rigidbody = GetComponent<Rigidbody>();
            _puppetAnimator = GetComponentInChildren<Animator>(); 
        }

        private void Start()
        {
            // Dialogue.DialogueManager.instance.OnDialogueStarted += HandleDialogueStarted;
            // Dialogue.DialogueManager.instance.OnDialogueEnded += HandleDialogueEnded;

            _enemyState.OnDied += HandleDeath;
            _enemyState.OnDamaged += HandleDamaged;
            _enemyState.OnStartedAttacking += HandleStartedAttack;
            _enemyState.OnStoppedAttacking += HandleStoppedAttack; 
            _enemyState.OnSawPlayer += HandleSawPlayer;
            _enemyState.OnStartedFleeing += HandleStartFleeing;
            _enemyState.OnStartedMoving += HandleStartedMoving;
            _enemyState.OnStoppedMoving += HandleStoppedMoving; 
        }
        
        private void OnDisable()
        {
            // Dialogue.DialogueManager.instance.OnDialogueStarted -= HandleDialogueStarted;
            // Dialogue.DialogueManager.instance.OnDialogueEnded -= HandleDialogueEnded;

            _enemyState.OnDied -= HandleDeath;
            _enemyState.OnDamaged -= HandleDamaged;
            _enemyState.OnStartedAttacking -= HandleStartedAttack;
            _enemyState.OnStoppedAttacking -= HandleStoppedAttack; 
            _enemyState.OnSawPlayer -= HandleSawPlayer;
            _enemyState.OnStartedFleeing -= HandleStartFleeing;
            _enemyState.OnStartedMoving -= HandleStartedMoving;
            _enemyState.OnStoppedMoving -= HandleStoppedMoving; 
        }

        private void HandleDamaged()
        {
            // SFXAudioEventDriver.Instance.FireSFXEvent(_props.DamagedSound);
            
            OnDamaged?.Invoke();
        }

        private void HandleStartedAttack(EnemyState.States state)
        {
            /*try
            {
                if (state == EnemyState.States.DoingMelee)
                {
                    SFXAudioEventDriver.Instance.FireSFXEvent(_props.DoMeleeAttackSound);
                }
                else
                {
                    SFXAudioEventDriver.Instance.FireSFXEvent(_props.DoRangedAttackSound);
                }
            }
            catch
            {
                Debug.LogError("not all sounds implemented");
            }*/

            OnStartedAttacking?.Invoke();
        }
        
        private void HandleStoppedAttack()
        {
            OnStoppedAttacking?.Invoke();
        }
        
        private void HandleSawPlayer()
        {
            OnSawPlayer?.Invoke();
        }
        
        private void HandleStartFleeing()
        {
            // SFXAudioEventDriver.Instance.FireSFXEvent(_props.FleeingSound);
            OnStartedFleeing?.Invoke();
        }
        
        private void HandleStartedMoving()
        {
            OnStartedMoving?.Invoke();
        }
        
        private void HandleStoppedMoving()
        {
            OnStoppedMoving?.Invoke();
        }

        void HandleDeath()
        {
            OnDied?.Invoke();
        }
        
        void HandleDialogueStarted(Dialogue.Schema.Cutscene cutscene)
        {
            _enemyState.IsAwake = false; 
        }
        
        void HandleDialogueEnded()
        {
            _enemyState.IsAwake = true; 
        }
        
        public void DoAnimationTrigger(string triggerName)
        {
            /*var info = _puppetAnimator.GetCurrentAnimatorStateInfo(0);

            // overrides for edge cases
            if (triggerName == "WalkEnd" && !info.IsName("Walk"))
            {
                return;
            }
        
            if (triggerName == "ExitRoll" && !info.IsName("Rolling"))
            {
                return;
            }

            if (triggerName == "WalkStart")
            {
                _puppetAnimator.ResetTrigger("ExitRoll");
            }*/
        
            // Debug.Log("triggering " + triggerName);
            _puppetAnimator.SetTrigger(triggerName);
        }
    }
}