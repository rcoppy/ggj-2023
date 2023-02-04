using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator _snailAnimator;
    [SerializeField] private Animator _ballAnimator;

    [SerializeField] private RollingAttackController _rollAttackController;
    [SerializeField] private GameObject _snailPuppet; 
    [SerializeField] private GameObject _ballPuppet; 
    
    // Start is called before the first frame update
    void Awake()
    {
        _snailChildren = _snailPuppet.GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private SkinnedMeshRenderer[] _snailChildren;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_rollAttackController.IsRolling)
        {
            var info = _snailAnimator.GetCurrentAnimatorStateInfo(0); 
            if (info.IsName("Rolling") && info.normalizedTime > 0.4f)
            {
                SetAreEnabledSnailChildren(false);
                _ballPuppet.SetActive(true);
            }
        }
        else
        {
            if (!_snailChildren[0].enabled)
            {
                SetAreEnabledSnailChildren(true);
                _ballPuppet.SetActive(false);
            }
        }
    }

    void SetAreEnabledSnailChildren(bool flag)
    {
        foreach (var c in _snailChildren)
        {
            c.enabled = flag; 
        }
    }

    public void DoAnimationTrigger(string triggerName)
    {
        var info = _snailAnimator.GetCurrentAnimatorStateInfo(0);

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
            _snailAnimator.ResetTrigger("ExitRoll");
        }
        
        Debug.Log("triggering " + triggerName);
        _snailAnimator.SetTrigger(triggerName);
    }
}
