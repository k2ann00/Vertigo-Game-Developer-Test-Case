using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WheelOfFortune.UI.Components;

namespace WheelOfFortune.UI.Popups
{
    public class SafeZonePopup : UIPanel
    {

        [Header("Display Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI zoneNumberText;

        [Header("Reward Display")]
        [SerializeField] private TextMeshProUGUI totalRewardsText;
        [SerializeField] private TextMeshProUGUI bonusRewardText;

        [Header("Buttons")]
        [SerializeField] private UIButton continueButton;

        [Header("Background")]
        [SerializeField] private Image backgroundOverlay;

        [Header("Effects")]
        [SerializeField] private ParticleSystem celebrationParticles;

        private System.Action onContinueCallback;
        private bool isSuperZone;

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
        }

        private void CleanupButtons()
        {
            if (continueButton != null)
            {
                continueButton.RemoveListener(OnContinueClicked);
            }
        }

        public void ShowSafeZone(int zoneNumber, int totalRewards, int bonusReward = 0, System.Action onContinue = null)
        {
            isSuperZone = (zoneNumber % 30 == 0);
            onContinueCallback = onContinue;

            UpdateDisplay(zoneNumber, totalRewards, bonusReward);
            Show(animated: true);

            // Celebration effect
            if (celebrationParticles != null)
            {
                celebrationParticles.Play();
            }
        }

        private void UpdateDisplay(int zoneNumber, int totalRewards, int bonusReward)
        {
            // Title
            if (titleText != null)
            {
                titleText.text = isSuperZone ? "SUPER ZONE!" : "SAFE ZONE!";
            }

            // Zone number
            if (zoneNumberText != null)
            {
                zoneNumberText.text = $"Zone {zoneNumber}";
            }

            // Message
            if (messageText != null)
            {
                messageText.text = isSuperZone
                    ? "Amazing! No bombs here!\nYour rewards are SAFE!"
                    : "Great! No bombs in this zone!\nYour rewards are safe!";
            }

            // Total rewards
            if (totalRewardsText != null)
            {
                totalRewardsText.text = $"Total Rewards: {totalRewards}";
            }

            // Bonus reward
            if (bonusRewardText != null)
            {
                if (bonusReward > 0)
                {
                    bonusRewardText.text = $"Bonus: +{bonusReward}";
                    bonusRewardText.gameObject.SetActive(true);
                }
                else
                {
                    bonusRewardText.gameObject.SetActive(false);
                }
            }
        }

        private void OnContinueClicked()
        {
            Hide(instant: false);
            onContinueCallback?.Invoke();
        }

        protected override void AnimateShow()
        {
            // Background fade
            if (backgroundOverlay != null)
            {
                Color bgColor = isSuperZone ? new Color(1f, 0.84f, 0f, 0) : new Color(0f, 1f, 0f, 0);
                backgroundOverlay.color = bgColor;
                StartCoroutine(FadeBackground(0f, 0.6f));
            }

            // Base fallback animation kullan
            base.AnimateShow();
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
