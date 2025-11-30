using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WheelOfFortune.UI.Components
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {

        [Header("Auto-Assigned Components")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private Image buttonImage;

        [Header("Settings")]
        [SerializeField] private bool enableDebugLogs = false;

        public Button Button => button;
        public TextMeshProUGUI ButtonText => buttonText;
        public Image ButtonImage => buttonImage;

        private void OnValidate()
        {
            AutoAssignComponents();
        }

        private void AutoAssignComponents()
        {
            // Button referansını otomatik ata
            if (button == null)
            {
                button = GetComponent<Button>();
                LogDebug("Button component auto-assigned");
            }

            // Text'i bul ve ata (naming convention'a göre)
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    LogDebug($"TextMeshProUGUI auto-assigned: {buttonText.gameObject.name}");
                }
            }

            // Image'i bul ve ata
            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
                if (buttonImage == null)
                {
                    // Child'larda ara
                    buttonImage = GetComponentInChildren<Image>();
                }

                if (buttonImage != null)
                {
                    LogDebug($"Image auto-assigned: {buttonImage.gameObject.name}");
                }
            }

#if UNITY_EDITOR
            // Editor'de dirty flag'i set et
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        public void SetText(string text)
        {
            if (buttonText != null)
            {
                buttonText.text = text;
            }
            else
            {
                Debug.LogWarning($"[UIButton] ButtonText is null on {gameObject.name}");
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }

        public void SetSprite(Sprite sprite)
        {
            if (buttonImage != null)
            {
                buttonImage.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"[UIButton] ButtonImage is null on {gameObject.name}");
            }
        }

        public void AddListener(UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        public void RemoveListener(UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        public void RemoveAllListeners()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[UIButton - {gameObject.name}] {message}");
            }
        }

    }
}
