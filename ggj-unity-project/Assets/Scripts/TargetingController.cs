using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GGJ2022;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using UnityEngine.UI;
using UnityFx.Outline;

[RequireComponent(typeof(RelativeCharacterController))]
public class TargetingController : MonoBehaviour
{
    private Queue<Collider> _targetsQueue;
    private GameObject _activeTarget = null;
    public GameObject ActiveTarget => _activeTarget; 

    [SerializeField] private float _targetingRadius = 10f;
    public float TargetingRadius => _targetingRadius; 
    
    [SerializeField] private LayerMask _excludedLayer;

    [SerializeField] private OutlineLayerCollection _layerCollection; 
    
    private OutlineLayer _targetableLayer;
    private OutlineLayer _activeLayer; 

    private RelativeCharacterController _characterController; 
    
    // Start is called before the first frame update
    void Awake()
    {
        _characterController = GetComponent<RelativeCharacterController>();

        _targetableLayer = _layerCollection.GetOrAddLayer(0);
        _activeLayer = _layerCollection.GetOrAddLayer(1);
    }

    float DotWithPlayer(Collider c, Vector3 dir)
    {
        var offset = (c.transform.position - transform.position).normalized;

        return Vector3.Dot(dir, offset); 
    }

    int CompareTargets(Collider a, Collider b, Vector3 dir)
    {
        float aa = DotWithPlayer(a, dir);
        float bb = DotWithPlayer(b, dir);

        if (bb < aa) return -1;
        if (bb == aa) return 0;
        return 1; 
    }

    private void OnEnable()
    {
        var validTargets = Physics.OverlapSphere(transform.position, _targetingRadius, ~_excludedLayer)
            .Where(c => c.CompareTag("Targetable")).ToList();

        if (validTargets.Count < 1) return; 

        var direction = _characterController.LastNonzeroMoveVector;
        validTargets.Sort((a,b) => CompareTargets(a, b, direction));
        
        foreach (var c in validTargets)
        {
            _targetableLayer.Add(c.gameObject);
        }

        var target = validTargets.First().gameObject;
        _targetableLayer.Remove(target);
        _activeLayer.Add(target);
        _activeTarget = target;

        _targetsQueue = new Queue<Collider>(validTargets); 
    }

    private void OnDisable()
    {
        _targetableLayer.Clear();
        _activeLayer.Clear();
    }

    public void CycleTargets()
    {
        if (_targetsQueue == null || _targetsQueue.Count < 1) return; 
        
        var old = _targetsQueue.Dequeue(); 
        _targetableLayer.Add(old.gameObject);
        _targetsQueue.Enqueue(old);

        var newTarget = _targetsQueue.Peek().gameObject; 
        _activeLayer.Clear();
        _activeLayer.Add(newTarget);
        _activeTarget = newTarget; 
    }

    public Vector3 DirectionToTarget()
    {
        if (_activeTarget == null || !isActiveAndEnabled) return Vector3.zero;
        return (_activeTarget.transform.position - transform.position).normalized; 
    }

    public float DistanceToTarget()
    {
        if (_activeTarget == null || !isActiveAndEnabled) return Mathf.Infinity;
        return (_activeTarget.transform.position - transform.position).magnitude; 
    }
    
    // Update is called once per frame
    void Update()
    {
        if (_activeTarget == null || !_activeTarget.activeInHierarchy)
        {
            CycleTargets();
        }
    }
}
