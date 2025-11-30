using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Data;

public class WheelSliceUI : MonoBehaviour
{
    [Header("Slice Components")]
    [SerializeField] private Image sliceIcon;
    [SerializeField] private TextMeshProUGUI sliceAmountText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    private RewardData currentReward;

    /// 
    private void Start()
    {
        sliceIcon = GetComponentInChildren<Image>();
        sliceAmountText = GetComponentInChildren<TextMeshProUGUI>();    
    }
    public void SetReward(RewardData reward)
    {
        currentReward = reward;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (currentReward == null)
            return;

        // Icon
        if (sliceIcon != null && currentReward.Icon != null)
        {
            sliceIcon.sprite = currentReward.Icon;
            sliceIcon.enabled = true;
        }
        else if (sliceIcon != null)
        {
            sliceIcon.enabled = false;
        }

        // Amount text
        if (sliceAmountText != null)
        {
            if (currentReward.IsBomb())
            {
                sliceAmountText.text = "💣";
            }
            else if (currentReward.Amount > 0)
            {
                sliceAmountText.text = currentReward.Amount.ToString();
            }
            else
            {
                sliceAmountText.text = "";
            }
        }
    }
}
