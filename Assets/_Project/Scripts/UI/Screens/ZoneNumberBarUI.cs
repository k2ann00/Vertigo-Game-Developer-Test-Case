using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using WheelOfFortune.Events;
using WheelOfFortune.Data;
using WheelOfFortune.UI.Components;
using UnityEngine.UI;
using DG.Tweening;

namespace WheelOfFortune.UI.Screens
{
    public class ZoneNumberBarUI : UIPanel
    {

        [Header("Zone Number Display")]
        [SerializeField] private Transform zoneNumberContainer;
        [SerializeField] private GameObject zoneNumberPrefab;
        [SerializeField] private GameObject currentZoneIndicator; // Ortadaki indicator

        [Header("Settings")]
        [SerializeField] private int visibleZoneCount = 9; // Ekranda görünen zone sayısı (örn: 9 item)
        [SerializeField] private int totalZones = 100; // Toplam zone sayısı
        [SerializeField] private float itemSpacing = 10f; // Zone item'ları arası boşluk
        [SerializeField] private float itemWidth = 60f; // Her item'ın genişliği

        [Header("Animation")]
        [SerializeField] private float transitionDuration = 0.3f; // Zone değişim animasyon süresi
        [SerializeField] private Ease transitionEase = Ease.OutCubic;

        [Header("Colors")]
        [SerializeField] private Color normalZoneColor = Color.white;
        [SerializeField] private Color currentZoneColor = Color.yellow;
        [SerializeField] private Color safeZoneColor = Color.green;
        [SerializeField] private Color superZoneColor = new Color(1f, 0.8f, 0f); // Golden
        [SerializeField] private Color completedZoneColor = Color.gray;

        // Fixed position items
        private List<ZoneNumberItem> zoneItems = new List<ZoneNumberItem>();
        private int currentZoneNumber = 1;
        private int centerIndex; // Ortadaki item index'i

