using System.Collections.Generic;
using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Events;
using WheelOfFortune.Interfaces;

namespace WheelOfFortune.Reward
{
    public class RewardManager : MonoBehaviour, IRewardable
    {

        private static RewardManager instance;

        public static RewardManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<RewardManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("RewardManager");
                        instance = go.AddComponent<RewardManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Reward Tracking")]
        [SerializeField] private int totalCoinsCollected;
        [SerializeField] private int totalGemsCollected;
        [SerializeField] private float currentMultiplier = 1f;

        [Header("UI References")]
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private GameObject rewardItemPrefab;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private List<RewardData> collectedRewards = new List<RewardData>();
        private int totalRewardsCollected;

        public int TotalRewardsCollected => totalRewardsCollected;
        public int TotalCoinsCollected => totalCoinsCollected;
        public int TotalGemsCollected => totalGemsCollected;
        public float CurrentMultiplier => currentMultiplier;

        private void Awake()
        {
            // Singleton kontrolü
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnRewardCollected += HandleRewardCollected;
            GameEvents.OnBombHit += HandleBombHit;
            GameEvents.OnGameRestart += HandleGameRestart;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnRewardCollected -= HandleRewardCollected;
            GameEvents.OnBombHit -= HandleBombHit;
            GameEvents.OnGameRestart -= HandleGameRestart;
        }

        private void HandleRewardCollected(RewardData reward)
        {
            CollectReward(reward);
        }

        private void HandleBombHit()
        {
            // Ödüller GİTMEZ! Kullanıcı revive edebilir.
            // Sadece Trash button'a basılırsa LoseAllRewards() çağrılır.
            LogDebug("Bomb hit! Rewards are safe until user decides (Revive or Trash)");
        }

        private void HandleGameRestart()
        {
            ResetRewards();
        }

        public void CollectReward(RewardData reward)
        {
            if (reward == null || !reward.IsValid())
            {
                Debug.LogWarning("[RewardManager] Invalid reward!");
                return;
            }

            // Bomb ise toplama
            if (reward.IsBomb())
            {
                LogDebug("Cannot collect bomb as reward!");
                return;
            }

            // Reward'u listeye ekle
            collectedRewards.Add(reward);
            totalRewardsCollected++;

            // Reward tipine göre işle
            ProcessReward(reward);

            LogDebug($"Collected reward: {reward.RewardName} x{reward.Amount}");
        }

        public void LoseAllRewards()
        {
            int lostCoins = totalCoinsCollected;
            int lostGems = totalGemsCollected;

            collectedRewards.Clear();
            totalCoinsCollected = 0;
            totalGemsCollected = 0;
            currentMultiplier = 1f;
            totalRewardsCollected = 0;

            LogDebug($"Lost all rewards! (Coins: {lostCoins}, Gems: {lostGems})");
        }

        private void ProcessReward(RewardData reward)
        {
            switch (reward.Type)
            {
                case RewardType.Coin:
                    int coinAmount = Mathf.RoundToInt(reward.Amount * currentMultiplier);
                    totalCoinsCollected += coinAmount;
                    LogDebug($"Added {coinAmount} coins (with {currentMultiplier}x multiplier)");
                    break;

                case RewardType.Gem:
                    totalGemsCollected += reward.Amount;
                    LogDebug($"Added {reward.Amount} gems");
                    break;

                case RewardType.Multiplier:
                    currentMultiplier *= reward.MultiplierValue;
                    LogDebug($"Multiplier increased to {currentMultiplier}x");
                    break;

                case RewardType.BonusItem:
                    // Bonus item işlemleri
                    LogDebug($"Collected bonus item: {reward.RewardName}");
                    break;
            }
        }

        public List<RewardData> GetCollectedRewards()
        {
            return new List<RewardData>(collectedRewards);
        }

        public void ResetRewards()
        {
            collectedRewards.Clear();
            totalCoinsCollected = 0;
            totalGemsCollected = 0;
            currentMultiplier = 1f;
            totalRewardsCollected = 0;

            LogDebug("Reward system reset");
        }

        public string GetRewardSummary()
        {
            return $"Coins: {totalCoinsCollected} | Gems: {totalGemsCollected} | " +
                   $"Multiplier: x{currentMultiplier} | Total Rewards: {totalRewardsCollected}";
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[RewardManager] {message}");
            }
        }

        [ContextMenu("Debug Reward Summary")]
        public void DebugRewardSummary()
        {
            Debug.Log($"=== Reward Summary ===");
            Debug.Log(GetRewardSummary());
            Debug.Log($"Collected Rewards: {collectedRewards.Count}");

            foreach (RewardData reward in collectedRewards)
            {
                Debug.Log($"  - {reward.RewardName}: {reward.Amount}");
            }
        }

    }
}
