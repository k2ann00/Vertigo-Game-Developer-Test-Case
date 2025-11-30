using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using WheelOfFortune.Events;
using WheelOfFortune.Interfaces;

namespace WheelOfFortune.Wheel
{
    public class WheelController : MonoBehaviour
    {

        [Header("Wheel Configuration")]
        [SerializeField] private int sliceCount = 8;
        [SerializeField] private WheelSpinner wheelSpinner;

        [Header("Slice References")]
        [SerializeField] private List<WheelSlice> wheelSlices = new List<WheelSlice>();

        [Header("Zone Config")]
        [Tooltip("Current zone için ZoneWheelConfig (auto-load edilir)")]
        [SerializeField] private ZoneWheelConfig currentZoneConfig;

        [Header("Config Loading")]
        [Tooltip("Zone config'leri nereden yüklenecek? (Resources path)")]
        [SerializeField] private string zoneConfigPath = "ZoneConfigs";

        [Header("Wheel Visual Assets")]
        [Tooltip("Bronze (Normal) wheel base sprite")]
        [SerializeField] private Sprite bronzeWheelSprite;

        [Tooltip("Silver (Safe Zone) wheel base sprite")]
        [SerializeField] private Sprite silverWheelSprite;

        [Tooltip("Golden (Super Zone) wheel base sprite")]
        [SerializeField] private Sprite goldenWheelSprite;

        [Tooltip("UI'daki wheel base image referansı")]
        [SerializeField] private Image wheelBaseImage;

        [Header("Wheel Indicators (Optional)")]
        [Tooltip("Bronze wheel indicator sprite")]
        [SerializeField] private Sprite bronzeIndicatorSprite;

        [Tooltip("Silver wheel indicator sprite")]
        [SerializeField] private Sprite silverIndicatorSprite;

        [Tooltip("Golden wheel indicator sprite")]
        [SerializeField] private Sprite goldenIndicatorSprite;

        [Tooltip("UI'daki indicator image referansı")]
        [SerializeField] private Image wheelIndicatorImage;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private int currentHighlightedIndex = -1;
        private int currentZoneNumber = 1;
        private List<WheelSliceData> currentSliceData = new List<WheelSliceData>();

        private void Awake()
        {
            ValidateComponents();
        }

        private void Start()
        {
            // Otomatik olarak ilk zone'u configure et
            if (currentZoneConfig == null)
            {
                LogDebug("Auto-configuring wheel for zone 1...");
                ConfigureWheelForZone(currentZoneNumber);
            }
            else
            {
                LogDebug("Using assigned zone config...");
                ConfigureWheel(currentZoneConfig);
            }
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void ValidateComponents()
        {
            if (wheelSpinner == null)
            {
                wheelSpinner = GetComponentInChildren<WheelSpinner>();
                if (wheelSpinner == null)
                {
                    Debug.LogError("[WheelController] WheelSpinner not found!");
                }
            }

            if (wheelSlices.Count == 0)
            {
                wheelSlices.AddRange(GetComponentsInChildren<WheelSlice>());
            }

            LogDebug($"WheelController initialized with {wheelSlices.Count} slices");
        }

        public void ConfigureWheel(ZoneWheelConfig zoneConfig)
        {
            if (zoneConfig == null)
            {
                Debug.LogError("[WheelController] ZoneWheelConfig is null!");
                return;
            }

            currentZoneConfig = zoneConfig;

            // Zone config'den slice'ları generate et (weighted random!)
            currentSliceData = zoneConfig.GenerateSlices();

            if (currentSliceData.Count != sliceCount)
            {
                Debug.LogWarning($"[WheelController] Generated {currentSliceData.Count} slices, expected {sliceCount}!");
            }

            // WheelSlice'lara data'yı ata
            for (int i = 0; i < wheelSlices.Count && i < currentSliceData.Count; i++)
            {
                wheelSlices[i].Initialize(i, currentSliceData[i]);
            }

            // Wheel type'ı ayarla (Bronze/Silver/Golden visual)
            SetWheelType(zoneConfig.WheelType);

            LogDebug($"Wheel configured for Zone {zoneConfig.ZoneNumber} - Generated {currentSliceData.Count} slices");
        }

        public void ConfigureWheelForZone(int zoneNumber)
        {
            currentZoneNumber = zoneNumber;

            // Zone config'i yükle
            ZoneWheelConfig config = LoadZoneConfig(zoneNumber);

            if (config == null)
            {
                Debug.LogError($"[WheelController] Could not load ZoneConfig for zone {zoneNumber}!");
                return;
            }

            ConfigureWheel(config);
        }

        [System.Obsolete("Use ConfigureWheel(ZoneWheelConfig) instead")]
        public void ConfigureWheel(ZoneData zoneData)
        {
            Debug.LogWarning("[WheelController] Using deprecated ConfigureWheel(ZoneData). Please use ZoneWheelConfig.");

            if (zoneData == null)
            {
                Debug.LogError("[WheelController] ZoneData is null!");
                return;
            }

            LogDebug($"Wheel configured for Zone {zoneData.ZoneNumber} (legacy mode)");
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            if (wheelSpinner != null)
            {
                wheelSpinner.OnSpinCompleted += HandleSpinCompleted;
            }

            // Popup kapandığında wheel'i regenerate et
            GameEvents.OnResultPopupClosed += HandleResultPopupClosed;
        }

        private void UnsubscribeFromEvents()
        {
            if (wheelSpinner != null)
            {
                wheelSpinner.OnSpinCompleted -= HandleSpinCompleted;
            }

            GameEvents.OnResultPopupClosed -= HandleResultPopupClosed;
        }

        private void HandleResultPopupClosed(bool autoSpin)
        {
            RegenerateWheel();
            LogDebug("Wheel regenerated after popup closed - ready for next spin with updated zone config");

            // Auto-spin istendiyse kısa bir delay sonra spin başlat
            if (autoSpin)
            {
                LogDebug("Auto-spin requested - starting spin after short delay");
                StartCoroutine(AutoSpinAfterDelay(0.5f)); // 0.5 saniye delay
            }
        }

        private System.Collections.IEnumerator AutoSpinAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SpinWheel();
            LogDebug("Auto-spin started!");
        }

