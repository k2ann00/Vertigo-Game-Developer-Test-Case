using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Components;

namespace WheelOfFortune.UI.Popups
{
    public class ResultPopup : UIPanel
    {

        [Header("Result Display")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private TextMeshProUGUI rewardAmountText;

        [Header("Buttons")]
        [SerializeField] private UIButton continueButton;
        [SerializeField] private UIButton collectButton;

        [Header("Background")]
        [SerializeField] private Image backgroundOverlay;

        private RewardData currentReward;
        private System.Action onContinueCallback;
        private System.Action onCollectCallback;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupButtons();
        }

        private void SetupButtons()
        {
            if (continueButton != null)
            {
                continueButton.AddListener(OnContinueClicked);
            }

            if (collectButton != null)
            {
                collectButton.AddListener(OnCollectClicked);
            }
        }

        private void CleanupButtons()
        {
            if (continueButton != null)
            {
                continueButton.RemoveListener(OnContinueClicked);
            }

            if (collectButton != null)
            {
                collectButton.RemoveListener(OnCollectClicked);
            }
        }

        public void ShowResult(RewardData reward, System.Action onContinue = null, System.Action onCollect = null)
        {
            currentReward = reward;
            onContinueCallback = onContinue;
            onCollectCallback = onCollect;

            UpdateDisplay();
            Show(animated: true);
        }

        private void UpdateDisplay()
        {
            if (currentReward == null)
                return;

            // Icon
            if (rewardIcon != null && currentReward.Icon != null)
            {
                rewardIcon.sprite = currentReward.Icon;
                rewardIcon.enabled = true;
            }
            else if (rewardIcon != null)
            {
                rewardIcon.enabled = false;
            }

            // Name
            if (rewardNameText != null)
            {
                rewardNameText.text = currentReward.RewardName;
            }

            // Amount
            if (rewardAmountText != null)
            {
                if (currentReward.Type == RewardType.Multiplier)
                {
                    rewardAmountText.text = $"x{currentReward.MultiplierValue}";
                }
                else if (currentReward.Amount > 0)
                {
                    rewardAmountText.text = $"+{currentReward.Amount}";
                }
                else
                {
                    rewardAmountText.text = "";
                }
            }

            // Button visibility
            bool isBomb = currentReward.IsBomb();
            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(!isBomb);
            }

            if (collectButton != null)
            {
                collectButton.gameObject.SetActive(!isBomb);
            }
        }

        private void OnContinueClicked()
        {
            Hide(instant: false);
            onContinueCallback?.Invoke();

            // CRITICAL: Popup kapandı, wheel'i regenerate et ve AUTO-SPIN başlat
            WheelOfFortune.Events.GameEvents.TriggerResultPopupClosed(autoSpin: true);
        }

        private void OnCollectClicked()
        {
            Hide(instant: false);
            onCollectCallback?.Invoke();

            // Collect clicked - wheel regenerate et ama auto-spin başlatma
            WheelOfFortune.Events.GameEvents.TriggerResultPopupClosed(autoSpin: false);
        }

        protected override void AnimateShow()
        {
            // Background fade için coroutine
            if (backgroundOverlay != null)
            {
                StartCoroutine(FadeBackground(0f, 0.3f));
            }

            // Base fallback animation kullan
            base.AnimateShow();
        }

        protected override void AnimateHide()
        {
            // Background fade out
            if (backgroundOverlay != null)
            {
                StartCoroutine(FadeBackground(0.3f, 0f));
            }

            // Base fallback animation kullan
            base.AnimateHide();
        }

        private System.Collections.IEnumerator FadeBackground(float from, float to)
        {
            float elapsed = 0f;
            Color color = backgroundOverlay.color;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / animationDuration);
                color.a = alpha;
                backgroundOverlay.color = color;
                yield return null;
            }

            color.a = to;
            backgroundOverlay.color = color;
        }

    }
}
