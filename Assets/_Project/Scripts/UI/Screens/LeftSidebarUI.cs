using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using WheelOfFortune.UI.Components;
using WheelOfFortune.Events;
using WheelOfFortune.Data;

namespace WheelOfFortune.UI.Screens
{
    public class LeftSidebarUI : UIPanel
    {

        [Header("Exit Button")]
        [SerializeField] private UIButton exitButton;
        [SerializeField] private UIButton devButton;

        [Header("Collected Items - Dynamic System")]
        [Tooltip("Collected item'ların ekleneceği container (Vertical Layout Group olmalı)")]
        [SerializeField] private Transform collectedItemsContainer;

        [Tooltip("Collected item prefab (Image + TextMeshProUGUI içermeli)")]
        [SerializeField] private GameObject collectedItemPrefab;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        // Dinamik item tracking - Key: ItemID, Value: CollectedItemUI instance
        private Dictionary<string, CollectedItemUI> itemDisplays = new Dictionary<string, CollectedItemUI>();

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
            ValidateReferences();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupButtons();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtons()
        {
            if (exitButton != null)
            {
                exitButton.AddListener(OnExitClicked);
            }

            if (devButton != null)
            {
                devButton.AddListener(OnDevButtonClicked);
            }
        }

        private void CleanupButtons()
        {
            if (exitButton != null)
            {
                exitButton.RemoveListener(OnExitClicked);
            }
        }

        private void ValidateReferences()
        {
            if (collectedItemsContainer == null)
            {
                Debug.LogError("[LeftSidebarUI] collectedItemsContainer referansı eksik!");
            }

            if (collectedItemPrefab == null)
            {
                Debug.LogWarning("[LeftSidebarUI] collectedItemPrefab referansı eksik! Item'lar gösterilemeyecek.");
            }

            LogDebug("LeftSidebarUI initialized with dynamic item system");
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnRewardCollected += HandleRewardCollected;
            GameEvents.OnGameRestart += HandleGameRestart;
            GameEvents.OnWheelSpinStarted += HandleWheelSpinStarted;
            GameEvents.OnWheelSpinCompleted += HandleWheelSpinCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnRewardCollected -= HandleRewardCollected;
            GameEvents.OnGameRestart -= HandleGameRestart;
            GameEvents.OnWheelSpinStarted -= HandleWheelSpinStarted;
            GameEvents.OnWheelSpinCompleted -= HandleWheelSpinCompleted;
        }

        private void HandleRewardCollected(RewardData reward)
        {
            // Reward'u dinamik olarak ekle veya güncelle
            AddOrUpdateItem(reward);
        }

        private void HandleGameRestart()
        {
            // Oyun yeniden başladığında tüm item'ları temizle
            ClearAllItems();
            LogDebug("Game restarted - all items cleared from LeftBar");
        }

        private void HandleWheelSpinStarted()
        {
            // Spinning sırasında exit button'u devre dışı bırak
            SetExitButtonInteractable(false);
            LogDebug("Exit button disabled during spin");
        }

        private void HandleWheelSpinCompleted(int sliceIndex)
        {
            // Spin bittikten sonra exit button'u aktif et
            SetExitButtonInteractable(true);
            LogDebug("Exit button enabled after spin");
        }

        private void SetExitButtonInteractable(bool interactable)
        {
            if (exitButton != null)
            {
                exitButton.SetInteractable(interactable);
            }
        }

        private void OnExitClicked()
        {
            Debug.Log("[LeftSidebarUI] Exit button clicked");

            if (Core.UIManager.Instance != null)
            {
                Core.UIManager.Instance.ShowExitPopup();
            }
            else
            {
                Debug.LogWarning("[LeftSidebarUI] UIManager not found!");
            }
        }

        private void OnDevButtonClicked()
        {
            Debug.Log("[LeftSidebarUI] Dev button clicked");

            if (Core.UIManager.Instance != null)
            {
                Core.UIManager.Instance.ShowDeveloperPopup();
            }
            else
            {
                Debug.LogWarning("[LeftSidebarUI] UIManager not found!");
            }
        }

        private void AddOrUpdateItem(RewardData reward)
        {
            if (reward == null || !reward.IsValid())
            {
                LogDebug("Invalid reward data, skipping");
                return;
            }

            // Bomb'ları gösterme
            if (reward.IsBomb())
            {
                LogDebug("Bomb rewards are not displayed in collection");
                return;
            }

            // Item ID - unique identifier
            string itemId = reward.RewardId;

            // Zaten mevcut mu?
            if (itemDisplays.ContainsKey(itemId))
            {
                // Mevcut item'ı güncelle
                CollectedItemUI existingItem = itemDisplays[itemId];
                existingItem.UpdateAmount(reward.Amount);

                LogDebug($"Updated existing item: {reward.RewardName} (+{reward.Amount})");
            }
            else
            {
                // Yeni item ekle
                CollectedItemUI newItem = CreateNewItemDisplay(reward);
                if (newItem != null)
                {
                    itemDisplays[itemId] = newItem;
                    LogDebug($"Added new item: {reward.RewardName} (x{reward.Amount})");
                }
            }
        }

