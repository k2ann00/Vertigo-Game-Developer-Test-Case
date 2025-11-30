using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WheelOfFortune.Utils;

namespace WheelOfFortune.Data
{
    public enum WheelType
    {
        Bronze,

        Silver,

        Golden
    }

    [CreateAssetMenu(fileName = "ZoneConfig_", menuName = "WheelOfFortune/Zone Wheel Config", order = 2)]
    public class ZoneWheelConfig : ScriptableObject
    {
        [Header("Zone Info")]
        [Tooltip("Zone numarası (1-100)")]
        [SerializeField] private int zoneNumber = 1;

        [SerializeField] private WheelType wheelType = WheelType.Bronze;

        [Tooltip("Safe zone mu? (Her 5. zone)")]
        [SerializeField] private bool isSafeZone;

        [Tooltip("Super zone mu? (Her 30. zone)")]
        [SerializeField] private bool isSuperZone;

        [Header("Test Mode")]
        [Tooltip("TEST: Tüm slice'lar bomba olsun mu? (Bomb test için)")]
        [SerializeField] private bool isBombTestZone;

        [Header("Wheel Settings")]
        [Tooltip("Wheel'da kaç slice var")]
        [SerializeField] private int sliceCount = 8;

        [Tooltip("Kaç tane bomba olacak (safe zone'larda 0)")]
        [SerializeField] private int bombCount = 1;

        [Header("Item Pool")]
        [Tooltip("Bu zone'da çıkabilecek item'lar (weighted random ile seçilir)")]
        [SerializeField] private List<ItemData> availableItems = new List<ItemData>();

        [Header("Amount Multipliers")]
        [Tooltip("Cash miktarlarını çarp (zone ilerledikçe artar)")]
        [SerializeField] private float cashMultiplier = 1f;

        [Tooltip("Gold miktarlarını çarp (zone ilerledikçe artar)")]
        [SerializeField] private float goldMultiplier = 1f;

        [Header("Bomb Icon")]
        [Tooltip("Bomba icon'u (optional - yoksa Resources'dan yüklenir)")]
        [SerializeField] private Sprite bombIcon;

        public int ZoneNumber => zoneNumber;
        public WheelType WheelType => wheelType;
        public bool IsSafeZone => isSafeZone;
        public bool IsSuperZone => isSuperZone;
        public bool IsBombTestZone => isBombTestZone;
        public int SliceCount => sliceCount;
        public int BombCount => bombCount;
        public List<ItemData> AvailableItems => availableItems;
        public float CashMultiplier => cashMultiplier;
        public float GoldMultiplier => goldMultiplier;

        public List<WheelSliceData> GenerateSlices()
        {
            List<WheelSliceData> slices = new List<WheelSliceData>();

            // TEST MODE: Tüm slice'lar bomba
            if (isBombTestZone)
            {
                Debug.LogWarning($"[ZoneWheelConfig] Zone {zoneNumber}: BOMB TEST MODE - All slices are bombs!");

                for (int i = 0; i < sliceCount; i++)
                {
                    Sprite icon = GetBombIcon();
                    slices.Add(WheelSliceData.CreateBomb(icon));
                }

                return slices;
            }

            // NORMAL MODE: Bomba + Item karışımı
            // 1. Bomba slice'larını ekle
            for (int i = 0; i < bombCount; i++)
            {
                Sprite icon = GetBombIcon();
                slices.Add(WheelSliceData.CreateBomb(icon));
            }

            // 2. Kalan slice'ları weighted random ile doldur
            int remainingSlices = sliceCount - bombCount;
            for (int i = 0; i < remainingSlices; i++)
            {
                ItemData selectedItem = SelectRandomItem();
                if (selectedItem != null)
                {
                    WheelSliceData sliceData = CreateSliceFromItem(selectedItem);
                    slices.Add(sliceData);
                }
                else
                {
                    Debug.LogWarning($"[ZoneWheelConfig] Zone {zoneNumber}: Item pool boş veya geçersiz!");
                }
            }

            // 3. Slice'ları karıştır (Fisher-Yates shuffle)
            slices.Shuffle();

            return slices;
        }

