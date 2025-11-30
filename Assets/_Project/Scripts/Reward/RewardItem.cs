using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;

namespace WheelOfFortune.Reward
{
    public class RewardItem : MonoBehaviour
    {

        [Header("UI Components")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Text amountText;
        [SerializeField] private Text nameText;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;

        private RewardData rewardData;

        public RewardData RewardData => rewardData;

        public void Initialize(RewardData data)
        {
            rewardData = data;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (rewardData == null)
                return;

            // Icon
            if (iconImage != null && rewardData.Icon != null)
            {
                iconImage.sprite = rewardData.Icon;
                iconImage.enabled = true;
            }
            else if (iconImage != null)
            {
                iconImage.enabled = false;
            }

            // Amount
            if (amountText != null)
            {
                if (rewardData.Type == RewardType.Multiplier)
                {
                    amountText.text = $"x{rewardData.MultiplierValue}";
                }
                else if (rewardData.Amount > 0)
                {
                    amountText.text = rewardData.Amount.ToString();
                }
                else
                {
                    amountText.text = "";
                }
            }

            // Name
            if (nameText != null)
            {
                nameText.text = rewardData.RewardName;
            }
        }

        public void Highlight()
        {
            if (iconImage != null)
            {
                iconImage.color = highlightColor;
            }
        }

        public void RemoveHighlight()
        {
            if (iconImage != null)
            {
                iconImage.color = normalColor;
            }
        }

        public void PlayCollectAnimation()
        {
            // TODO: Animation implementasyonu
            Debug.Log($"[RewardItem] Collecting {rewardData.RewardName}");
        }

    }
}