        private CollectedItemUI CreateNewItemDisplay(RewardData reward)
        {
            if (collectedItemPrefab == null || collectedItemsContainer == null)
            {
                Debug.LogError("[LeftSidebarUI] Cannot create item - prefab or container is null!");
                return null;
            }

            // Prefab'ı instantiate et
            GameObject itemObj = Instantiate(collectedItemPrefab, collectedItemsContainer);
            itemObj.name = $"CollectedItem_{reward.RewardId}";

            // CollectedItemUI component'ini al veya ekle
            CollectedItemUI itemUI = itemObj.GetComponent<CollectedItemUI>();
            if (itemUI == null)
            {
                itemUI = itemObj.AddComponent<CollectedItemUI>();
            }

            // Item'ı initialize et
            itemUI.Initialize(reward.Icon, reward.Amount);

            return itemUI;
        }

        public void ClearAllItems()
        {
            foreach (var kvp in itemDisplays)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }

            itemDisplays.Clear();
            LogDebug("Cleared all collected items");
        }

        [System.Obsolete("Use AddOrUpdateItem with RewardData instead")]
        public void UpdateCoinAmount(int amount)
        {
            // Coin item'ını manuel oluştur
            RewardData coinReward = new RewardData(
                "coin",
                "Coin",
                null,
                RewardType.Coin,
                amount
            );
            AddOrUpdateItem(coinReward);
        }

        [System.Obsolete("Use AddOrUpdateItem with RewardData instead")]
        public void UpdateCashAmount(int amount)
        {
            // Cash item'ını manuel oluştur
            RewardData cashReward = new RewardData(
                "cash",
                "Cash",
                null,
                RewardType.Coin,
                amount
            );
            AddOrUpdateItem(cashReward);
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[LeftSidebarUI] {message}");
            }
        }

        [ContextMenu("Debug - Show All Items")]
        private void DebugShowAllItems()
        {
            Debug.Log($"=== Collected Items ({itemDisplays.Count}) ===");
            foreach (var kvp in itemDisplays)
            {
                Debug.Log($"  - {kvp.Key}: {kvp.Value.CurrentAmount}");
            }
        }

    }

    [System.Serializable]
    public class CollectedItemUI : MonoBehaviour
    {

        [Header("UI References")]
        [Tooltip("Item icon'u için Image component")]
        [SerializeField] private Image iconImage;

        [Tooltip("Item sayısı için TextMeshProUGUI component")]
        [SerializeField] private TextMeshProUGUI amountText;

        private int currentAmount = 0;

        public int CurrentAmount => currentAmount;

        public void Initialize(Sprite icon, int initialAmount)
        {
            // Runtime'da component eklendiyse, child component'leri bul
            if (iconImage == null)
            {
                // Önce GetComponentInChildren ile bul
                iconImage = GetComponentInChildren<Image>();

                // Bulunamadıysa specific name ile ara
                if (iconImage == null)
                {
                    Transform iconTransform = transform.Find("Icon");
                    if (iconTransform != null)
                    {
                        iconImage = iconTransform.GetComponent<Image>();
                    }
                }
            }

            if (amountText == null)
            {
                // TextMeshProUGUI component'ini bul
                amountText = GetComponentInChildren<TextMeshProUGUI>();

                // Bulunamadıysa specific name ile ara
                if (amountText == null)
                {
                    Transform amountTransform = transform.Find("Amount");
                    if (amountTransform != null)
                    {
                        amountText = amountTransform.GetComponent<TextMeshProUGUI>();
                    }
                }
            }

            // Validation
            if (iconImage == null)
            {
                Debug.LogError("[CollectedItemUI] Icon Image component not found! Make sure prefab has an Image child named 'Icon'");
            }

            if (amountText == null)
            {
                Debug.LogError("[CollectedItemUI] Amount TextMeshProUGUI component not found! Make sure prefab has a TextMeshProUGUI child named 'Amount'");
            }

            SetIcon(icon);
            SetAmount(initialAmount);
        }

        public void SetAmount(int amount)
        {
            currentAmount = amount;
            UpdateDisplay();
        }

        public void UpdateAmount(int addAmount)
        {
            currentAmount += addAmount;
            UpdateDisplay();
        }

        public void SetIcon(Sprite icon)
        {
            if (iconImage != null && icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
        }

        private void UpdateDisplay()
        {
            if (amountText != null)
            {
                amountText.text = FormatAmount(currentAmount);
            }
        }

        private string FormatAmount(int amount)
        {
            // Kısa format (opsiyonel)
            if (amount >= 1000000)
            {
                return $"{amount / 1000000f:F1}M"; // 1.5M
            }
            else if (amount >= 1000)
            {
                return $"{amount / 1000f:F1}k"; // 11.7k
            }

            return amount.ToString();
        }

    }
}
