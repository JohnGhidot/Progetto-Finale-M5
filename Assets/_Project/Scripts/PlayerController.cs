using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;

    [SerializeField] private LayerMask _raycastMask = ~0;
    [SerializeField] private float _maxRaycastDistance = 10f;
    [SerializeField] private int _navAreaMask = NavMesh.AllAreas;

    private Camera _mainCamera;

    private void Start()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        if (_agent != null && !_agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("PlayerController: nessuna NavMesh vicina alla posizione iniziale.");
            }
        }

        _navAreaMask = NavMesh.AllAreas;
    }

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (_agent == null)
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        if (_agent == null)
        {
            Debug.LogError("PlayerController richiede un NavMeshAgent!");
            enabled = false;
        }
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetNavPointFromMouse(out Vector3 target))
            {
                _agent.isStopped = false;
                _agent.SetDestination(target);
            }
        }
    }

    private bool TryGetNavPointFromMouse(out Vector3 navPoint)
    {
        navPoint = default;

        if (_mainCamera == null)
        {
            return false;
        }

        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, _raycastMask))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, _maxRaycastDistance, _navAreaMask))
            {
                navPoint = navHit.position;
                return true;
            }
        }

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 planePoint = ray.GetPoint(enter);
            if (NavMesh.SamplePosition(planePoint, out NavMeshHit navHit, _maxRaycastDistance, _navAreaMask))
            {
                navPoint = navHit.position;
                return true;
            }
        }

        return false;
    }

}
