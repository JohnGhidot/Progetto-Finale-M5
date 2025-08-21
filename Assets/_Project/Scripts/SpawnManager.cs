using Cinemachine;
using UnityEngine;
using UnityEngine.AI;

public class SpawnManager : MonoBehaviour
{

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;

    void Start()
    {
        if (_playerPrefab == null || _spawnPoint == null)
        {
            Debug.LogError("SpawnManager: PlayerPrefab o SpawnPoint non assegnati!");
            return;
        }

        GameObject playerInstance = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);

        NavMeshAgent _agent = playerInstance.GetComponent<NavMeshAgent>();
        if (_agent != null)
        {
            if (NavMesh.SamplePosition(_spawnPoint.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                _agent.Warp(hit.position);
            }
        }

        if (_virtualCamera == null)
        {
            _virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        if (_virtualCamera != null)
        {
            _virtualCamera.Follow = playerInstance.transform;
            _virtualCamera.LookAt = playerInstance.transform;
        }
        else
        {
            Debug.LogWarning("SpawnManager: nessuna CinemachineVirtualCamera trovata in scena.");
        }
    }
}
