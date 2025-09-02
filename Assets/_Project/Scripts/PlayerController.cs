using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;

    [SerializeField] private LayerMask _raycastMask = ~0;
    [SerializeField] private float _maxRaycastDistance = 1000f;
    [SerializeField] private int _navAreaMask = NavMesh.AllAreas;

    private void Awake()
    {
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

    private void Start()
    {
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
    }

    private void Update()
    {

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {

            //Debug.Log("Click ricevuto");
            if (TryGetNavPointFromMouse(out Vector3 target))
            {
                _agent.isStopped = false;
                _agent.SetDestination(target);
            }
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            UIManager.Instance.ShowVictory();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Respawn();
            }
        }

    }

    private bool TryGetNavPointFromMouse(out Vector3 navPoint)
    {
        navPoint = default;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Camera.main è null");
            return false;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _maxRaycastDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, _navAreaMask))
            {
                navPoint = navHit.position;
                return true;
            }
        }
        else
        {
            Debug.Log("Raycast non ha colpito nulla (passo al fallback piano)");
        }

        const float groundY = 0f;
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, groundY, 0f));
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 p = ray.GetPoint(enter);
            if (NavMesh.SamplePosition(p, out NavMeshHit navHit2, 2f, _navAreaMask))
            {
                navPoint = navHit2.position;
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (enemy != null)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Respawn();
            }
        }
    }


}
