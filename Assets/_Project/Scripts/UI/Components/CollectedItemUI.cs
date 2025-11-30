using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WheelOfFortune.Data;

namespace WheelOfFortune.UI.Components
{
    public class CollectedItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;

        [Header("Animation Settings")]
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float slideInDelay = 0.05f;

        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            FindChildComponents();
        }

        private void FindChildComponents()
        {
            if (iconImage == null)
            {
                Transform iconTransform = transform.Find("Icon");
                if (iconTransform != null)
                {
                    iconImage = iconTransform.GetComponent<Image>();
                }
                else
                {
                    iconImage = GetComponentInChildren<Image>();
                }
            }

            if (amountText == null)
            {
                Transform amountTransform = transform.Find("Amount");
                if (amountTransform != null)
                {
                    amountText = amountTransform.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    amountText = GetComponentInChildren<TextMeshProUGUI>();
                }
            }

            if (iconImage == null)
            {
                Debug.LogError("[CollectedItemUI] Icon Image not found! Make sure there's a child named 'Icon' with Image component.");
            }

            if (amountText == null)
            {
                Debug.LogError("[CollectedItemUI] Amount Text not found! Make sure there's a child named 'Amount' with TextMeshProUGUI component.");
            }
        }

        public void Setup(Sprite icon, int amount)
        {
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null && icon == null)
            {
                Debug.LogWarning("[CollectedItemUI] Icon sprite is null!");
            }

            if (amountText != null)
            {
                amountText.text = amount.ToString();
            }
        }

        public void Setup(RewardData reward)
        {
            Setup(reward.Icon, reward.Amount);
        }

        public void PlaySlideInAnimation(int index)
        {
            if (canvasGroup == null) return;

            canvasGroup.alpha = 0f;
            float delay = index * slideInDelay;

            StartCoroutine(SlideInCoroutine(delay));
        }

        private System.Collections.IEnumerator SlideInCoroutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            Vector3 startPos = rectTransform.localPosition + Vector3.left * 100f;
            Vector3 targetPos = rectTransform.localPosition;

            rectTransform.localPosition = startPos;

            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideInDuration;

                t = Mathf.SmoothStep(0f, 1f, t);

                rectTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                canvasGroup.alpha = t;

                yield return null;
            }

            rectTransform.localPosition = targetPos;
            canvasGroup.alpha = 1f;
        }
    }
}
