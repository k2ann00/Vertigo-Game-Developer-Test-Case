using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Components;
using WheelOfFortune.Reward;
using WheelOfFortune.Events;
using WheelOfFortune.Wheel;
using WheelOfFortune.Core;

namespace WheelOfFortune.UI.Popups
{
    public class ExitPopup : UIPanel
    {
        [Header("Scroll View")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentContainer;

        [Header("Item Prefab")]
        [SerializeField] private GameObject collectedItemPrefab;

        [Header("Message Text")]
        [SerializeField] private TextMeshProUGUI messageText;

        [Header("Buttons")]
        [SerializeField] private UIButton continueButton;
        [SerializeField] private UIButton exitButton;

        [Header("Settings")]
        [SerializeField] private float itemSpacing = 20f;
        [SerializeField] private bool autoScrollToEnd = true;
        [SerializeField] private float slideInDuration = 2f;
        [SerializeField] private float slideInDelay = 0.05f;

        private List<GameObject> spawnedItems = new List<GameObject>();

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupButtons();
        }

        private void SetupButtons()
        {
            if (continueButton != null)
            {
                continueButton.AddListener(OnContinueClicked);
            }

            if (exitButton != null)
            {
                exitButton.AddListener(OnExitClicked);
            }
        }

        private void CleanupButtons()
        {
            if (continueButton != null)
            {
                continueButton.RemoveListener(OnContinueClicked);
            }

            if (exitButton != null)
            {
                exitButton.RemoveListener(OnExitClicked);
            }
        }

        public void ShowExitPopup()
        {
            ClearItems();

            // ÖNCE popup'ı göster (GameObject aktif olmalı coroutine için!)
            Show(animated: true);

            // SONRA item'leri populate et (artık GameObject aktif, coroutine çalışabilir)
            PopulateCollectedItems();

            if (autoScrollToEnd)
            {
                StartCoroutine(ScrollToEndAfterDelay());
            }
        }

        private void PopulateCollectedItems()
        {
            if (RewardManager.Instance == null)
            {
                Debug.LogWarning("[ExitPopup] RewardManager not found!");
                return;
            }

            List<RewardData> collectedRewards = RewardManager.Instance.GetCollectedRewards();

            if (collectedRewards.Count == 0)
            {
                Debug.Log("[ExitPopup] No rewards collected yet");
                return;
            }

            // Aynı item'ları grupla ve amount'ları topla
            Dictionary<string, RewardData> groupedRewards = new Dictionary<string, RewardData>();

            foreach (RewardData reward in collectedRewards)
            {
                string key = reward.RewardId;

                if (groupedRewards.ContainsKey(key))
                {
                    // Mevcut reward'ın amount'ını artır
                    RewardData existingReward = groupedRewards[key];
                    int totalAmount = existingReward.Amount + reward.Amount;

                    // Yeni RewardData oluştur (amount güncellenmiş)
                    groupedRewards[key] = new RewardData(
                        existingReward.RewardId,
                        existingReward.RewardName,
                        existingReward.Icon,
                        existingReward.Type,
                        totalAmount
                    );
                }
                else
                {
                    // Yeni reward ekle
                    groupedRewards.Add(key, reward);
                }
            }

            // Gruplanan reward'ları göster
            int index = 0;
            foreach (var kvp in groupedRewards)
            {
                CreateItemUI(kvp.Value, index);
                index++;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer as RectTransform);

            Debug.Log($"[ExitPopup] Populated {groupedRewards.Count} unique items (from {collectedRewards.Count} total rewards)");
        }

        private void CreateItemUI(RewardData reward, int index)
        {
            if (collectedItemPrefab == null)
            {
                Debug.LogError("[ExitPopup] CollectedItemPrefab is null!");
                return;
            }

            GameObject itemObj = Instantiate(collectedItemPrefab, contentContainer);
            itemObj.name = $"CollectedItem_{reward.RewardId}";

            // Icon ve Amount child'larını bul ve setup et
            SetupItemDisplay(itemObj, reward);

            // Slide-in animasyonu başlat
            StartCoroutine(PlaySlideInAnimation(itemObj, index));

            spawnedItems.Add(itemObj);
        }