        private void HandleSpinCompleted(int sliceIndex)
        {
            LogDebug($"Spin completed at slice {sliceIndex}");

            // Kazanılan slice'ı highlight et
            HighlightSlice(sliceIndex);

            // Kazanılan slice data'yı al
            WheelSlice winningSlice = GetSliceByIndex(sliceIndex);
            if (winningSlice != null)
            {
                WheelSliceData sliceData = winningSlice.SliceData;

                LogDebug($"=== SPIN RESULT ===");
                LogDebug($"Index: {sliceIndex}");
                LogDebug($"Item: {sliceData.ItemName} ({sliceData.ItemType})");
                LogDebug($"Amount: {sliceData.Amount}");
                LogDebug($"==================");

                // Event tetikle
                GameEvents.TriggerWheelSpinCompleted(sliceIndex);

                // Bomb kontrolü
                if (sliceData.IsBomb)
                {
                    LogDebug("BOMB HIT!");
                    GameEvents.TriggerBombHit();
                }
                else
                {
                    // WheelSliceData'yı RewardData'ya dönüştür (event için)
                    RewardData reward = ConvertToRewardData(sliceData);
                    GameEvents.TriggerRewardCollected(reward);
                }
            }

        }

        private RewardData ConvertToRewardData(WheelSliceData sliceData)
        {
            // RewardType mapping - Her ItemType için uygun RewardType
            RewardType type = RewardType.BonusItem; // Default

            switch (sliceData.ItemType)
            {
                case ItemType.Cash:
                    type = RewardType.Coin;
                    break;
                case ItemType.Gold:
                    type = RewardType.Gem;
                    break;
                case ItemType.Bomb:
                    type = RewardType.Bomb;
                    break;
                case ItemType.Chest:
                case ItemType.Weapon:
                case ItemType.Armor:
                case ItemType.Consumable:
                case ItemType.SpecialItem:
                    type = RewardType.BonusItem;
                    break;
            }

            // Item ID oluştur - ItemType ve ItemName'den unique key
            string itemId = $"{sliceData.ItemType}_{sliceData.ItemName}".ToLower().Replace(" ", "_");

            // RewardData oluştur
            RewardData reward = new RewardData(
                itemId,
                sliceData.ItemName,
                sliceData.Icon,
                type,
                sliceData.Amount
            );

            LogDebug($"Converted WheelSliceData to RewardData: ItemType={sliceData.ItemType}, RewardType={type}, Name={sliceData.ItemName}");

            return reward;
        }

        public void SpinWheel()
        {
            if (wheelSpinner == null || wheelSpinner.IsSpinning)
            {
                LogDebug("Cannot spin - already spinning or spinner is null");
                return;
            }

            ClearHighlight();

            int targetIndex = Random.Range(0, sliceCount);

            GameEvents.TriggerWheelSpinStarted();

            wheelSpinner.Stop(targetIndex);

            LogDebug($"Wheel spinning to index {targetIndex}");
        }

        public void SpinToSlice(int sliceIndex)
        {
            if (wheelSpinner == null || wheelSpinner.IsSpinning)
            {
                LogDebug("Cannot spin - already spinning or spinner is null");
                return;
            }

            sliceIndex = Mathf.Clamp(sliceIndex, 0, sliceCount - 1);

            ClearHighlight();
            GameEvents.TriggerWheelSpinStarted();
            wheelSpinner.Stop(sliceIndex);

            LogDebug($"Wheel spinning to specific index {sliceIndex}");
        }

