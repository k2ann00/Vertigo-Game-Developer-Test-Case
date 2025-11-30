using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public class WheelSliceData
    {
        [Header("Slice Type")]
        [Tooltip("Bu slice bomba mı?")]
        [SerializeField] private bool isBomb;

        [Header("Item Info")]
        [Tooltip("Item tipi (bomba değilse)")]
        [SerializeField] private ItemType itemType;

        [Tooltip("Item icon'u")]
        [SerializeField] private Sprite icon;

        [Tooltip("Item miktarı")]
        [SerializeField] private int amount;

        [Tooltip("Item adı (UI'da gösterilmek için)")]
        [SerializeField] private string itemName;

        [Header("Visual")]
        [Tooltip("Slice arka plan rengi (optional)")]
        [SerializeField] private Color backgroundColor = Color.white;

        [Tooltip("Item nadirliği (visual effect için)")]
        [SerializeField] private ItemRarity rarity;

        public bool IsBomb => isBomb;
        public ItemType ItemType => itemType;
        public Sprite Icon => icon;
        public int Amount => amount;
        public string ItemName => itemName;
        public Color BackgroundColor => backgroundColor;
        public ItemRarity Rarity => rarity;

        public WheelSliceData()
        {
            isBomb = false;
            amount = 0;
            itemName = string.Empty;
            backgroundColor = Color.white;
        }

        public static WheelSliceData CreateBomb(Sprite bombIcon)
        {
            return new WheelSliceData
            {
                isBomb = true,
                icon = bombIcon,
                itemName = "Bomb",
                itemType = ItemType.Bomb,
                amount = 0,
                backgroundColor = new Color(0.8f, 0.2f, 0.2f) // Kırmızı ton
            };
        }

        public static WheelSliceData CreateFromItem(ItemData itemData, int amount, Color? bgColor = null)
        {
            return new WheelSliceData
            {
                isBomb = false,
                itemType = itemData.ItemType,
                icon = itemData.Icon,
                amount = amount,
                itemName = itemData.ItemName,
                rarity = itemData.Rarity,
                backgroundColor = bgColor ?? itemData.SliceBackgroundColor
            };
        }

        public bool IsValid()
        {
            if (isBomb)
            {
                return icon != null;
            }

            return icon != null && !string.IsNullOrEmpty(itemName);
        }

        public override string ToString()
        {
            if (isBomb)
            {
                return "[BOMB]";
            }

            return $"[{itemType}] {itemName} x{amount}";
        }

        public string GetDisplayText()
        {
            if (isBomb)
            {
                return "BOMB!";
            }

            if (amount > 1)
            {
                return $"{itemName}\nx{amount}";
            }

            return itemName;
        }

    }
}
