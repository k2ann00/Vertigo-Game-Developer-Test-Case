using UnityEngine;

namespace WheelOfFortune.Data
{
    [CreateAssetMenu(fileName = "Item_", menuName = "WheelOfFortune/Item Data", order = 1)]
    public class ItemData : ScriptableObject
    {
        [Header("Item Info")]
        [Tooltip("Benzersiz item ID (örn: cash_basic, chest_bronze)")]
        [SerializeField] private string itemId;

        [Tooltip("Kullanıcıya gösterilecek item adı")]
        [SerializeField] private string itemName;

        [SerializeField] private ItemType itemType;
        [SerializeField] private ItemRarity rarity;

        [Tooltip("Item icon/sprite")]
        [SerializeField] private Sprite icon;

        [Header("Amount Range")]
        [Tooltip("Minimum miktar (random için)")]
        [SerializeField] private int minAmount = 1;

        [Tooltip("Maximum miktar (random için)")]
        [SerializeField] private int maxAmount = 1;

        [Header("Zone Availability")]
        [Tooltip("Bu item hangi zone'dan itibaren çıkabilir")]
        [SerializeField] private int availableFromZone = 1;

        [Tooltip("Bu item hangi zone'a kadar çıkabilir (999 = sınırsız)")]
        [SerializeField] private int availableUntilZone = 999;

        [Header("Spawn Settings")]
        [Tooltip("Weighted random için spawn ağırlığı (0-100)")]
        [Range(0f, 100f)]
        [SerializeField] private float baseSpawnWeight = 10f;

        [Header("Visual Settings")]
        [Tooltip("Slice'ın arka plan rengi (optional)")]
        [SerializeField] private Color sliceBackgroundColor = Color.white;

        public string ItemId => itemId;
        public string ItemName => itemName;
        public ItemType ItemType => itemType;
        public ItemRarity Rarity => rarity;
        public Sprite Icon => icon;
        public int MinAmount => minAmount;
        public int MaxAmount => maxAmount;
        public int AvailableFromZone => availableFromZone;
        public int AvailableUntilZone => availableUntilZone;
        public float BaseSpawnWeight => baseSpawnWeight;
        public Color SliceBackgroundColor => sliceBackgroundColor;

        public bool IsAvailableInZone(int zoneNumber)
        {
            return zoneNumber >= availableFromZone && zoneNumber <= availableUntilZone;
        }

        public int GetRandomAmount()
        {
            return Random.Range(minAmount, maxAmount + 1);
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(itemId)
                   && !string.IsNullOrEmpty(itemName)
                   && icon != null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Min/Max kontrolü
            if (minAmount > maxAmount)
            {
                maxAmount = minAmount;
            }

            // Zone kontrolü
            if (availableFromZone < 1)
            {
                availableFromZone = 1;
            }

            if (availableUntilZone < availableFromZone)
            {
                availableUntilZone = availableFromZone;
            }

            // ItemId otomatik oluşturma (eğer boşsa)
            if (string.IsNullOrEmpty(itemId) && !string.IsNullOrEmpty(itemName))
            {
                itemId = itemName.ToLower().Replace(" ", "_");
            }
        }
#endif

    }
}
