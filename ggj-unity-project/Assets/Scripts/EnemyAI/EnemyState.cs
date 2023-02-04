using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

namespace GGJ2022.EnemyAI
{
    public class EnemyState : MonoBehaviour
    {
        public enum States
        {
            Idle,
            DoingMelee,
            DoingRanged,
            Patrolling,
            Seeking,
            Fleeing,
            Dead
        }

        [SerializeField] private float _maxAggro = 100f;

        [SerializeField] private float _aggroCoolDownRate = 15f;

        [SerializeField] private float _aggroLevel = 0f;

        public float AggroLevel => _aggroLevel;

        [SerializeField] private float _sightRadius = 5f; // meters
        public float SightRadius => _sightRadius;

        [SerializeField] private float _attackRangeRadius = 3f;

        [SerializeField] private bool _canFly = false;
        public bool CanFly => _canFly;

        [SerializeField] private bool _shouldFlee = true;

        private States _state = States.Idle;
        public States State => _state;

        [SerializeField] private bool _shouldPatrol = true;

        private bool _isPlayerInRange = false;

        private bool _isPlayerInSight = false;
        public bool IsPlayerInSight => _isPlayerInSight;

        private Vector3 _destinationPosition;

        [SerializeField] private Transform _attackTarget;

        private bool _isPlayerObstructed = false;

        [SerializeField] private bool _isRangedAttackEnabled = true;
        [SerializeField] private bool _isMeleeAttackEnabled = true;

        public bool HasRanged => _isRangedAttackEnabled;
        public bool HasMelee => _isMeleeAttackEnabled;

        [SerializeField] private float _maxWalkSpeed = 3f;
        [SerializeField] private float _maxRunSpeed = 5f;

        [SerializeField] private float _healthFleeingThreshold = 1f;

        [SerializeField] private int _meleeDamage = 1;
        public int MeleeDamage => _meleeDamage;
        
        [SerializeField] private int _rangedDamage = 1;
        public int RangedDamage => _rangedDamage;

        [SerializeField] private int _maxHealth = 5;
        public int MaxHealth => _maxHealth;

        private int _health;
        public int Health => _health;

        private bool _isAttackInProgress = false;

        [SerializeField] private float _deathTimeout = 2f; // seconds til destroy 
        private float _timeOfDeath;

        [SerializeField] private LayerMask _layersToExclude;
        private int _exclusionMask;

        private bool _isMoving = false;
        public bool IsMoving => _isMoving;

        private bool _canMove = true;

        private bool _isLineOfSightObstructed = false;

        private Rigidbody _attackTargetRigidbody;

        [SerializeField] private GameObject _projectile;

        [SerializeField] private float _attackCooldownTime = 0.8f;
        private float _lastAttackTime;

        public bool IsAwake = true; 
        
        // components
        private Collider _collider;
        private Rigidbody _rigidbody;



        // TODO: scriptable object that contains animation, weapon object references
        private ScriptableObject _data;

        public delegate void Died();

        public delegate void DeathTimedOut();

        public delegate void StartedMoving();

        public delegate void StoppedMoving();

        public delegate void SawPlayer();

        public delegate void LostSightOfPlayer();

        public delegate void StartedFleeing();

        public delegate void ReachedPlayer();

        public delegate void StartedAttacking(States state);

        public delegate void StoppedAttacking();

        public delegate void Damaged();

        public Died OnDied;
        public Damaged OnDamaged;
        public StartedMoving OnStartedMoving;
        public StoppedMoving OnStoppedMoving;
        public SawPlayer OnSawPlayer;
        public LostSightOfPlayer OnLostSightOfPlayer;
        public StartedFleeing OnStartedFleeing;
        public ReachedPlayer OnReachedPlayer;
        public StartedAttacking OnStartedAttacking;
        public StoppedAttacking OnStoppedAttacking;
        public DeathTimedOut OnDeathTimedOut;

