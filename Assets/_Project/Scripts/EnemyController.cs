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

    [Header ("Vision")]
    [SerializeField] private float _viewDistance = 10f;
    [SerializeField] private float _viewAngle = 90f;

    [Header("Sentry Settings")]
    [SerializeField] private float _sentryIdleTime = 2f;
    [SerializeField] private float _idleTurnDegree = 45f;
    private float _idleTimer = 0f;
    private Quaternion _homeRotation;
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

    private float _lostSightTimer = 0f;
    private float _searchTimer = 0f;
    private float _repathTimer = 0f;

    private Vector3 _lastKnownPlayerPosition;


    private void Awake()
    {
        _spawnPosition = transform.position;
        _homeRotation = transform.rotation;

        _state = _enemyState;

        if (_enemyKind == EnemyKind.Sentry)
        {
            _state = EnemyState.Idle;
        }
        else
        {
            if (_patrolPoints != null && _patrolPoints.Length > 0)
            {
                _state = EnemyState.Patrol;
                _agent.SetDestination(_patrolPoints[_patrolIndex].position);
            }
            else
            {
                _state = EnemyState.Idle;
            }
        }
    }

    private void Update()
    {
        bool seePlayer = CanSeePlayer();

        if (seePlayer == true)
        {
            _lastKnownPlayerPosition = _player.position;
            _lostSightTime = 0f;

            if (_state != EnemyState.Chase)
            {
                ChangeState(EnemyState.Chase);
            }
        }
        else
        {
            _lostSightTime += Time.deltaTime;
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
        _agent.isStopped = true;

        if (_enemyKind == EnemyKind.Sentry)
        {
            _idleTimer += Time.deltaTime;

            if (_idleTimer >= _sentryIdleTime)
            {
                _idleTimer = 0f;

                Vector3 euler = transform.eulerAngles;
                euler.y += _idleTurnDegree;
                Quaternion newRot = Quaternion.Euler(euler);
            }
        }
    }

    private void StatePatrol()
    {
        _agent.isStopped = false;

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
        _agent.isStopped = false;

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
        _agent.isStopped = false;

        if (_agent.pathPending == false && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
        {
            _searchTimer = Time.deltaTime;

            Vector3 euler = transform.eulerAngles;
            euler.y *= 90f * Time.deltaTime;
            transform.rotation = Quaternion.Euler(euler);

            if (_searchTimer >= _searchTime)
            {
                _searchTimer = 0f;
                ChangeState(EnemyState.Return);
            }
        }
    }

    private void StateReturn()
    {
        _agent.isStopped = false;

        if (_enemyKind == EnemyKind.Sentry)
        {
            _agent.SetDestination(_spawnPosition);

            if (_agent.pathPending == false && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _homeRotation, 360f * Time.deltaTime);
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

                for (int i = 0, i < _patrolPoints.Length; i * *)
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
        if (_player == null || _eyes == null)
        {
            return false;
        }

        Vector3 toPlayer = _player.position - _eyes.position;
        float dist = toPlayer.magnitude;
        if (dist > _viewDistance)
        {
            return false;
        }

        Vector3 dirToPlayer = toPlayer.normalized;
        float angle = Vector3.Angle(_eyes.forward, dirToPlayer);
        if (angle > _viewAngle * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(_eyes.position, dirToPlayer, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
        {
            Transform hitRoot = hit.transform.root;
            if (hitRoot != _player)
            {
                return false;
            }
        }

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.Respawn();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (_eyes == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_eyes.position, _viewDistance);

        Vector3 left = Quaternion.Euler(0f, -_viewAngle * 0.5f, 0f) * _eyes.forward;
        Vector3 right = Quaternion.Euler(0f, _viewAngle * 0.5f, 0f) * _eyes.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_eyes.position, _eyes.position + left * _viewDistance);
        Gizmos.DrawLine(_eyes.position, _eyes.position + right * _viewDistance);
    }
}
