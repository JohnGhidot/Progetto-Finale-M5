using UnityEngine;
using System.Collections;
using UnityEngine.AI;

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

    private void Start()
    {
        _initialRotation = _doorToOpen.transform.rotation;
        _navMeshObstacle = _doorToOpen.GetComponent<NavMeshObstacle>();
    }

    private void Update()
    {
        if (_player == null)
        {
            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                _player = playerObject.transform;
            }
            else
            {
                return;
            }
        }

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        bool inRange = distanceToPlayer <= _interactionDistance;

        if (!_isMoving && inRange)
        {
            if (!_ownsPrompt)
            {
                UIManager.Instance?.ShowInteractionPrompt(true, "Press \"E\" to open");
                _ownsPrompt = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(OpenAndCloseDoor());
            }
        }
        else
        {

            if (_ownsPrompt)
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
        Quaternion targetRotation = Quaternion.Euler(_initialRotation.eulerAngles.x, _initialRotation.eulerAngles.y + _openAngle, _initialRotation.eulerAngles.z);

        while (Quaternion.Angle(_doorToOpen.transform.rotation, targetRotation) > 0.01f)
        {
            _doorToOpen.transform.rotation = Quaternion.RotateTowards(_doorToOpen.transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            yield return null;
        }

        _doorToOpen.transform.rotation = targetRotation;
        yield return new WaitForSeconds(3f);
        targetRotation = _initialRotation;

        while (Quaternion.Angle(_doorToOpen.transform.rotation, targetRotation) > 0.01f)
        {
            _doorToOpen.transform.rotation = Quaternion.RotateTowards(_doorToOpen.transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            yield return null;
        }

        _doorToOpen.transform.rotation = targetRotation;
        if (_navMeshObstacle != null)
        {
            _navMeshObstacle.enabled = true;
        }
        _isMoving = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _interactionDistance);
    }

}