        protected override void Awake()
        {
            base.Awake();
            InitializeZoneNumbers();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeZoneNumbers()
        {
            if (zoneNumberContainer == null || zoneNumberPrefab == null)
            {
                Debug.LogWarning("[ZoneNumberBarUI] Container or prefab not assigned!");
                return;
            }

            // Merkez index'i hesapla (ortadaki item)
            centerIndex = visibleZoneCount / 2;

            
            // Sabit pozisyonlarda item'ları oluştur
            CreateFixedPositionItems();

            // İlk zone display'i
            UpdateAllZoneNumbers(currentZoneNumber);

            // Indicator'ı ortaya konumlandır
            PositionIndicator();

            Debug.Log($"[ZoneNumberBarUI] Carousel initialized with {visibleZoneCount} items (center index: {centerIndex})");
        }

        private void CreateFixedPositionItems()
        {
            zoneItems.Clear();

            // Container'ın toplam genişliğini hesapla
            float totalWidth = (itemWidth + itemSpacing) * visibleZoneCount;

            // Başlangıç offset'i (ortalaması için)
            float startOffset = -(totalWidth / 2f) + (itemWidth / 2f);

            for (int i = 0; i < visibleZoneCount; i++)
            {
                GameObject itemGO = Instantiate(zoneNumberPrefab, zoneNumberContainer);
                itemGO.name = $"ZoneNumberItem_{i}";

                ZoneNumberItem item = itemGO.GetComponent<ZoneNumberItem>();
                if (item == null)
                {
                    item = itemGO.AddComponent<ZoneNumberItem>();
                }

                // RectTransform ayarları
                RectTransform itemRect = item.RectTransform;
                if (itemRect != null)
                {
                    // Center-aligned anchor/pivot
                    itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                    itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                    itemRect.pivot = new Vector2(0.5f, 0.5f);

                    // Sabit pozisyon hesapla
                    float xPos = startOffset + (i * (itemWidth + itemSpacing));
                    itemRect.anchoredPosition = new Vector2(xPos, 0f);
                }

                zoneItems.Add(item);
            }

            Debug.Log($"[ZoneNumberBarUI] Created {visibleZoneCount} items at fixed positions");
        }

        private void PositionIndicator()
        {
            if (currentZoneIndicator == null) return;

            RectTransform indicatorRect = currentZoneIndicator.GetComponent<RectTransform>();
            if (indicatorRect != null)
            {
                // Ortaya yerleştir
                indicatorRect.anchoredPosition = Vector2.zero;
            }

            currentZoneIndicator.SetActive(true);
        }

        // Event Subscriptions

        private void SubscribeToEvents()
        {
            GameEvents.OnZoneChanged += HandleZoneChanged;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnZoneChanged -= HandleZoneChanged;
        }

        private void HandleZoneChanged(int newZoneNumber)
        {
            currentZoneNumber = newZoneNumber;
            UpdateZoneNumberDisplay(newZoneNumber);
        }

        private void UpdateAllZoneNumbers(int newCenterZone, bool animated = false)
        {
            currentZoneNumber = newCenterZone;

            // Her item için zone number'ı hesapla
            for (int i = 0; i < zoneItems.Count; i++)
            {
                // Item'ın göstereceği zone number = (centerZone - centerIndex + i)
                int zoneNumber = newCenterZone - centerIndex + i;

                // Zone number geçerliyse göster
                if (zoneNumber >= 1 && zoneNumber <= totalZones)
                {
                    UpdateZoneItem(zoneItems[i], zoneNumber, i == centerIndex);
                    zoneItems[i].gameObject.SetActive(true);
                }
                else
                {
                    zoneItems[i].gameObject.SetActive(false);
                }
            }
        }

        private void UpdateZoneItem(ZoneNumberItem item, int zoneNumber, bool isCenter)
        {
            if (item == null) return;

            bool isSafe = IsSafeZone(zoneNumber);
            bool isSuper = IsSuperZone(zoneNumber);
            bool isCompleted = (zoneNumber < currentZoneNumber);

            // Merkezdeki item current zone
            Color textColor = GetZoneColor(isCenter, isSafe, isSuper, isCompleted);

            // Background opacity: Sadece current zone'da farklı
            Color bgColor = Color.clear;
            if (isCenter)
            {
                bgColor = new Color(currentZoneColor.r, currentZoneColor.g, currentZoneColor.b, 0.3f);
            }

            item.Setup(zoneNumber, isCenter, isSafe, isSuper, isCompleted, textColor, bgColor);

            // Highlight animasyonu (opsiyonel)
            if (isCenter)
            {
                item.AnimateHighlight();
            }
        }

        private void UpdateZoneNumberDisplay(int newZoneNumber)
        {
            // Carousel güncelle - tüm number'ları +1 artır
            UpdateAllZoneNumbers(newZoneNumber, true);
        }

        private Color GetZoneColor(bool isCurrent, bool isSafe, bool isSuper, bool isCompleted)
        {
            if (isCurrent)
                return currentZoneColor;
            if (isCompleted)
                return completedZoneColor;
            if (isSuper)
                return superZoneColor;
            if (isSafe)
                return safeZoneColor;

            return normalZoneColor;
        }

        private bool IsSafeZone(int zoneNumber)
        {
            return zoneNumber % 5 == 0;
        }

        private bool IsSuperZone(int zoneNumber)
        {
            return zoneNumber % 30 == 0;
        }

        public void GoToNextZone()
        {
            if (currentZoneNumber < totalZones)
            {
                UpdateAllZoneNumbers(currentZoneNumber + 1, true);
            }
        }

        public void GoToPreviousZone()
        {
            if (currentZoneNumber > 1)
            {
                UpdateAllZoneNumbers(currentZoneNumber - 1, true);
            }
        }

        public void SetZone(int zoneNumber)
        {
            if (zoneNumber >= 1 && zoneNumber <= totalZones)
            {
                UpdateAllZoneNumbers(zoneNumber, false);
            }
        }

    }
}
