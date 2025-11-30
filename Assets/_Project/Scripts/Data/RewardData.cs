using System;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public class RewardData
    {
        [Header("Basic Info")]
        [SerializeField] private string rewardId;
        [SerializeField] private string rewardName;
        [SerializeField] private Sprite icon;

        [Header("Reward Properties")]
        [SerializeField] private RewardType type;
        [SerializeField] private int amount;
        [SerializeField] private float multiplierValue = 1f; // Multiplier tipi için

        public string RewardId => rewardId;
        public string RewardName => rewardName;
        public Sprite Icon => icon;
        public RewardType Type => type;
        public int Amount => amount;
        public float MultiplierValue => multiplierValue;

        public RewardData() { }

        public RewardData(string id, string name, Sprite icon, RewardType type, int amount, float multiplier = 1f)
        {
            this.rewardId = id;
            this.rewardName = name;
            this.icon = icon;
            this.type = type;
            this.amount = amount;
            this.multiplierValue = multiplier;
        }

        public bool IsBomb()
        {
            return type == RewardType.Bomb;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(rewardId) && !string.IsNullOrEmpty(rewardName);
        }

    }
}