        void ProcessState()
        {
            if (!IsAwake) return; 
            
            Vector3? attempt;
            switch (_state)
            {
                case States.Dead:

                    _canMove = false;

                    if (Time.time - _timeOfDeath > _deathTimeout)
                    {
                        OnDeathTimedOut?.Invoke();
                        Destroy(gameObject);
                    }

                    break;

                case States.Fleeing:
                    bool flag = CheckIsPlayerInSightRange();

                    if (flag && (transform.position == _destinationPosition || !_isMoving))
                    {
                        attempt = ChooseFleeingTarget();

                        if (attempt == null)
                        {
                            TriggerStateChange(States.Idle);
                            break;
                        }

                        _destinationPosition = (Vector3) attempt;
                    }
                    else if (!flag)
                    {
                        TriggerStateChange(States.Idle);
                    }

                    break;

                case States.Idle:

                    if (CheckIsPlayerInSightRange())
                    {
                        TriggerStateChange(States.Seeking);
                    }
                    else if (_shouldPatrol)
                    {
                        TriggerStateChange(States.Patrolling);
                    }

                    break;

                case States.Patrolling:
                    if (CheckIsPlayerInSightRange())
                    {
                        TriggerStateChange(States.Seeking);
                    }
                    else if (transform.position == _destinationPosition || !_isMoving)
                    {
                        attempt = ChoosePatrolPoint();

                        if (attempt == null)
                        {
                            TriggerStateChange(States.Idle);
                            break;
                        }

                        _destinationPosition = (Vector3) attempt;
                    }

                    break;

                case States.Seeking:
                    if (_shouldFlee && _health <= _healthFleeingThreshold)
                    {
                        TriggerStateChange(States.Fleeing);
                        break;
                    }

                    if (CheckIsPlayerInSightRange())
                    {

                        if (_isRangedAttackEnabled)
                        {
                            Vector3? result = ChooseRangedPosition();

                            if (result == null)
                            {
                                TriggerStateChange(_isMeleeAttackEnabled ? States.Seeking : States.Idle);
                            }
                            else if (!_isMoving)
                            {
                                _destinationPosition = (Vector3) result;
                            }
                        }
                        else if (CheckIsSightlineObstructed())
                        {
                            attempt = ChooseNavigationTarget();

                            if (attempt == null)
                            {
                                TriggerStateChange(States.Idle);
                                break;
                            }

                            _destinationPosition = (Vector3) attempt;
                        }
                        else
                        {
                            _destinationPosition = _attackTarget.position;
                        }
                    }
                    else
                    {
                        TriggerStateChange(States.Idle);
                    }

                    break;

                case States.DoingMelee:
                case States.DoingRanged:
                    if (_isAttackInProgress)
                    {
                        return;
                    }

                    if (_state == States.DoingRanged)
                    {
                        if (CheckIsPlayerInRangedAttackRadius() && !_isAttackInProgress)
                        {
                            _canMove = false;
                            _isAttackInProgress = true;
                            _lastAttackTime = Time.time; 
                            DoRanged();
                            OnStartedAttacking?.Invoke(_state);
                        }
                        else
                        {
                            _canMove = true;
                            _isAttackInProgress = false;
                            TriggerStateChange(States.Seeking);
                        }
                    }
                    else if (_state == States.DoingMelee && !_isMoving
                                                         && GetDistanceToPlayer() < _collider.bounds.size.magnitude
                                                         && !_isAttackInProgress)
                    {
                        _canMove = false;
                        _isAttackInProgress = true;
                        _lastAttackTime = Time.time;
                        DoMelee();
                        OnStartedAttacking?.Invoke(_state);
                    }
                    else
                    {
                        _canMove = true;
                        TriggerStateChange(States.Seeking);
                    }

                    break;
            }
        }


        bool GetIsPositionOccupied(Vector3 position)
        {
            return Physics.OverlapBox(position + _collider.bounds.center, _collider.bounds.extents, Quaternion.identity,
                    _exclusionMask)
                .Length < 1;
        }

        Vector3 NudgeTarget(Vector3 position)
        {
            float rotationInterval = 30f; // degrees
            float intervals = Random.Range(0f, 360f / rotationInterval - 1f);

            float rotationAmount = intervals * rotationInterval;

            Quaternion rotation = Quaternion.Euler(0, rotationAmount, 0);

            float nudgeDistance = 1f; // meters
            Vector3 relativeNudge = nudgeDistance * (rotation * Vector3.forward);

            return position + relativeNudge;
        }

        Vector3? GetValidTarget(Vector3 position)
        {
            // max 12 nduges
            for (int i = 0; i < 12; i++)
            {
                if (GetIsPositionOccupied(position))
                {
                    position = NudgeTarget(position);
                }
                else
                {
                    return position;
                }
            }

            return null;
        }

