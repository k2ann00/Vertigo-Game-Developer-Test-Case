using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using WheelOfFortune.UI.Components;
using WheelOfFortune.Events;
using WheelOfFortune.Wheel;
using WheelOfFortune.Core;

namespace WheelOfFortune.UI.Popups
{
    public class GameOverPopup : UIPanel
    {

        [Header("Display Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private TextMeshProUGUI totalRewardsText;

        [Header("Buttons")]
        [SerializeField] private UIButton trashButton; // Çöpe At
        [SerializeField] private UIButton reviveVideoButton; // Revive (Reklam/Video)
        [SerializeField] private UIButton reviveCoinButton; // 90 Altın Revive

        [Header("Background")]
        [SerializeField] private Image backgroundOverlay;

        [Header("Video Player")]
        [Tooltip("Video oynatılacak panel (açılıp kapanacak)")]
        [SerializeField] private GameObject videoPlayerPanel;

        [Tooltip("VideoPlayer component referansı")]
        [SerializeField] private VideoPlayer videoPlayer;

        [Tooltip("Oynatılacak video clip")]
        [SerializeField] private VideoClip reviveVideoClip;

        [Tooltip("Video render edilecek RawImage (opsiyonel)")]
        [SerializeField] private RawImage videoRawImage;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
            SetupVideoPlayer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupButtons();

            // VideoPlayer event'ini temizle
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }

        private void SetupButtons()
        {
            if (trashButton != null)
            {
                trashButton.AddListener(OnTrashClicked);
            }

            if (reviveVideoButton != null)
            {
                reviveVideoButton.AddListener(OnReviveVideoClicked);
            }

            if (reviveCoinButton != null)
            {
                reviveCoinButton.AddListener(OnReviveCoinClicked);
            }
        }

        private void CleanupButtons()
        {
            if (trashButton != null)
            {
                trashButton.RemoveListener(OnTrashClicked);
            }

            if (reviveVideoButton != null)
            {
                reviveVideoButton.RemoveListener(OnReviveVideoClicked);
            }

            if (reviveCoinButton != null)
            {
                reviveCoinButton.RemoveListener(OnReviveCoinClicked);
            }
        }

        private void SetupVideoPlayer()
        {
            // Video panel'i başlangıçta kapat
            if (videoPlayerPanel != null)
            {
                videoPlayerPanel.SetActive(false);
            }

            // VideoPlayer yoksa component ekle
            if (videoPlayer == null && videoPlayerPanel != null)
            {
                videoPlayer = videoPlayerPanel.GetComponent<VideoPlayer>();
                if (videoPlayer == null)
                {
                    videoPlayer = videoPlayerPanel.AddComponent<VideoPlayer>();
                }
            }

            // VideoPlayer ayarları
            if (videoPlayer != null)
            {
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoPlayer.skipOnDrop = true;

                // RawImage varsa render texture kullan
                if (videoRawImage != null)
                {
                    videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                    RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
                    videoPlayer.targetTexture = renderTexture;
                    videoRawImage.texture = renderTexture;
                }
                else
                {
                    videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                }

                // Video bitince event dinle
                videoPlayer.loopPointReached += OnVideoFinished;

                Debug.Log("[GameOverPopup] VideoPlayer setup complete");
            }
        }

        public void ShowGameOver(int coinsLost, int gemsLost)
        {
            UpdateDisplay(coinsLost, gemsLost);
            Show(animated: true);
        }

        private void UpdateDisplay(int coinsLost, int gemsLost)
        {
            // Title
            if (titleText != null)
            {
                titleText.text = "GAME OVER!";
            }

            // Message
            if (messageText != null)
            {
                messageText.text = "You hit the BOMB!\nAll rewards lost...";
            }

           
            // Total
            if (totalRewardsText != null)
            {
                int total = coinsLost + (gemsLost * 10); // Gem değeri örnek
                totalRewardsText.text = $"Total Value Lost: {total}";
            }
        }

        private void OnTrashClicked()
        {
            Debug.Log("[GameOverPopup] Trash clicked - resetting everything...");

            // 1. RewardManager'da tüm ödülleri sil (LoseAllRewards)
            if (Reward.RewardManager.Instance != null)
            {
                Reward.RewardManager.Instance.LoseAllRewards();
                Debug.Log("[GameOverPopup] All rewards lost (trash)");
            }

            // 2. Zone'u 1'e set et (ZoneManager üzerinden - UI de güncellenir!)
            if (Core.ZoneManager.Instance != null)
            {
                Core.ZoneManager.Instance.ResetToZoneOne();
                Debug.Log("[GameOverPopup] Zone reset to 1 via ZoneManager");
            }

            // 3. Wheel'i regenerate et
            var wheelController = FindObjectOfType<WheelController>();
            if (wheelController != null)
            {
                wheelController.RegenerateWheel();
                Debug.Log("[GameOverPopup] Wheel regenerated");
            }

            // 4. Game restart event'ini tetikle
            GameEvents.TriggerGameRestart();

            // 5. Popup'ı kapat
            Hide(instant: false);

            // 6. ÖNEMLI: autoSpin = FALSE! (Otomatik dönmesin, kullanıcı butona bassın)
            GameEvents.TriggerResultPopupClosed(autoSpin: false);

            Debug.Log("[GameOverPopup] Game reset complete - Zone 1, ready for manual spin");
        }

        private void OnReviveVideoClicked()
        {
            Debug.Log("[GameOverPopup] Revive Video clicked - starting video");

            // BASİT: Sadece panel aç ve video coroutine başlat
            StartCoroutine(PlayReviveVideo());
        }

        private void OnReviveCoinClicked()
        {
            // Demo için altın kontrolü yapmadan direkt devam ettir
            Debug.Log("[GameOverPopup] Coin Revive clicked - continuing without coin deduction (demo mode)");

            // Popup'ı kapat
            Hide(instant: false);

            // Wheel'i regenerate et ve AUTO-SPIN başlat
            GameEvents.TriggerResultPopupClosed(autoSpin: true);
        }

        private System.Collections.IEnumerator PlayReviveVideo()
        {
            // 1. Video panel'i AÇ
            if (videoPlayerPanel != null)
            {
                videoPlayerPanel.SetActive(true);
                Debug.Log("[GameOverPopup] Video panel activated!");
            }
            else
            {
                Debug.LogError("[GameOverPopup] videoPlayerPanel is NULL! Inspector'da assign et!");
                yield break;
            }

            // 2. VideoPlayer kontrolü
            if (videoPlayer == null)
            {
                Debug.LogError("[GameOverPopup] VideoPlayer is NULL!");
                yield return new WaitForSeconds(3f); // Fallback: 3 saniye bekle
                CompleteVideoPlayback();
                yield break;
            }

            // 3. Video clip kontrolü
            if (reviveVideoClip == null)
            {
                Debug.LogError("[GameOverPopup] Video clip is NULL! Inspector'da assign et!");
                yield return new WaitForSeconds(3f); // Fallback: 3 saniye bekle
                CompleteVideoPlayback();
                yield break;
            }

            // 4. Video clip'i ayarla ve hazırla
            videoPlayer.clip = reviveVideoClip;
            videoPlayer.Prepare();

            Debug.Log("[GameOverPopup] Preparing video...");

            // 5. Video hazır olana kadar bekle
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            Debug.Log("[GameOverPopup] Video prepared! Starting playback...");

            // 6. Videoyu OYNAT
            videoPlayer.Play();

            // 7. Video BITENE KADAR BEKLE!
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            Debug.Log("[GameOverPopup] Video finished!");

            // Video bitme eventi de gelecek ama coroutine tamamlandı
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            Debug.Log("[GameOverPopup] OnVideoFinished event triggered");
            CompleteVideoPlayback();
        }

        private void CompleteVideoPlayback()
        {
            Debug.Log("[GameOverPopup] Completing video playback...");

            // 1. Video panel'i KAPAT
            if (videoPlayerPanel != null)
            {
                videoPlayerPanel.SetActive(false);
                Debug.Log("[GameOverPopup] Video panel deactivated!");
            }

            // 2. VideoPlayer'ı durdur
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            // 3. GameOver popup'ı kapat
            Hide(instant: false);

            // 4. Wheel'i regenerate et ve AUTO-SPIN başlat
            GameEvents.TriggerResultPopupClosed(autoSpin: true);

            Debug.Log("[GameOverPopup] Reviving player...");
        }

        protected override void AnimateShow()
        {
            // Background fade
            if (backgroundOverlay != null)
            {
                StartCoroutine(FadeBackground(0f, 0.9f));
            }

            // Base fallback animation kullan
            base.AnimateShow();
        }

        private System.Collections.IEnumerator FadeBackground(float from, float to)
        {
            float elapsed = 0f;
            Color color = backgroundOverlay.color;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / animationDuration);
                color.a = alpha;
                backgroundOverlay.color = color;
                yield return null;
            }

            color.a = to;
            backgroundOverlay.color = color;
        }

    }
}
