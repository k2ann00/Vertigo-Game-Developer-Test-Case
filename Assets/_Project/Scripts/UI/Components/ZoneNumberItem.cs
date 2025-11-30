using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WheelOfFortune.UI.Components
{
    public class ZoneNumberItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI zoneText;
        [SerializeField] private Image background;

       
        private int zoneNumber;
        private bool isCurrent;
        private bool isSafe;
        private bool isSuper;
        private bool isCompleted;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            // Auto-find components if not assigned
            if (zoneText == null)
            {
                zoneText = GetComponentInChildren<TextMeshProUGUI>();
            }

            if (background == null)
            {
                background = GetComponent<Image>();
            }
        }

        public void Setup(int zoneNumber, bool isCurrent, bool isSafe, bool isCompleted, Color textColor)
        {
            this.zoneNumber = zoneNumber;
            this.isCurrent = isCurrent;
            this.isSafe = isSafe;
            this.isCompleted = isCompleted;

            UpdateDisplay(textColor);
        }

        public void Setup(int zoneNumber, bool isCurrent, bool isSafe, bool isSuper, bool isCompleted,
                         Color textColor, Color backgroundColor)
        {
            this.zoneNumber = zoneNumber;
            this.isCurrent = isCurrent;
            this.isSafe = isSafe;
            this.isSuper = isSuper;
            this.isCompleted = isCompleted;

            UpdateDisplay(textColor, backgroundColor);
        }

        private void UpdateDisplay(Color textColor, Color? backgroundColor = null)
        {
            // Zone number text
            if (zoneText != null)
            {
                zoneText.text = zoneNumber.ToString();
                zoneText.color = textColor;

                // Current zone formatting
                if (isCurrent)
                {
                    zoneText.fontStyle = FontStyles.Bold;
                    zoneText.fontSize = 32;
                }
                else
                {
                    zoneText.fontStyle = FontStyles.Normal;
                    zoneText.fontSize = 24;
                }

                // Completed zone style
                if (isCompleted)
                {
                    zoneText.alpha = 0.6f;
                }
            }

            // Background
            if (background != null)
            {
                if (backgroundColor.HasValue)
                {
                    background.color = backgroundColor.Value;
                }
                else
                {
                    // Default background logic
                    if (isCurrent)
                    {
                        background.color = new Color(1f, 0.9f, 0.3f, 0.4f); // Yellow highlight
                    }
                    else if (isSafe)
                    {
                        background.color = new Color(0.3f, 1f, 0.3f, 0.2f); // Green tint
                    }
                    else if (isSuper)
                    {
                        background.color = new Color(1f, 0.7f, 0.1f, 0.3f); // Golden tint
                    }
                    else
                    {
                        background.color = new Color(0.2f, 0.2f, 0.2f, 0.1f); // Dark tint
                    }
                }
            }

        }

        public void AnimateHighlight()
        {
            if (!isCurrent) return;

            // Scale pulse animation
            transform.DOScale(1.2f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => transform.DOScale(1f, 0.2f));

            // Glow effect (background fade in/out)
            if (background != null)
            {
                Sequence glowSequence = DOTween.Sequence();
                glowSequence.Append(background.DOFade(0.6f, 0.3f));
                glowSequence.Append(background.DOFade(0.4f, 0.3f));
                glowSequence.SetLoops(2);
            }
        }

        public void AnimateComplete()
        {
            if (zoneText != null)
            {
                zoneText.DOFade(0.6f, 0.5f);
            }

            if (background != null)
            {
                background.DOFade(0.1f, 0.5f);
            }
        }

        public int ZoneNumber => zoneNumber;
        public bool IsCurrent => isCurrent;
        public RectTransform RectTransform => rectTransform;

    }
}
