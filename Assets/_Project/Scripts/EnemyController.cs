using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

public class EnemyController : MonoBehaviour
{
    private enum EnemyKind { Sentry, Patroller }
    [SerializeField] private EnemyKind _enemyKind = EnemyKind.Sentry;

    private enum EnemyState { Idle, Patrol, Chase, Search, Return }
    [SerializeField] private EnemyState _enemyState = EnemyState.Idle;
    private EnemyState _state;

    [Header("References (Shared)")]
    [SerializeField] private Transform _player;
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private Transform _eyes;

    [Header("Vision")]
    [SerializeField] private float _viewDistance = 10f;
    [SerializeField] private float _viewAngle = 75f;

    [Header("Sentry Settings")]
    [SerializeField] private float _sentryIdleTime = 2f;
    [SerializeField] private float _idleTurnDegree = 45f;
    [SerializeField] private float _rotationSpeed = 3f;
    private float _idleTimer = 0f;
    private Quaternion _homeRotation;
    private Quaternion _targetRotation;
    private Vector3 _spawnPosition;

    [Header("Patroller Settings")]
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private float _patrolIdleTime = 0.5f;
    private int _patrolIndex = 0;
    private float _patrolTimer = 0f;

    [Header("Chase Settings (Shared)")]
    [SerializeField] private float _lostSightTime = 1f;
    [SerializeField] private float _searchTime = 3f;
    [SerializeField] private float _repathInterval = 0.5f;
    [SerializeField] private float _searchRotationSpeed = 3f;

    private float _lostSightTimer = 0f;
    private float _searchTimer = 0f;
    private float _repathTimer = 0f;

    private Vector3 _lastKnownPlayerPosition;
    private Quaternion _searchTargetRotation;

    [Header("Hit Settings (Shared)")]
    [Tooltip("Distanza a cui considerare il Player 'preso'.")]
    [SerializeField] private float _hitDistance = 0.3f;

    private void Awake()
    {
        _spawnPosition = transform.position;
        _homeRotation = transform.rotation;
        _targetRotation = _homeRotation;
        _state = _enemyState;

        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        if (_agent != null)
        {
            _agent.stoppingDistance = 0f;
        }

        if (_enemyKind == EnemyKind.Sentry)
        {
            _state = EnemyState.Idle;
        }
        else
        {
            if (_patrolPoints != null && _patrolPoints.Length > 0)
            {
                _state = EnemyState.Patrol;

                if (_agent != null)
                {
                    _agent.SetDestination(_patrolPoints[_patrolIndex].position);
                }
            }
            else
            {
                _state = EnemyState.Idle;
            }
        }
    }

    private void OnEnable()
    {
        FindPlayerIfNeeded();
    }

    private void Update()
    {
        FindPlayerIfNeeded();

        bool seePlayer = CanSeePlayer();

        if (seePlayer == true)
        {
            _lastKnownPlayerPosition = _player.position;
            _lostSightTimer = 0f;

            if (_state != EnemyState.Chase)
            {
                ChangeState(EnemyState.Chase);
            }
        }
        else
        {
            _lostSightTimer += Time.deltaTime;
        }

        switch (_state)
        {
            case EnemyState.Idle:
                {
                    StateIdle();
                    break;
                }
            case EnemyState.Patrol:
                {
                    StatePatrol();
                    break;
                }
            case EnemyState.Chase:
                {
                    StateChase();
                    break;
                }
            case EnemyState.Search:
                {
                    StateSearch();
                    break;
                }
            case EnemyState.Return:
                {
                    StateReturn();
                    break;
                }
        }
    }

