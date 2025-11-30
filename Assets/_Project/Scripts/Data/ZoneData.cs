using System;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Data
{
    [Serializable]
    public class ZoneData
    {
        [Header("Zone Info")]
        [SerializeField] private int zoneNumber;
        [SerializeField] private string zoneName;

        [Header("Zone Type")]
        [SerializeField] private bool isSafeZone;    // Her 5. zone (5, 10, 15, 20, 25...)
        [SerializeField] private bool isSuperZone;   // Her 30. zone (30, 60, 90...)

        [Header("Bomb Configuration")]
        [SerializeField] private int bombCount;      // Normal: 1, Safe: 0

        [Header("Rewards")]
        [SerializeField] private List<RewardData> possibleRewards = new List<RewardData>();

        public int ZoneNumber => zoneNumber;
        public string ZoneName => zoneName;
        public bool IsSafeZone => isSafeZone;
        public bool IsSuperZone => isSuperZone;
        public int BombCount => bombCount;
        public List<RewardData> PossibleRewards => possibleRewards;

        public ZoneData() { }

        public ZoneData(int number)
        {
            this.zoneNumber = number;

            // Zone tipini belirle
            this.isSuperZone = (number % 30 == 0);
            this.isSafeZone = (number % 5 == 0);

            // Zone adını oluştur
            if (isSuperZone)
                this.zoneName = $"Super Zone {number}";
            else if (isSafeZone)
                this.zoneName = $"Safe Zone {number}";
            else
                this.zoneName = $"Zone {number}";

            // Bomb sayısını belirle
            this.bombCount = isSafeZone ? 0 : 1;

            this.possibleRewards = new List<RewardData>();
        }

        public void AddReward(RewardData reward)
        {
            if (reward != null && reward.IsValid())
            {
                possibleRewards.Add(reward);
            }
        }

        public RewardData GetRandomReward(bool excludeBomb = true)
        {
            if (possibleRewards.Count == 0)
                return null;

            List<RewardData> validRewards = excludeBomb
                ? possibleRewards.FindAll(r => !r.IsBomb())
                : possibleRewards;

            if (validRewards.Count == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, validRewards.Count);
            return validRewards[randomIndex];
        }

        public override string ToString()
        {
            return $"Zone {zoneNumber} - {zoneName} | Safe: {isSafeZone} | Super: {isSuperZone} | Bombs: {bombCount}";
        }

    }
}
