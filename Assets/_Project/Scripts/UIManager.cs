using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TMP_Text _interactionText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_interactionText != null)
        {
            _interactionText.gameObject.SetActive(false);
        }
    }

    public void ShowInteractionPrompt(bool show, string message = null)
    {
        if (_interactionText == null)
        {
            return;
        }

        if (show)

        {
            if (!string.IsNullOrEmpty(message))
            {
                _interactionText.text = message;
            }

            if (!_interactionText.gameObject.activeSelf)
            {
                _interactionText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (_interactionText.gameObject.activeSelf)
            {
                _interactionText.gameObject.SetActive(false);
            }
        }
        
    }
}