        private void SetupItemDisplay(GameObject itemObj, RewardData reward)
        {
            // Icon child'ı bul
            Transform iconTransform = itemObj.transform.Find("Icon");
            if (iconTransform != null)
            {
                Image iconImage = iconTransform.GetComponent<Image>();
                if (iconImage != null && reward.Icon != null)
                {
                    iconImage.sprite = reward.Icon;
                    iconImage.enabled = true;
                }
                else if (reward.Icon == null)
                {
                    Debug.LogWarning($"[ExitPopup] Reward '{reward.RewardName}' has no icon!");
                }
            }
            else
            {
                Debug.LogError("[ExitPopup] 'Icon' child not found in prefab!");
            }

            // Amount child'ı bul
            Transform amountTransform = itemObj.transform.Find("Amount");
            if (amountTransform != null)
            {
                TextMeshProUGUI amountText = amountTransform.GetComponent<TextMeshProUGUI>();
                if (amountText != null)
                {
                    amountText.text = reward.Amount.ToString();
                }
            }
            else
            {
                Debug.LogError("[ExitPopup] 'Amount' child not found in prefab!");
            }
        }

        private System.Collections.IEnumerator PlaySlideInAnimation(GameObject itemObj, int index)
        {
            CanvasGroup canvasGroup = itemObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = itemObj.AddComponent<CanvasGroup>();
            }

            RectTransform rectTransform = itemObj.GetComponent<RectTransform>();
            if (rectTransform == null) yield break;

            canvasGroup.alpha = 0f;
            float delay = index * slideInDelay;

            yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            Vector3 startPos = rectTransform.localPosition + Vector3.left * 100f;
            Vector3 targetPos = rectTransform.localPosition;

            rectTransform.localPosition = startPos;

            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideInDuration;
                t = Mathf.SmoothStep(0f, 1f, t);

                rectTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                canvasGroup.alpha = t;

                yield return null;
            }

            rectTransform.localPosition = targetPos;
            canvasGroup.alpha = 1f;
        }

        private void ClearItems()
        {
            foreach (GameObject item in spawnedItems)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }

            spawnedItems.Clear();
        }

        private System.Collections.IEnumerator ScrollToEndAfterDelay()
        {
            // Layout'un güncellenmesi için bekle
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (scrollRect != null)
            {
                // Layout'u zorla güncelle
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer as RectTransform);

                // Scroll'u sona ayarla (1f = en sağ)
                scrollRect.horizontalNormalizedPosition = 1f;

                Debug.Log($"[ExitPopup] Auto-scrolled to end. Position: {scrollRect.horizontalNormalizedPosition}");
            }
            else
            {
                Debug.LogWarning("[ExitPopup] ScrollRect is null - cannot auto scroll!");
            }
        }

        private void OnContinueClicked()
        {
            Hide(instant: false);
        }

        private void OnExitClicked()
        {
            Debug.Log("[ExitPopup] Exit clicked - resetting everything and restarting...");

            // 1. RewardManager'ı sıfırla (tüm ödüller temizlenir)
            if (RewardManager.Instance != null)
            {
                RewardManager.Instance.ResetRewards();
                Debug.Log("[ExitPopup] Rewards reset");
            }

            // 2. Zone'u 1'e set et (ZoneManager üzerinden - UI de güncellenir!)
            if (Core.ZoneManager.Instance != null)
            {
                Core.ZoneManager.Instance.ResetToZoneOne();
                Debug.Log("[ExitPopup] Zone reset to 1 via ZoneManager");
            }

            // 3. Wheel'i regenerate et
            var wheelController = FindObjectOfType<WheelController>();
            if (wheelController != null)
            {
                wheelController.RegenerateWheel();
                Debug.Log("[ExitPopup] Wheel regenerated");
            }

            // 4. Game restart event'ini tetikle
            GameEvents.TriggerGameRestart();

            // 5. Popup'ı kapat
            Hide(instant: false);

            // 6. ÖNEMLI: autoSpin = FALSE! (Otomatik dönmesin, kullanıcı butona bassın)
            GameEvents.TriggerResultPopupClosed(autoSpin: false);

            Debug.Log("[ExitPopup] Game reset complete - Zone 1, ready for manual spin");
        }
    }
}