    private void StateIdle()
    {
        if (_agent != null)
        {
            _agent.isStopped = true;
        }

        if (_enemyKind == EnemyKind.Sentry)
        {
            float t = 1f - Mathf.Exp(-_rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, t);

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= _sentryIdleTime && Quaternion.Angle(transform.rotation, _targetRotation) < 1f)
            {
                _idleTimer = 0f;
                Vector3 euler = _targetRotation.eulerAngles;
                euler.y += _idleTurnDegree;
                _targetRotation = Quaternion.Euler(euler);
            }
        }
    }

    private void StatePatrol()
    {
        if (_agent != null)
        {
            _agent.isStopped = false;
        }

        if (_patrolPoints == null || _patrolPoints.Length == 0)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (_agent.pathPending == false && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
        {
            _patrolTimer += Time.deltaTime;

            if (_patrolTimer >= _patrolIdleTime)
            {
                _patrolTimer = 0f;
                _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
                _agent.SetDestination(_patrolPoints[_patrolIndex].position);
            }
        }
    }

    private void StateChase()
    {
        if (_agent != null)
        {
            _agent.isStopped = false;
        }

        if (_player != null)
        {
            Vector3 a = transform.position;
            Vector3 b = _player.position;
            a.y = 0f;
            b.y = 0f;

            Vector3 diff = a - b;
            float sqr = diff.sqrMagnitude;
            float hitSqr = _hitDistance * _hitDistance;

            if (sqr <= hitSqr)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Respawn();
                }
                return;
            }
        }

        _repathTimer += Time.deltaTime;
        if (_repathTimer >= _repathInterval)
        {
            _repathTimer = 0f;
            _agent.SetDestination(_lastKnownPlayerPosition);
        }

        if (_lostSightTimer >= _lostSightTime)
        {
            ChangeState(EnemyState.Search);
        }
    }

    private void StateSearch()
    {
        if (_agent != null)
        {
            _agent.isStopped = false;
        }

        if (_agent.pathPending == false && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
        {
            _searchTimer += Time.deltaTime;

            float t = 1f - Mathf.Exp(-_searchRotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, _searchTargetRotation, t);

            if (Quaternion.Angle(transform.rotation, _searchTargetRotation) < 1f)
            {
                Vector3 euler = _searchTargetRotation.eulerAngles;
                euler.y += 90f;
                _searchTargetRotation = Quaternion.Euler(euler);
            }

            if (_searchTimer >= _searchTime)
            {
                _searchTimer = 0f;
                ChangeState(EnemyState.Return);
            }
        }
    }

    private void StateReturn()
    {
        if (_agent != null)
        {
            _agent.isStopped = false;
        }

        if (_enemyKind == EnemyKind.Sentry)
        {
            _agent.SetDestination(_spawnPosition);

            if (_agent.pathPending == false && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
            {
                float t = 1f - Mathf.Exp(-_rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, _homeRotation, t);

                if (Quaternion.Angle(transform.rotation, _homeRotation) <= 1f)
                {
                    ChangeState(EnemyState.Idle);
                }
            }
        }
        else
        {
            if (_patrolPoints != null && _patrolPoints.Length > 0)
            {
                int closest = 0;
                float best = float.MaxValue;

                for (int i = 0; i < _patrolPoints.Length; i++)
                {
                    float dist = Vector3.SqrMagnitude(transform.position - _patrolPoints[i].position);
                    if (dist < best)
                    {
                        best = dist;
                        closest = i;
                    }
                }

                _patrolIndex = closest;
                _agent.SetDestination(_patrolPoints[_patrolIndex].position);
                ChangeState(EnemyState.Patrol);
            }
            else
            {
                ChangeState(EnemyState.Idle);
            }
        }
    }

    private void ChangeState(EnemyState next)
    {
        _state = next;

        if (_state == EnemyState.Search)
        {
            _searchTargetRotation = transform.rotation;
        }

        if (_state != EnemyState.Search)
        {
            _searchTimer = 0f;
        }

        if (_state != EnemyState.Chase)
        {
            _repathTimer = 0f;
        }
    }

    private bool CanSeePlayer()
    {
        if (_player == null)
        {
            return false;
        }

        Transform eyes = (_eyes != null) ? _eyes : transform;

        Vector3 targetPos = _player.position + Vector3.up * 1.0f;
        Vector3 toPlayer = targetPos - eyes.position;

        float dist = toPlayer.magnitude;
        if (dist > _viewDistance)
        {
            return false;
        }

        Vector3 dirToPlayer = toPlayer.normalized;
        float angle = Vector3.Angle(eyes.forward, dirToPlayer);
        if (angle > _viewAngle * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(eyes.position, dirToPlayer, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform hitRoot = (hit.rigidbody != null) ? hit.rigidbody.transform : hit.transform.root;

            if (hitRoot == _player.root)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform root = (other.attachedRigidbody != null) ? other.attachedRigidbody.transform : other.transform.root;

        bool isPlayerByComponent = (root.GetComponent<PlayerController>() != null);
        bool isPlayerByTag = root.CompareTag("Player");

        if (isPlayerByComponent == true || isPlayerByTag == true)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Respawn();
            }
        }
    }

    private void FindPlayerIfNeeded()
    {
        if (_player != null)
        {
            if (_player.GetComponent<PlayerController>() == null && _player.root.GetComponent<PlayerController>() == null)
            {
                _player = null;
            }
            else
            {
                return;
            }
        }

        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            _player = pc.transform;
            return;
        }

        GameObject tagged = GameObject.FindWithTag("Player");
        if (tagged != null)
        {
            Transform root = tagged.transform.root;
            if (root.GetComponent<PlayerController>() != null)
            {
                _player = root;
                return;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform eyes = (_eyes != null) ? _eyes : transform;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyes.position, _viewDistance);

        Vector3 left = Quaternion.Euler(0f, -_viewAngle * 0.5f, 0f) * eyes.forward;
        Vector3 right = Quaternion.Euler(0f, _viewAngle * 0.5f, 0f) * eyes.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(eyes.position, eyes.position + left * _viewDistance);
        Gizmos.DrawLine(eyes.position, eyes.position + right * _viewDistance);
    }
}
