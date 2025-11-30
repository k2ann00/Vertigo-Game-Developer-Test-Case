using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Events;
using WheelOfFortune.Interfaces;

namespace WheelOfFortune.Zone
{
    public class ZoneManager : MonoBehaviour, IZoneProgressable
    {

        [Header("Zone Configuration")]
        [SerializeField] private int startingZone = 1;
        [SerializeField] private int maxZones = 100;

        [Header("Safe Zone Settings")]
        [SerializeField] private int safeZoneInterval = 5;  // Her 5. zone safe
        [SerializeField] private int superZoneInterval = 30; // Her 30. zone super

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private int currentZoneNumber;
        private ZoneData currentZone;
        private Dictionary<int, ZoneData> zoneCache = new Dictionary<int, ZoneData>();

        public int CurrentZoneNumber => currentZoneNumber;
        public ZoneData CurrentZone => currentZone;

        private void Awake()
        {
            InitializeZoneSystem();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeZoneSystem()
        {
            currentZoneNumber = startingZone;
            currentZone = CreateZone(currentZoneNumber);

            LogDebug($"Zone system initialized at Zone {currentZoneNumber}");
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnWheelSpinCompleted += HandleWheelSpinCompleted;
            GameEvents.OnGameRestart += HandleGameRestart;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnWheelSpinCompleted -= HandleWheelSpinCompleted;
            GameEvents.OnGameRestart -= HandleGameRestart;
        }

        private void HandleWheelSpinCompleted(int sliceIndex)
        {
            // Bomb değilse bir sonraki zone'a geç
            // Bu kontrolü RewardManager yapabilir, şimdilik basit tut
            LogDebug("Wheel spin completed, preparing for potential zone change");
        }

        private void HandleGameRestart()
        {
            ResetZones();
        }

        public void NextZone()
        {
            if (currentZoneNumber >= maxZones)
            {
                LogDebug("Max zone reached!");
                return;
            }

            currentZoneNumber++;
            currentZone = GetOrCreateZone(currentZoneNumber);

            // Event tetikle
            GameEvents.TriggerZoneChanged(currentZoneNumber);

            LogDebug($"Advanced to Zone {currentZoneNumber} - {currentZone.ZoneName}");
        }

        public ZoneData GetCurrentZone()
        {
            return currentZone;
        }

        public void JumpToZone(int zoneNumber)
        {
            if (zoneNumber < 1 || zoneNumber > maxZones)
            {
                Debug.LogWarning($"[ZoneManager] Invalid zone number: {zoneNumber}");
                return;
            }

            currentZoneNumber = zoneNumber;
            currentZone = GetOrCreateZone(currentZoneNumber);

            GameEvents.TriggerZoneChanged(currentZoneNumber);

            LogDebug($"Jumped to Zone {currentZoneNumber}");
        }

        public void ResetZones()
        {
            currentZoneNumber = startingZone;
            currentZone = GetOrCreateZone(currentZoneNumber);
            zoneCache.Clear();

            GameEvents.TriggerZoneChanged(currentZoneNumber);

            LogDebug("Zone system reset");
        }

        private ZoneData GetOrCreateZone(int zoneNumber)
        {
            // Cache'de varsa döndür
            if (zoneCache.ContainsKey(zoneNumber))
            {
                return zoneCache[zoneNumber];
            }

            // Yoksa oluştur
            ZoneData zone = CreateZone(zoneNumber);
            zoneCache[zoneNumber] = zone;

            return zone;
        }

        private ZoneData CreateZone(int zoneNumber)
        {
            ZoneData zone = new ZoneData(zoneNumber);

            // Zone'a reward'lar ekle
            PopulateZoneRewards(zone);

            return zone;
        }

        private void PopulateZoneRewards(ZoneData zone)
        {
            // TODO: Bu kısım RewardManager ile entegre edilecek
            // Şimdilik dummy reward'lar ekleyelim

            // Normal zone'da 1 bomb
            if (!zone.IsSafeZone)
            {
                RewardData bomb = new RewardData(
                    $"bomb_{zone.ZoneNumber}",
                    "Bomb",
                    null,
                    RewardType.Bomb,
                    0
                );
                zone.AddReward(bomb);
            }

            // Diğer slotlara rastgele reward'lar
            int rewardCount = zone.IsSafeZone ? 8 : 7; // Safe zone'da bomb yok

            for (int i = 0; i < rewardCount; i++)
            {
                RewardType type = (RewardType)Random.Range(0, 3); // Coin, Gem, Multiplier
                int amount = type == RewardType.Multiplier ? 2 : Random.Range(10, 100);

                RewardData reward = new RewardData(
                    $"reward_{zone.ZoneNumber}_{i}",
                    type.ToString(),
                    null,
                    type,
                    amount
                );

                zone.AddReward(reward);
            }
        }

        public bool IsCurrentZoneSafe()
        {
            return currentZone != null && currentZone.IsSafeZone;
        }

        public bool IsCurrentZoneSuper()
        {
            return currentZone != null && currentZone.IsSuperZone;
        }

        public int GetNextSafeZoneNumber()
        {
            int nextSafe = ((currentZoneNumber / safeZoneInterval) + 1) * safeZoneInterval;
            return Mathf.Min(nextSafe, maxZones);
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ZoneManager] {message}");
            }
        }

        [ContextMenu("Debug Current Zone")]
        public void DebugCurrentZone()
        {
            if (currentZone != null)
            {
                Debug.Log(currentZone.ToString());
                Debug.Log($"Rewards in zone: {currentZone.PossibleRewards.Count}");
            }
        }

    }
}
