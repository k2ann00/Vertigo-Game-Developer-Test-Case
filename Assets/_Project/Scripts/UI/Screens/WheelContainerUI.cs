using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WheelOfFortune.Data;
using WheelOfFortune.Wheel;
using WheelOfFortune.Events;

namespace WheelOfFortune.UI.Screens
{
    public class WheelContainerUI : UIPanel
    {

        [Header("Wheel Components")]
        [SerializeField] private Image wheelBaseImage;
        [SerializeField] private Transform wheelSlicesContainer;
        [SerializeField] private Image wheelCenterIndicator;

        [Header("Wheel Controller")]
        [SerializeField] private WheelController wheelController;

        [Header("Spin Button")]
        [SerializeField] private Button spinButton;
        [SerializeField] private TextMeshProUGUI spinButtonText;

        [Header("Wheel Slices")]
        [SerializeField] private List<WheelSliceUI> wheelSlices = new List<WheelSliceUI>();

        [Header("Multiplier Display")]
        [SerializeField] private TextMeshProUGUI wheelMultiplierText;

        [Header("Settings")]
        [SerializeField] private int sliceCount = 8;

        private bool isSpinning = false;

        protected override void Awake()
        {
            base.Awake();
            ValidateComponents();
            ValidateSlices();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
            SetupSpinButton();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void ValidateComponents()
        {
            // WheelController'ı bul
            if (wheelController == null)
            {
                wheelController = FindObjectOfType<WheelController>();
                if (wheelController == null)
                {
                    Debug.LogError("[WheelContainerUI] WheelController not found!");
                }
            }

            // Spin button'u bul (child'larda ara)
            if (spinButton == null)
            {
                spinButton = GetComponentInChildren<Button>();
            }

            if (spinButtonText == null && spinButton != null)
            {
                spinButtonText = spinButton.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private void SetupSpinButton()
        {
            if (spinButton != null)
            {
                spinButton.onClick.RemoveAllListeners();
                spinButton.onClick.AddListener(OnSpinButtonClicked);
                UpdateSpinButtonState(true);
            }
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnWheelSpinStarted += HandleSpinStarted;
            GameEvents.OnWheelSpinCompleted += HandleSpinCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnWheelSpinStarted -= HandleSpinStarted;
            GameEvents.OnWheelSpinCompleted -= HandleSpinCompleted;
        }

        private void HandleSpinStarted()
        {
            isSpinning = true;
            UpdateSpinButtonState(false);
        }

        private void HandleSpinCompleted(int sliceIndex)
        {
            isSpinning = false;
            UpdateSpinButtonState(true);
        }

        private void OnSpinButtonClicked()
        {
            if (wheelController == null || isSpinning)
            {
                Debug.LogWarning("[WheelContainerUI] Cannot spin - controller null or already spinning!");
                return;
            }

            Debug.Log("[WheelContainerUI] Spin button clicked!");
            wheelController.SpinWheel();
        }

        private void UpdateSpinButtonState(bool enabled)
        {
            if (spinButton != null)
            {
                spinButton.interactable = enabled;
            }

            if (spinButtonText != null)
            {
                spinButtonText.text = enabled ? "SPIN" : "SPINNING...";
            }
        }

        private void ValidateSlices()
        {
            if (wheelSlices.Count == 0 && wheelSlicesContainer != null)
            {
                // Container'dan slice'ları topla
                WheelSliceUI[] slices = wheelSlicesContainer.GetComponentsInChildren<WheelSliceUI>();
                wheelSlices.AddRange(slices);
            }

            if (wheelSlices.Count != sliceCount)
            {
                Debug.LogWarning($"[WheelContainerUI] Expected {sliceCount} slices, found {wheelSlices.Count}");
            }
        }

        public void ConfigureSlices(ZoneData zoneData)
        {
            if (zoneData == null)
            {
                Debug.LogError("[WheelContainerUI] ZoneData is null!");
                return;
            }

            List<RewardData> rewards = zoneData.PossibleRewards;

            for (int i = 0; i < wheelSlices.Count && i < rewards.Count; i++)
            {
                wheelSlices[i].SetReward(rewards[i]);
            }
        }

        public void SetWheelBaseSprite(Sprite sprite)
        {
            if (wheelBaseImage != null)
            {
                wheelBaseImage.sprite = sprite;
            }
        }

        public void UpdateMultiplierDisplay(float multiplier)
        {
            if (wheelMultiplierText != null)
            {
                wheelMultiplierText.text = $"x{multiplier:F1}";
                wheelMultiplierText.gameObject.SetActive(multiplier > 1f);
            }
        }

        public void RotateWheel(float angle)
        {
            if (wheelSlicesContainer != null)
            {
                wheelSlicesContainer.localEulerAngles = new Vector3(0, 0, angle);
            }
        }

        protected override void AnimateShow()
        {
            // Base fallback animation kullan (DOTween import edilince custom animation eklenebilir)
            base.AnimateShow();
        }

    }

}
