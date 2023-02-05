using System;
using System.Collections;
using System.Collections.Generic;
using GGJ2022;
using GGJ2022.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAnimationManager))]
[RequireComponent(typeof(RelativeCharacterController))]
[RequireComponent(typeof(RollingAttackController))]
[RequireComponent(typeof(TargetingController))]
public class PlayerStateManager : MonoBehaviour
{
    public enum States
    {
        Disabled,
        Move, 
        Attack, 
        Die
    }

    private bool _isTargeting = false;
    public bool IsTargeting => _isTargeting;

    private RelativeCharacterController _movementController;
    private RollingAttackController _rollingAttackController;
    private PlayerAnimationManager _animationManager;
    private TargetingController _targetingController; 
    
    private States _activeState = States.Move;

    private void Awake()
    {
        _movementController = GetComponent<RelativeCharacterController>();
        _rollingAttackController = GetComponent<RollingAttackController>();
        _animationManager = GetComponent<PlayerAnimationManager>();
        _targetingController = GetComponent<TargetingController>(); 
        
        SetTargeting(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        _rollingAttackController.OnRollEnd += () =>
        {
            Debug.Log("roll ended");
            SetActiveState(States.Move);
            _animationManager.DoAnimationTrigger("ExitRoll");
        };
        
    }

    // Update is called once per frame
    void Update()
    {
      
    }

    void SetActiveState(States state)
    {
        if (_activeState == state) return; 
        
        _movementController.enabled = false;
        _rollingAttackController.enabled = false; 
        
        switch (state) 
        {
            case States.Attack:
                _rollingAttackController.enabled = true;
                
                if (_targetingController.DistanceToTarget() > 2f * _targetingController.TargetingRadius)
                {
                    SetTargeting(false);
                }

                var dir = _isTargeting ? _targetingController.DirectionToTarget() 
                    : _movementController.GetMoveDirectionFromInputVector(); 
                
                _rollingAttackController.TriggerRoll(dir, _isTargeting);
                
                _animationManager.DoAnimationTrigger("EnterRoll");
                SFXAudioEventDriver.StaticFireSFXEvent("SnailRoll");
                break; 
            case States.Move:
                _movementController.enabled = true; 
                break; 
            case States.Die:
                _animationManager.DoAnimationTrigger("Die");
                SetTargeting(false);
                break;
            case States.Disabled:
                SetTargeting(false);
                break; 
        }

        _activeState = state; 
    }

    public void ToggleTargeting(InputAction.CallbackContext context)
    {
        if (_activeState is States.Die or States.Disabled) return;
        
        if (context.started)
        {
            _isTargeting = true;
        } else if (context.canceled)
        {
            _isTargeting = false;
        }
        
        _targetingController.enabled = _isTargeting;
    }

    public void CycleTargeting(InputAction.CallbackContext context)
    {
        if (_activeState is States.Die or States.Disabled) return;
        if (!_isTargeting) return; 
        
        if (context.performed)
        {
            _targetingController.CycleTargets(); 
        }
    }
    
    void SetTargeting(bool flag)
    {
        _isTargeting = flag;
        _targetingController.enabled = flag;
    }

    public void DoAttack(InputAction.CallbackContext context)
    {
        if (_activeState is States.Die or States.Disabled) return; 
        
        if (context.performed && _activeState == States.Move)
        {
            SetActiveState(States.Attack);
        }
    }

    public void DoShrink()
    {
        transform.localScale /= LevelStateManager.Instance.ScaleIncrement;
    }
}
