using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class DoorController : MonoBehaviour
{
    [SerializeField] private GameObject _doorToOpen;
    [SerializeField] private float _interactionDistance = 2f;
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private float _openAngle = -90f;

    private Quaternion _initialRotation;
    private bool _isMoving = false;
    private NavMeshObstacle _navMeshObstacle;
    private Transform _player;
    private bool _ownsPrompt = false;

    [SerializeField] private NavMeshSurface _navMeshSurface;

    private void Awake()
    {
        _initialRotation = _doorToOpen.transform.rotation;
        _navMeshObstacle = _doorToOpen.GetComponent<NavMeshObstacle>();
    }

    private void Start()
    {
        if (_navMeshSurface == null)
        {
            GameObject navMesh = GameObject.Find("NavMesh");
            if (navMesh != null)
            {
                _navMeshSurface = navMesh.GetComponent<NavMeshSurface>();
            }

            if (_navMeshSurface == null)
            {
                _navMeshSurface = FindObjectOfType<NavMeshSurface>();
            }

            if (_navMeshSurface == null)
            {
                Debug.LogWarning("[DoorController] NavMeshSurface non assegnato in Inspector e non trovato in scena.");
            }
        }

        ResolvePlayerRoot();
    }

    private void Update()
    {
        if (_player == null)
        {
            ResolvePlayerRoot();
            if (_player == null)
            {
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        bool inRange = distanceToPlayer <= _interactionDistance;

        if (_isMoving == false && inRange == true)
        {
            if (_ownsPrompt == false)
            {
                UIManager.Instance?.ShowInteractionPrompt(true, "Press \"E\" to open");
                _ownsPrompt = true;
            }

            if (Input.GetKeyDown(KeyCode.E) == true)
            {
                StartCoroutine(OpenAndCloseDoor());
            }
        }
        else
        {
            if (_ownsPrompt == true)
            {
                UIManager.Instance?.ShowInteractionPrompt(false);
                _ownsPrompt = false;
            }
        }
    }

    private IEnumerator OpenAndCloseDoor()
    {
        _isMoving = true;

        if (_navMeshObstacle != null)
        {
            _navMeshObstacle.enabled = false;
        }

        if (_navMeshSurface != null)
        {
            _navMeshSurface.BuildNavMesh();
        }

        Quaternion targetRotation = Quaternion.Euler(
            _initialRotation.eulerAngles.x,
            _initialRotation.eulerAngles.y + _openAngle,
            _initialRotation.eulerAngles.z
        );

        while (Quaternion.Angle(_doorToOpen.transform.rotation, targetRotation) > 0.01f)
        {
            _doorToOpen.transform.rotation = Quaternion.RotateTowards(
                _doorToOpen.transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        _doorToOpen.transform.rotation = targetRotation;

        yield return new WaitForSeconds(3f);

        targetRotation = _initialRotation;

        while (Quaternion.Angle(_doorToOpen.transform.rotation, targetRotation) > 0.01f)
        {
            _doorToOpen.transform.rotation = Quaternion.RotateTowards(
                _doorToOpen.transform.rotation,
                targetRotation,
                _rotationSpeed * Time.deltaTime
            );
            yield return null;
        }

        _doorToOpen.transform.rotation = targetRotation;

        if (_navMeshObstacle != null)
        {
            _navMeshObstacle.enabled = true;
        }

        if (_navMeshSurface != null)
        {
            _navMeshSurface.BuildNavMesh();
        }

        _isMoving = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _interactionDistance);
    }

    private void ResolvePlayerRoot()
    {
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
}