        private void HighlightSlice(int index)
        {
            ClearHighlight();

            WheelSlice slice = GetSliceByIndex(index);
            if (slice != null)
            {
                slice.Highlight();
                currentHighlightedIndex = index;
            }
        }

        private void ClearHighlight()
        {
            foreach (WheelSlice slice in wheelSlices)
            {
                if (slice != null)
                {
                    slice.ClearHighlight();
                }
            }

            currentHighlightedIndex = -1;
        }

        private ZoneWheelConfig LoadZoneConfig(int zoneNumber)
        {
            string configName = $"ZoneConfig_{zoneNumber:D3}";  // ZoneConfig_001 002 ...
            string fullPath = $"{zoneConfigPath}/{configName}";

            ZoneWheelConfig config = Resources.Load<ZoneWheelConfig>(fullPath);

            if (config == null)
            {
                Debug.LogWarning($"[WheelController] ZoneConfig not found at: Resources/{fullPath}");

                config = CreateFallbackConfig(zoneNumber);
            }

            return config;
        }

        private ZoneWheelConfig CreateFallbackConfig(int zoneNumber)
        {
            Debug.LogWarning($"[WheelController] Creating fallback config for zone {zoneNumber}");

            // Runtime'da ScriptableObject oluştur (sadece test için!)
            ZoneWheelConfig fallback = ScriptableObject.CreateInstance<ZoneWheelConfig>();

            // Reflection ile private field'lara eriş (ideal değil ama fallback için ok)
            var zoneNumberField = typeof(ZoneWheelConfig).GetField("zoneNumber",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            zoneNumberField?.SetValue(fallback, zoneNumber);

            return fallback;
        }

        private WheelSlice GetSliceByIndex(int index)
        {
            if (index >= 0 && index < wheelSlices.Count)
            {
                return wheelSlices[index];
            }

            return null;
        }

        public void SetTargetZone(int zoneNumber)
        {
            currentZoneNumber = zoneNumber;
            LogDebug($"Target zone set to: {zoneNumber}");
        }

        public void RegenerateWheel()
        {
            // anlık zone no
            ConfigureWheelForZone(currentZoneNumber);
            LogDebug("Wheel regenerated with new random slices!");
        }

        public void SetWheelType(WheelType wheelType)
        {
            if (wheelBaseImage == null)
            {
                Debug.LogWarning("[WheelController] wheelBaseImage referansı atanmamış!");
                return;
            }

            Sprite wheelSprite = null;
            Sprite indicatorSprite = null;

            // Wheel type'a göre sprite'ı seç
            switch (wheelType)
            {
                case WheelType.Bronze:
                    wheelSprite = bronzeWheelSprite;
                    indicatorSprite = bronzeIndicatorSprite;
                    break;
                case WheelType.Silver:
                    wheelSprite = silverWheelSprite;
                    indicatorSprite = silverIndicatorSprite;
                    break;
                case WheelType.Golden:
                    wheelSprite = goldenWheelSprite;
                    indicatorSprite = goldenIndicatorSprite;
                    break;
            }

            // Wheel base sprite'ı değiştir
            if (wheelSprite != null)
            {
                wheelBaseImage.sprite = wheelSprite;
                LogDebug($"Wheel type changed to: {wheelType}");
            }
            else
            {
                Debug.LogWarning($"[WheelController] {wheelType} wheel sprite atanmamış!");
            }

            // Indicator sprite'ı değiştir (opsiyonel)
            if (wheelIndicatorImage != null && indicatorSprite != null)
            {
                wheelIndicatorImage.sprite = indicatorSprite;
            }
        }

        public ZoneWheelConfig GetCurrentZoneConfig()
        {
            return currentZoneConfig;
        }

        public List<WheelSliceData> GetCurrentSliceData()
        {
            return new List<WheelSliceData>(currentSliceData);
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[WheelController] {message}");
            }
        }

        #region HIZLI TEST ICIN YAZDIM
        [ContextMenu("TEST - Spin Wheel")]
        private void TestSpin()
        {
            SpinWheel();
        }

        [ContextMenu("TEST - Load Zone 1")]
        private void TestLoadZone1()
        {
            ConfigureWheelForZone(1);
        }

        [ContextMenu("TEST - Load Zone 5 (Safe)")]
        private void TestLoadZone5()
        {
            ConfigureWheelForZone(5);
        }

        [ContextMenu("TEST - Load Zone 30 (Super)")]
        private void TestLoadZone30()
        {
            ConfigureWheelForZone(30);
        }

        [ContextMenu("TEST - Regenerate Wheel")]
        private void TestRegenerate()
        {
            RegenerateWheel();
        }
        #endregion
    }
}
