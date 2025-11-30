using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;
using DG.Tweening;

namespace WheelOfFortune.Wheel
{
    public class WheelSlice : MonoBehaviour
    {

        [Header("Visual Components")]
        [SerializeField] private Image rewardIcon;
        //[SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private TextMeshProUGUI amountText;

        [Header("Slice Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color bombColor = new Color(0.8f, 0.2f, 0.2f);

        [Header("Animation")]
        [SerializeField] private float highlightDuration = 0.3f;

        private WheelSliceData sliceData;
        private int sliceIndex;
        private bool isHighlighted;

        public WheelSliceData SliceData => sliceData;
        public int SliceIndex => sliceIndex;
        public bool IsBomb => sliceData != null && sliceData.IsBomb;

        public void Initialize(int index, WheelSliceData data)
        {
            sliceIndex = index;
            sliceData = data;

            // Auto-find components if not assigned
            if (rewardIcon == null)
            {
                rewardIcon = transform.Find("Icon")?.GetComponent<Image>();
            }

            if (amountText == null)
            {
                amountText = transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
            }

            UpdateVisuals();
        }

        [System.Obsolete("Use Initialize(int, WheelSliceData) instead")]
        public void Initialize(int index, RewardData reward)
        {
            Debug.LogWarning("[WheelSlice] Using deprecated Initialize method. Please use WheelSliceData.");

            sliceIndex = index;

            // RewardData'yı WheelSliceData'ya dönüştür (basit)
            if (reward != null)
            {
                sliceData = new WheelSliceData();
                // Manuel assignment (reflection kullanmadan)
                // Bu geçici bir çözüm, ideal değil
            }

            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (sliceData == null || !sliceData.IsValid())
            {
                Debug.LogWarning($"[WheelSlice {sliceIndex}] Invalid slice data!");
                return;
            }

            // Icon
            if (rewardIcon != null)
            {
                if (sliceData.Icon != null)
                {
                    rewardIcon.sprite = sliceData.Icon;
                    rewardIcon.enabled = true;
                    rewardIcon.color = Color.white;
                }
                else
                {
                    rewardIcon.enabled = false;
                }
            }

            // Amount text
            if (amountText != null)
            {
                if (sliceData.IsBomb)
                {
                    amountText.text = "";
                    amountText.enabled = false;
                }
                else if (sliceData.Amount > 1)
                {
                    amountText.text = $"x{sliceData.Amount}";
                    amountText.enabled = true;
                }
                else
                {
                    amountText.text = "";
                    amountText.enabled = false;
                }
            }
        }

        private Color GetTextColorByRarity(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common:
                    return Color.white;
                case ItemRarity.Uncommon:
                    return new Color(0.3f, 1f, 0.3f); // Green
                case ItemRarity.Rare:
                    return new Color(0.3f, 0.6f, 1f); // Blue
                case ItemRarity.Epic:
                    return new Color(0.8f, 0.3f, 1f); // Purple
                case ItemRarity.Legendary:
                    return new Color(1f, 0.8f, 0f);   // Gold
                default:
                    return Color.white;
            }
        }

        public void Highlight()
        {
            if (isHighlighted) return;

            isHighlighted = true;

            // Scale animation
            transform.DOScale(1.1f, highlightDuration)
                .SetEase(Ease.OutBack);
        }

        public void ClearHighlight()
        {
            if (!isHighlighted) return;

            isHighlighted = false;

            // Restore scale
            transform.DOScale(1f, highlightDuration)
                .SetEase(Ease.OutQuad);
        }

        public void Reconfigure(WheelSliceData newData)
        {
            sliceData = newData;
            ClearHighlight();
            UpdateVisuals();
        }

        public string GetDisplayText()
        {
            return sliceData?.GetDisplayText() ?? "";
        }

    }
}
