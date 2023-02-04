using System;
using UnityEngine;
using UnityEngine.XR;
using GGJ2022.Audio;

namespace GGJ2022.EnemyAI
{
    public class EnemyAnimationManager : MonoBehaviour
    {
        private EnemyState _enemyState;

        [SerializeField] private EnemyProps _props; 

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
            SFXAudioEventDriver.Instance.FireSFXEvent(_props.DamagedSound);
        }

        private void HandleStartedAttack(EnemyState.States state)
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
        
        private void HandleStoppedAttack()
        {
            // throw new NotImplementedException();
        }
        
        private void HandleSawPlayer()
        {
            // throw new NotImplementedException();
        }
        
        private void HandleStartFleeing()
        {
            SFXAudioEventDriver.Instance.FireSFXEvent(_props.FleeingSound);
        }
        
        private void HandleStartedMoving()
        {
            // throw new NotImplementedException();
        }
        
        private void HandleStoppedMoving()
        {
            // throw new NotImplementedException();
        }
        
        private void Awake()
        {
            _enemyState = GetComponent<EnemyState>();
        }

        void HandleDeath()
        {
            // throw new NotImplementedException();
        }
        
        void HandleDialogueStarted(Dialogue.Schema.Cutscene cutscene)
        {
            _enemyState.IsAwake = false; 
        }
        
        void HandleDialogueEnded()
        {
            _enemyState.IsAwake = true; 
        }
    }
}