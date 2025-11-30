using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Events;
using WheelOfFortune.Wheel;

namespace WheelOfFortune.Core
{
    public class ZoneManager : MonoBehaviour
    {

        public static ZoneManager Instance { get; private set; }

        [Header("Zone Settings")]
        [SerializeField] private int startingZone = 1;
        [SerializeField] private int maxZone = 100;

        [Header("Zone Rules")]
        [Tooltip("Her kaç zone'da bir Safe Zone (bomba yok)")]
        [SerializeField] private int safeZoneInterval = 5; // 5, 10, 15, 20...

        [Tooltip("Her kaç zone'da bir Super Zone (özel ödüller)")]
        [SerializeField] private int superZoneInterval = 30; // 30, 60, 90...

        [Header("References")]
        [SerializeField] private WheelController wheelController;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private int currentZone;
        private int highestZoneReached;
        private ZoneWheelConfig currentZoneConfig;

        public int CurrentZone => currentZone;
        public int HighestZoneReached => highestZoneReached;
        public bool IsSafeZone => IsSafeZoneCheck(currentZone);
        public bool IsSuperZone => IsSuperZoneCheck(currentZone);

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            InitializeZone();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeZone()
        {
            // SaveSystem'den yükle (şimdilik PlayerPrefs)
            currentZone = PlayerPrefs.GetInt("CurrentZone", startingZone);
            highestZoneReached = PlayerPrefs.GetInt("HighestZone", startingZone);

            LogDebug($"Zone System initialized - Starting at Zone {currentZone}");

            // İLK KEZ oyun başlarken wheel'i configure et
            if (wheelController != null)
            {
                wheelController.ConfigureWheelForZone(currentZone);
            }

            // Zone UI ve wheel type'ı ayarla (slice'ları değiştirmeden)
            LoadCurrentZone();
        }

        private void LoadCurrentZone()
        {
            // Zone tipini belirle
            WheelType wheelType = GetWheelTypeForZone(currentZone);

            // Sadece visual'ı değiştir (slice'ları DEĞİŞTİRME!)
            if (wheelController != null)
            {
                wheelController.SetWheelType(wheelType);
            }

            // UI'ı güncelle
            UpdateZoneUI();

            // Özel zone ise event tetikle
            if (IsSafeZone)
            {
                GameEvents.TriggerSafeZoneEntered(currentZone);
                LogDebug($"SAFE ZONE entered: Zone {currentZone}");
            }
            else if (IsSuperZone)
            {
                GameEvents.TriggerSuperZoneEntered(currentZone);
                LogDebug($"SUPER ZONE entered: Zone {currentZone}");
            }

            LogDebug($"Zone {currentZone} loaded - Type: {wheelType}");
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnRewardCollected += HandleRewardCollected;
            GameEvents.OnBombHit += HandleBombHit;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnRewardCollected -= HandleRewardCollected;
            GameEvents.OnBombHit -= HandleBombHit;
        }

        private void HandleRewardCollected(RewardData reward)
        {
            // Bomb değilse zone ilerlet
            if (reward.Type != RewardType.Bomb)
            {
                NextZone();
            }
        }

        private void HandleBombHit()
        {
            LogDebug("Bomb hit! Zone stays same - waiting for user decision (revive or trash)");
            // Zone reset YOK! Kullanıcı GameOverPopup'da trash'e basarsa reset olacak
            // Revive ederse kaldığı yerden devam edecek
        }

        public void NextZone()
        {
            currentZone++;

            // Max zone kontrolü
            if (currentZone > maxZone)
            {
                currentZone = maxZone;
                LogDebug("Max zone reached!");
            }

            // Highest zone tracking
            if (currentZone > highestZoneReached)
            {
                highestZoneReached = currentZone;
                SaveProgress();
            }

            // WheelController'a yeni zone number'ını bildir
            // (RegenerateWheel için gerekli)
            if (wheelController != null)
            {
                wheelController.SetTargetZone(currentZone);
            }

            // Event tetikle
            GameEvents.TriggerZoneChanged(currentZone);

            // Yeni zone'u yükle (sadece UI ve wheel type)
            LoadCurrentZone();

            LogDebug($"Advanced to Zone {currentZone}");
        }

        public void ResetToZone(int zoneNumber)
        {
            currentZone = Mathf.Clamp(zoneNumber, startingZone, maxZone);

            // WheelController'a yeni zone number'ını bildir
            if (wheelController != null)
            {
                wheelController.SetTargetZone(currentZone);
            }

            // Event tetikle
            GameEvents.TriggerZoneChanged(currentZone);

            // Zone'u yükle (sadece UI ve wheel type)
            LoadCurrentZone();

            SaveProgress();

            LogDebug($"Reset to Zone {currentZone}");
        }

        public void ResetToZoneOne()
        {
            ResetToZone(startingZone);
        }

        private bool IsSafeZoneCheck(int zone)
        {
            // Super zone değilse ve interval'e bölünüyorsa Safe Zone
            return !IsSuperZoneCheck(zone) && zone % safeZoneInterval == 0;
        }

        private bool IsSuperZoneCheck(int zone)
        {
            // Super zone interval'e bölünüyorsa Super Zone
            return zone % superZoneInterval == 0;
        }

        private WheelType GetWheelTypeForZone(int zone)
        {
            if (IsSuperZoneCheck(zone))
                return WheelType.Golden;
            else if (IsSafeZoneCheck(zone))
                return WheelType.Silver;
            else
                return WheelType.Bronze;
        }

        private void UpdateZoneUI()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateZoneDisplay(currentZone);
            }
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt("CurrentZone", currentZone);
            PlayerPrefs.SetInt("HighestZone", highestZoneReached);
            PlayerPrefs.Save();

            LogDebug($"Progress saved - Zone {currentZone}, Highest: {highestZoneReached}");
        }

        public void ResetProgress()
        {
            currentZone = startingZone;
            highestZoneReached = startingZone;
            SaveProgress();

            LoadCurrentZone();

            LogDebug("Progress reset to Zone 1");
        }

        public ZoneWheelConfig GetCurrentZoneConfig()
        {
            return wheelController?.GetCurrentZoneConfig();
        }

        public WheelType GetZoneType(int zone)
        {
            return GetWheelTypeForZone(zone);
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ZoneManager] {message}");
            }
        }

        [ContextMenu("TEST - Next Zone")]
        private void TestNextZone()
        {
            NextZone();
        }

        [ContextMenu("TEST - Reset Progress")]
        private void TestResetProgress()
        {
            ResetProgress();
        }

        [ContextMenu("TEST - Jump to Zone 5 (Safe)")]
        private void TestJumpToZone5()
        {
            ResetToZone(5);
        }

        [ContextMenu("TEST - Jump to Zone 30 (Super)")]
        private void TestJumpToZone30()
        {
            ResetToZone(30);
        }

    }
}
