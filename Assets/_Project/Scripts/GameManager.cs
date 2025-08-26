using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowVictory();
            }
        }
    }
}
