using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator _snailAnimator;
    [SerializeField] private Animator _ballAnimator;

    [SerializeField] private RollingAttackController _rollAttackController;
    [SerializeField] private GameObject _snailPuppet; 
    [SerializeField] private GameObject _ballPuppet; 
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_rollAttackController.IsRolling)
        {
            var info = _snailAnimator.GetCurrentAnimatorStateInfo(0); 
            if (info.IsName("Rolling") && info.normalizedTime > 0.4f)
            {
                _snailPuppet.SetActive(false);
                _ballPuppet.SetActive(true);
            }
        }
        else
        {
            if (!_snailPuppet.activeInHierarchy)
            {
                _snailPuppet.SetActive(true);
                _ballPuppet.SetActive(false);
            }
        }
    }

    public void DoAnimationTrigger(string triggerName)
    {
        _snailAnimator.SetTrigger(triggerName);
    }
}
