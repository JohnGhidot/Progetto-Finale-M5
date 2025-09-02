using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private Transform _respawnPoint;

    [SerializeField] private bool _isExit = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isExit == false)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVictory();
            }
        }
    }

    public void Respawn()
    {
        GameObject playerObj = FindPlayerRoot();
        if (playerObj == null)
        {
            Debug.LogWarning("[GameManager] Respawn tentato, ma Player non trovato (v. tag).");
            return;
        }

        if (_respawnPoint == null)
        {
            Debug.LogWarning("[GameManager] RespawnPoint non assegnato in Inspector.");
            return;
        }

        NavMeshAgent agent = playerObj.GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh == true)
        {
            bool warped = agent.Warp(_respawnPoint.position);
            if (warped == false)
            {
                playerObj.transform.position = _respawnPoint.position;
            }

            playerObj.transform.rotation = _respawnPoint.rotation;
            agent.ResetPath();
        }
        else
        {
            playerObj.transform.SetPositionAndRotation(_respawnPoint.position, _respawnPoint.rotation);
        }
    }

    private GameObject FindPlayerRoot()
    {
        PlayerController pc = FindObjectOfType<PlayerController>();
        if (pc != null)
        {
            return pc.gameObject;
        }

        GameObject tagged = GameObject.FindWithTag("Player");
        if (tagged != null)
        {
            return tagged.transform.root.gameObject;
        }

        return null;

    }


}