        Vector3? ChooseRangedPosition()
        {
            // find a place to stand from where we can fire
            // assumes that we're already in range

            return GetValidTarget(transform.position);
        }

        Vector3? ChooseNavigationTarget()
        {
            // pick a point to circumvent player obstruction 

            float rayDistance = 0.5f * GetDistanceToPlayer();

            float sweepAngle = 120f;
            float sweepOffset = 90f - 0.5f * sweepAngle;

            Vector3 rightVector = Vector3.Cross(Vector3.up, _attackTarget.position - transform.position).normalized;

            for (int i = 0; i < 10; i++)
            {
                float eulerDirection = sweepOffset + Random.value * sweepAngle;
                Vector3 castVector = Quaternion.Euler(0, eulerDirection, 0) * (rayDistance * rightVector);

                if (!Physics.Raycast(_collider.bounds.center, castVector, rayDistance))
                {
                    return transform.position + castVector;
                }
            }

            return null;
        }

        Vector3? ChooseFleeingTarget()
        {
            // todo: running away from player
            return GetValidTarget(transform.position + 0.5f * (transform.position - _attackTarget.position));
        }

        bool CheckIsPlayerInSightRange()
        {
            _isPlayerInSight = GetDistanceToPlayer() < _sightRadius;
            return _isPlayerInSight;
        }

        bool CheckIsSightlineObstructed()
        {
            Vector3 castVector = _attackTarget.position - transform.position;

            RaycastHit hit;
            if (Physics.Raycast(_collider.bounds.center, castVector, out hit, castVector.magnitude))
            {
                _isLineOfSightObstructed = hit.collider.transform != _attackTarget;
            }

            _isLineOfSightObstructed = false; 
            return _isLineOfSightObstructed;
        }

        Vector3? ChoosePatrolPoint()
        {
            float range = 360f;
            var rotation = Quaternion.Euler(0, Random.Range(0f, range), 0);

            float dist = Random.Range(1f, 4f); // meters

            var vector = rotation * (dist * transform.forward);

            return GetValidTarget(transform.position + vector);
        }

        void TriggerStateChange(States state)
        {
            if (!IsAwake) return;
            if (_state == state)
            {
                return;
            }

            if ((_state is (States.DoingMelee or States.DoingRanged)) &&
                (state is not (States.DoingMelee or States.DoingRanged)))
            {
                _isAttackInProgress = false;
                OnStoppedAttacking?.Invoke();
            }

            _state = state;

            Vector3? attempt;

            switch (state)
            {
                case States.Dead:
                    _timeOfDeath = Time.time;
                    OnDied?.Invoke();
                    break;

                case States.Fleeing:

                    attempt = ChooseFleeingTarget();

                    if (attempt == null)
                    {
                        TriggerStateChange(States.Idle);
                        break;
                    }

                    _destinationPosition = (Vector3) attempt;
                    OnStartedFleeing?.Invoke();
                    break;

                case States.Idle:
                    _destinationPosition = transform.position; 
                    break;

                case States.Patrolling:
                    attempt = ChoosePatrolPoint();

                    if (attempt == null)
                    {
                        TriggerStateChange(States.Idle);
                        break;
                    }

                    _destinationPosition = (Vector3) attempt;
                    break;

                case States.Seeking:
                    break;

                case States.DoingMelee:
                case States.DoingRanged:
                    // if (_isAttackInProgress)
                    // {
                    //     break;
                    // }
                    //
                    // _isAttackInProgress = true;
                    // OnStartedAttacking(_state);
                    break;
            }
        }

        void HandleOnSawPlayer()
        {
            TriggerStateChange(States.Seeking);
        }

        bool CheckIsPlayerInRangedAttackRadius()
        {
            if (!_isRangedAttackEnabled)
            {
                return false;
            }

            if (GetDistanceToPlayer() < 1.3f * _attackRangeRadius)
            {
                return true;
            }

            return false;
        }

        float GetDistanceToPlayer()
        {
            return (_attackTarget.position - transform.position).magnitude;
        }

        void HandleOnReachedPlayer()
        {
            TriggerStateChange(States.DoingMelee);
        }

        void ProcessStatsUpdates()
        {
            if (!IsAwake) return; 
            
            _aggroLevel -= _aggroCoolDownRate * Time.deltaTime;
            _aggroLevel = Mathf.Clamp(_aggroLevel, 0f, _maxAggro);

            if (Time.time > _lastAttackTime + _attackCooldownTime && _isAttackInProgress)
            {
                _isAttackInProgress = false; 
            }
        }

