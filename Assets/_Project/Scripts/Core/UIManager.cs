using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WheelOfFortune.Events;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Screens;
using WheelOfFortune.UI.Popups;
using WheelOfFortune.UI.Components;

namespace WheelOfFortune.Core
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;

        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UIManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("UIManager");
                        instance = go.AddComponent<UIManager>();
                    }
                }
                return instance;
            }
        }

        [Header("UI References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Sound Button")]
        [SerializeField] private UIButton soundButton;
        [SerializeField] private Image soundButtonImage;
        [SerializeField] private float mutedAlpha = 0.3f;
        [SerializeField] private float unmutedAlpha = 1f;

        [Header("Volume Slider")]
        [SerializeField] private Slider masterVolumeSlider;

        [Header("UI Screens")]
        [SerializeField] private ZoneNumberBarUI zoneNumberBarUI;
        [SerializeField] private LeftSidebarUI leftSidebarUI;

        [Header("UI Popups")]
        [SerializeField] private ResultPopup resultPopup;
        [SerializeField] private GameOverPopup gameOverPopup;
        [SerializeField] private ExitPopup exitPopup;
        [SerializeField] private DeveloperPopup devPopup;

        [Header("Zone Display")]
        [Tooltip("Zone numarasını gösteren text (opsiyonel - ZoneNumberBarUI varsa gerek yok)")]
        [SerializeField] private TextMeshProUGUI zoneNumberText;

        private void Awake()
        {
            // Singleton kontrolü
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUI();
            SetupSoundButton();
            SetupVolumeSlider();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            if (soundButton != null)
            {
                soundButton.RemoveListener(OnSoundButtonClicked);
            }

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(OnVolumeSliderChanged);
            }
        }

        private void InitializeUI()
        {
            // Canvas kontrolü
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }

            LogDebug("UIManager initialized");
        }

        private void SetupSoundButton()
        {
            if (soundButton != null)
            {
                soundButton.AddListener(OnSoundButtonClicked);
                UpdateSoundButtonVisual();
                LogDebug("Sound button setup complete");
            }
        }

        private void SetupVolumeSlider()
        {
            if (masterVolumeSlider != null)
            {
                // Slider'ı 0-1 aralığında ayarla
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;

                // AudioManager'dan mevcut volume değerini al ve slider'a ata
                if (AudioManager.Instance != null)
                {
                    masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();
                }
                else
                {
                    masterVolumeSlider.value = 1f;
                }

                // Slider değiştiğinde çağrılacak metodu bağla
                masterVolumeSlider.onValueChanged.AddListener(OnVolumeSliderChanged);

                LogDebug("Volume slider setup complete");
            }
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnRewardCollected += HandleRewardCollected;
            GameEvents.OnBombHit += HandleBombHit;
            GameEvents.OnZoneChanged += HandleZoneChanged;
            GameEvents.OnSuperZoneEntered += HandleSuperZoneEntered;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnRewardCollected -= HandleRewardCollected;
            GameEvents.OnBombHit -= HandleBombHit;
            GameEvents.OnZoneChanged -= HandleZoneChanged;
            GameEvents.OnSuperZoneEntered -= HandleSuperZoneEntered;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        

        private void HandleRewardCollected(RewardData reward)
        {
            // Result popup'ını göster
            ShowResultPopup(reward);

            LogDebug($"UI: Reward collected - {reward.RewardName}");
        }

        private void HandleBombHit()
        {
            // Game Over popup'ını göster
            ShowGameOverPopup();

            LogDebug("UI: Bomb hit!");
        }

        private void HandleZoneChanged(int newZoneNumber)
        {
            // Zone display'i güncelle
            UpdateZoneDisplay(newZoneNumber);

            LogDebug($"UI: Zone changed to {newZoneNumber}");
        }

        

        private void HandleSuperZoneEntered(int zoneNumber)
        {
            // Super Zone popup'ını göster
           // ShowSuperZonePopup();

            LogDebug($"UI: Super Zone {zoneNumber} entered!");
        }

        private void HandleGameOver()
        {
            ShowGameOverPopup();
        }

        public void UpdateCoinDisplay(int amount)
        {
            if (leftSidebarUI != null)
            {
                leftSidebarUI.UpdateCoinAmount(amount);
            }
        }

        public void UpdateCashDisplay(int amount)
        {
            if (leftSidebarUI != null)
            {
                leftSidebarUI.UpdateCashAmount(amount);
            }
        }

        

        public void UpdateZoneDisplay(int zoneNumber)
        {
            if (zoneNumberText != null)
            {
                zoneNumberText.text = $"ZONE {zoneNumber}";
            }

            // ZoneNumberBarUI kendi event'ini dinliyor, ayrıca update gerekmiyor
        }

        public void ShowResultPopup(RewardData reward)
        {
            if (resultPopup != null)
            {
                resultPopup.ShowResult(reward,
                    onContinue: () =>
                    {
                        LogDebug("Result popup closed - ready for next spin");
                        // Popup kapandı, kullanıcı spin button'a basabilir
                        // Wheel regeneration spin başlarken yapılacak
                    },
                    onCollect: () =>
                    {
                        LogDebug("Collect clicked - collecting rewards");
                        // Opsiyonel: Reward'ları collect et ve güvenli yere kaydet
                        // Şimdilik sadece popup'ı kapat
                    });
            }
            else
            {
                LogDebug($"Result popup not assigned - showing reward: {reward.RewardName}");
            }
        }

        public void ShowGameOverPopup()
        {
            if (gameOverPopup != null)
            {
                // RewardManager'dan bilgileri al (şimdilik dummy değerler)
                int coinsLost = 0;
                int gemsLost = 0;

                if (Reward.RewardManager.Instance != null)
                {
                    coinsLost = Reward.RewardManager.Instance.TotalCoinsCollected;
                    gemsLost = Reward.RewardManager.Instance.TotalGemsCollected;
                }

                gameOverPopup.ShowGameOver(coinsLost, gemsLost);
            }
            else
            {
                LogDebug("Showing Game Over popup");
            }
        }



        public void ShowExitPopup()
        {
            if (exitPopup != null)
            {
                exitPopup.ShowExitPopup();
                LogDebug("Exit popup shown");
            }
            else
            {
                LogDebug("Exit popup not assigned!");
            }
        }

        public void ShowDeveloperPopup()
        {
            if (devPopup != null)
            {
                devPopup.ShowDeveloperPopup();
                LogDebug("Developer popup shown");
            }
            else
            {
                LogDebug("Developer popup not assigned!");
            }
        }

        private void OnSoundButtonClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ToggleMusic();
                UpdateSoundButtonVisual();

                LogDebug($"Sound button clicked - Music {(AudioManager.Instance.IsMusicMuted() ? "muted" : "unmuted")}");
            }
        }

        private void OnVolumeSliderChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
                LogDebug($"Master volume changed to {value:F2}");
            }
        }

        private void UpdateSoundButtonVisual()
        {
            if (soundButtonImage == null || AudioManager.Instance == null)
                return;

            bool isMuted = AudioManager.Instance.IsMusicMuted();

            Color color = soundButtonImage.color;
            color.a = isMuted ? mutedAlpha : unmutedAlpha;
            soundButtonImage.color = color;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[UIManager] {message}");
            }
        }

    }
}