        private ItemData SelectRandomItem()
        {
            if (availableItems == null || availableItems.Count == 0)
            {
                return null;
            }

            // Zone'da kullanılabilir item'ları filtrele
            List<ItemData> validItems = availableItems
                .Where(item => item != null && item.IsAvailableInZone(zoneNumber))
                .ToList();

            if (validItems.Count == 0)
            {
                Debug.LogWarning($"[ZoneWheelConfig] Zone {zoneNumber}: Kullanılabilir item yok!");
                return null;
            }

            // Weighted random selection (ListExtensions kullan)
            return validItems.SelectWeightedRandom(item => item.BaseSpawnWeight);
        }

        private WheelSliceData CreateSliceFromItem(ItemData itemData)
        {
            // Random amount al
            int amount = itemData.GetRandomAmount();

            // Zone multiplier uygula
            amount = ApplyZoneMultiplier(itemData.ItemType, amount);

            // Slice oluştur
            return WheelSliceData.CreateFromItem(itemData, amount);
        }

        private int ApplyZoneMultiplier(ItemType itemType, int baseAmount)
        {
            float multiplier = 1f;

            switch (itemType)
            {
                case ItemType.Cash:
                    multiplier = cashMultiplier;
                    break;

                case ItemType.Gold:
                    multiplier = goldMultiplier;
                    break;

                // Diğer item tipleri için multiplier eklenebilir
                default:
                    multiplier = 1f;
                    break;
            }

            return Mathf.RoundToInt(baseAmount * multiplier);
        }

        private Sprite GetBombIcon()
        {
            if (bombIcon != null)
            {
                return bombIcon;
            }

            // Resources'dan yükle
            Sprite loadedIcon = Resources.Load<Sprite>("Icons/ui_card_icon_death");

            if (loadedIcon == null)
            {
                Debug.LogWarning("[ZoneWheelConfig] Bomb icon bulunamadı! Resources/Icons/ui_card_icon_death.png kontrol edin.");
            }

            return loadedIcon;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Zone number kontrolü
            if (zoneNumber < 1)
            {
                zoneNumber = 1;
            }

            // Slice count kontrolü
            if (sliceCount < 1)
            {
                sliceCount = 8;
            }

            // TEST MODE: Bomb test zone aktifse
            if (isBombTestZone)
            {
                // Bomb test mode'da tüm ayarları override et
                bombCount = sliceCount; // Tüm slice'lar bomba
                isSafeZone = false;
                isSuperZone = false;
                wheelType = WheelType.Bronze;
                Debug.LogWarning($"[ZoneWheelConfig] Zone {zoneNumber}: BOMB TEST MODE aktif - Tüm slice'lar bomba!");
                return; // Diğer validasyonları atla
            }

            // Bomb count kontrolü
            if (bombCount < 0)
            {
                bombCount = 0;
            }

            if (bombCount >= sliceCount)
            {
                bombCount = sliceCount - 1;
            }

            // Safe/Super zone'larda bomba olmamalı
            if (isSafeZone || isSuperZone)
            {
                bombCount = 0;
            }

            // Zone tipini otomatik ayarla
            AutoSetZoneType();
        }

        private void AutoSetZoneType()
        {
            if (zoneNumber % 30 == 0)
            {
                wheelType = WheelType.Golden;
                isSuperZone = true;
                isSafeZone = true; // Super zone aynı zamanda safe
                bombCount = 0;
            }
            else if (zoneNumber % 5 == 0)
            {
                wheelType = WheelType.Silver;
                isSafeZone = true;
                isSuperZone = false;
                bombCount = 0;
            }
            else
            {
                wheelType = WheelType.Bronze;
                isSafeZone = false;
                isSuperZone = false;
                bombCount = 1;
            }
        }

        [ContextMenu("Auto Set Multipliers")]
        private void AutoSetMultipliers()
        {
            // Zone ilerledikçe ödüller artar
            cashMultiplier = 1f + (zoneNumber * 0.1f);
            goldMultiplier = 1f + (zoneNumber * 0.05f);

            Debug.Log($"[ZoneWheelConfig] Zone {zoneNumber}: Cash x{cashMultiplier:F1}, Gold x{goldMultiplier:F1}");
        }
#endif

        public override string ToString()
        {
            if (isBombTestZone)
            {
                return $"Zone {zoneNumber} [BOMB TEST] - ALL SLICES ARE BOMBS! ({sliceCount} bombs)";
            }

            string zoneType = isSuperZone ? "SUPER" : (isSafeZone ? "SAFE" : "NORMAL");
            return $"Zone {zoneNumber} [{zoneType}] - {wheelType} Wheel | Slices: {sliceCount} | Bombs: {bombCount}";
        }

    }
}
