using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ExitTrigger : MonoBehaviour
{
    [SerializeField] private float _proximityMargin = 0.25f;

    private Collider _col;
    private Transform _player;
    private bool _fired = false;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (_col == null)
        {
            Debug.LogError("[ExitTrigger] Nessun Collider trovato!");
        }
        else
        {
            if (_col.isTrigger == false)
            {
                _col.isTrigger = true;
            }
        }
    }

    private void Update()
    {
        if (_fired == true)
        {
            return;
        }

        if (_player == null)
        {
            ResolvePlayer();
        }

        if (_player == null || _col == null)
        {
            return;
        }

        Vector3 closest = _col.ClosestPoint(_player.position);
        float sqr = (closest - _player.position).sqrMagnitude;
        float marginSqr = _proximityMargin * _proximityMargin;

        if (sqr <= marginSqr)
        {
            FireVictory();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_fired == true)
        {
            return;
        }

        Transform root = (other.attachedRigidbody != null) ? other.attachedRigidbody.transform : other.transform.root;

        bool isPlayerByComponent = (root.GetComponent<PlayerController>() != null);
        bool isPlayerByTag = root.CompareTag("Player");

        if (isPlayerByComponent == true || isPlayerByTag == true)
        {
            FireVictory();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_fired == true)
        {
            return;
        }

        Transform root = (other.attachedRigidbody != null) ? other.attachedRigidbody.transform : other.transform.root;

        bool isPlayerByComponent = (root.GetComponent<PlayerController>() != null);
        bool isPlayerByTag = root.CompareTag("Player");

        if (isPlayerByComponent == true || isPlayerByTag == true)
        {
            FireVictory();
        }
    }

    private void FireVictory()
    {
        _fired = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory();
        }
    }

    private void ResolvePlayer()
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Collider c = GetComponent<Collider>();
        if (c is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (c is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
        }
    }
}
