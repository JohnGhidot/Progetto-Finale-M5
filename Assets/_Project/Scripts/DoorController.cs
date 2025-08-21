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

    private void Start()
    {
        _initialRotation = _doorToOpen.transform.rotation;
        _navMeshObstacle = _doorToOpen.GetComponent<NavMeshObstacle>();
    }

    private void OnMouseDown()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            Transform player = playerObject.transform;

            if (!_isMoving && Vector3.Distance(transform.position, player.position) <= _interactionDistance)
            {
                StartCoroutine(OpenAndCloseDoor());
            }
        }
        else
        {
            Debug.LogError("Oggetto Player non trovato. Assicurati che il tuo Player abbia il tag 'Player'.");
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
}