        // Use this for initialization
        void Start()
        {
            _health = _maxHealth;
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();
            _exclusionMask = ~_layersToExclude;

            _destinationPosition = transform.position;
            _lastAttackTime = Time.time; 

            if (_attackTarget == null)
            {
                _attackTarget = LevelStateManager.Instance.PlayerObject.transform; 
            }
            
            _attackTargetRigidbody = _attackTarget.GetComponent<Rigidbody>();

            
        }

        // Update is called once per frame
        void Update()
        {
            ProcessStatsUpdates();
            
            ProcessState();
        }

        private void FixedUpdate()
        {
            if (!IsAwake) return; 
            
            var directionVector = _destinationPosition - transform.position;
            float distanceToDestination = directionVector.magnitude;

            float distanceBetweenColliders =
                (_attackTargetRigidbody.ClosestPointOnBounds(_rigidbody.worldCenterOfMass) -
                 _attackTargetRigidbody.worldCenterOfMass).magnitude
                + 1.2f * _collider.bounds.extents.magnitude; 
            
            if (_canMove && distanceToDestination > distanceBetweenColliders)
            {
                _isMoving = true;

                // take out vertical component 
                var moveDirection = (directionVector - Vector3.Dot(Vector3.up, directionVector) * Vector3.up)
                    .normalized;

                float speed = _maxWalkSpeed;

                if (_state == States.Fleeing || _aggroLevel > 0.35f * _maxAggro)
                {
                    speed = _maxRunSpeed;
                }

                float acceleration = 3f * speed;

                if (_rigidbody.velocity.magnitude < speed)
                {
                    _rigidbody.velocity += acceleration * Time.fixedDeltaTime * moveDirection;
                }
            }
            else
            {
                // apply lateral friction 
                var vertical = Vector3.Dot(Vector3.up, _rigidbody.velocity) * Vector3.up;
                var lateral = _rigidbody.velocity - vertical;

                _rigidbody.velocity = 0.85f * lateral + vertical;

                if (_rigidbody.velocity.magnitude < 0.05f)
                {
                    _isMoving = false; 
                }

                // if reached target and player is there, attack
                if (_isRangedAttackEnabled && CheckIsPlayerInRangedAttackRadius() && 
                    _state != States.DoingMelee && _state != States.DoingRanged)
                {
                    TriggerStateChange(States.DoingRanged);
                } else if (_isMeleeAttackEnabled && 
                           (_attackTargetRigidbody.worldCenterOfMass - _rigidbody.worldCenterOfMass).magnitude < _collider.bounds.size.magnitude 
                           && _state != States.DoingMelee)
                {
                    TriggerStateChange(States.DoingMelee);
                }
            }
        }

        public void DoDamage(int damage)
        {
            _health = Math.Max(0, _health - damage);

            if (_health <= 0)
            {
                TriggerStateChange(States.Dead);
            }

            _aggroLevel += 4f * damage; 
        }
        
        void DoMelee()
        {
            var center = _rigidbody.worldCenterOfMass; 
            var direction = (_attackTargetRigidbody.worldCenterOfMass - center).normalized;

            var extents = _collider.bounds.extents; 
            var attackCenter = 1.2f * extents.magnitude * direction + center;

            var colliders = Physics.OverlapBox(attackCenter, 1.4f * extents);

            foreach (var c in colliders)
            {
                var state = c.gameObject.GetComponent<PlayerState>();

                if (state == null) continue; 
                
                state.DoDamage(_meleeDamage);
                c.attachedRigidbody.AddForce(200f * (c.bounds.center - center).normalized);
                break;
            }
        }

        void DoRanged()
        {
            var center = _rigidbody.worldCenterOfMass; 
            var direction = (_attackTargetRigidbody.worldCenterOfMass - center).normalized; 
            var attackCenter = 0.4f * direction + center;
            var p = Instantiate(_projectile).GetComponent<ProjectileBehaviour>();
            
            p.transform.position = attackCenter;
            p.parent = gameObject;
            p.targetType = typeof(PlayerState); 
            
            // throw off the aim slightly to decrease difficulty 
            Quaternion error = Quaternion.Inverse(Quaternion.Euler(0f, Random.Range(-10f, 10f), 0f)) * Quaternion.identity;
            
            p.velocity = 15f * (error * direction); 
        }
    }
}
