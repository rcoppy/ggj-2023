using System.Collections;
using System.Collections.Generic;
using GGJ2022;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerAnimationManager))]
public class PlayerStateManager : MonoBehaviour
{
    public enum States
    {
        Disabled,
        Move, 
        Attack, 
        Die
    }

    [SerializeField] private bool _isTargeting = false;
    public bool IsTargeting => _isTargeting;

    [SerializeField] private RelativeCharacterController _movementController;
    [SerializeField] private RollingAttackController _rollingAttackController;

    [SerializeField] private PlayerAnimationManager _animationManager; 
    
    private States _activeState = States.Move; 
    
    // Start is called before the first frame update
    void Start()
    {
        _rollingAttackController.OnRollEnd += () =>
        {
            Debug.Log("roll ended");
            _animationManager.DoAnimationTrigger("ExitRoll"); 
            SetActiveState(States.Move);
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

                var dir = _movementController.GetMoveDirectionFromInputVector(); 
                _rollingAttackController.TriggerRoll(dir);
                _animationManager.DoAnimationTrigger("EnterRoll");
                break; 
            case States.Move:
                _movementController.enabled = true; 
                break; 
            case States.Die:
                _animationManager.DoAnimationTrigger("Die");
                break;
            case States.Disabled:
                break; 
        }

        _activeState = state; 
    }

    public void ToggleTargeting(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isTargeting = !_isTargeting; 
        }
    }

    public void DoAttack(InputAction.CallbackContext context)
    {
        if (context.performed && _activeState == States.Move)
        {
            SetActiveState(States.Attack);
        }
    }
}
