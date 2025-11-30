using UnityEngine;
using UnityEngine.Video;
using System;

namespace WheelOfFortune.UI.Popups
{
    [RequireComponent(typeof(VideoPlayer))]
    public class ReviveVideoPlayer : MonoBehaviour
    {

        [Header("Video Settings")]
        [Tooltip("Oynatılacak video clip (Resources/Videos/ klasöründen)")]
        [SerializeField] private VideoClip reviveVideoClip;

        [Tooltip("Video URL'i (alternatif olarak web'den video oynatmak için)")]
        [SerializeField] private string videoURL;

        [Tooltip("Video bittikten sonra kaç saniye beklenecek")]
        [SerializeField] private float postVideoDelay = 0.5f;

        [Header("UI References")]
        [Tooltip("Video render edilecek RawImage (opsiyonel)")]
        [SerializeField] private UnityEngine.UI.RawImage videoDisplay;

        [Tooltip("Video oynarken gösterilecek panel")]
        [SerializeField] private GameObject videoPanel;

        [Tooltip("Skip butonu (opsiyonel - videonun %80'i izlendikten sonra aktif olur)")]
        [SerializeField] private UnityEngine.UI.Button skipButton;

        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;

        private VideoPlayer videoPlayer;
        private Action onVideoComplete;
        private bool isPlaying;
        private float minimumWatchPercentage = 0.8f; // %80 izleme zorunluluğu

        public bool IsPlaying => isPlaying;

        private void Awake()
        {
            // VideoPlayer component'ini al
            videoPlayer = GetComponent<VideoPlayer>();

            if (videoPlayer == null)
            {
                Debug.LogWarning("[ReviveVideoPlayer] VideoPlayer component not found, adding it...");
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            SetupVideoPlayer();

            // Video panel'i başlangıçta kapat
            if (videoPanel != null)
            {
                videoPanel.SetActive(false);
                LogDebug("Video panel initialized (inactive)");
            }
        }

        private void Start()
        {
            // Debug: Video clip assignment kontrolü
            LogDebug("=== ReviveVideoPlayer Start Debug ===");
            LogDebug($"Video Clip Assigned: {reviveVideoClip != null} - {(reviveVideoClip != null ? reviveVideoClip.name : "NULL")}");
            LogDebug($"Video URL: '{videoURL}'");
            LogDebug($"Video Panel: {videoPanel != null}");
            LogDebug($"Video Display: {videoDisplay != null}");
            LogDebug($"VideoPlayer Component: {videoPlayer != null}");
            LogDebug("=====================================");
        }

        private void OnEnable()
        {
            // Video eventi dinle
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached += OnVideoFinished;
            }
        }

        private void OnDisable()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoFinished;
            }
        }

        private void SetupVideoPlayer()
        {
            if (videoPlayer == null) return;

            // Temel ayarlar
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.skipOnDrop = true;

            // Render mode ayarla
            if (videoDisplay != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;

                // RenderTexture oluştur
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
                videoPlayer.targetTexture = renderTexture;
                videoDisplay.texture = renderTexture;
            }
            else
            {
                // Direct render
                videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            }

            // Skip butonunu başlangıçta devre dışı bırak
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
                skipButton.onClick.AddListener(OnSkipClicked);
            }

            LogDebug("ReviveVideoPlayer configured");
        }

        /// <param name="onComplete">Video bitince çağrılacak callback</param>
        public void PlayReviveVideo(Action onComplete)
        {
            // ÖNCE: GameObject'in aktif olduğundan emin ol
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[ReviveVideoPlayer] GameObject is inactive! Activating...");
                gameObject.SetActive(true);
            }

            if (isPlaying)
            {
                LogDebug("Video already playing!");
                return;
            }

            onVideoComplete = onComplete;

            // Video panel'i ÖNCE göster
            if (videoPanel != null)
            {
                videoPanel.SetActive(true);
                LogDebug("Video panel activated");
            }
            else
            {
                Debug.LogWarning("[ReviveVideoPlayer] Video panel is not assigned!");
            }

            // VideoPlayer kontrolü
            if (videoPlayer == null)
            {
                Debug.LogError("[ReviveVideoPlayer] VideoPlayer component is null!");
                StartCoroutine(FallbackVideoSimulation());
                return;
            }

            // Video source'u ayarla
            if (reviveVideoClip != null)
            {
                videoPlayer.source = VideoSource.VideoClip;
                videoPlayer.clip = reviveVideoClip;
                LogDebug($"Playing video clip: {reviveVideoClip.name}");
            }
            else if (!string.IsNullOrEmpty(videoURL))
            {
                videoPlayer.source = VideoSource.Url;
                videoPlayer.url = videoURL;
                LogDebug($"Playing video from URL: {videoURL}");
            }
            else
            {
                Debug.LogError("[ReviveVideoPlayer] No video clip or URL assigned!");
                Debug.LogError($"[ReviveVideoPlayer] reviveVideoClip = {reviveVideoClip}, videoURL = '{videoURL}'");

                // Fallback: 3 saniye bekle ve bitir
                StartCoroutine(FallbackVideoSimulation());
                return;
            }

            // Videoyu başlat
            videoPlayer.Prepare();
            videoPlayer.prepareCompleted += OnVideoPrepared;

            isPlaying = true;
        }

        public void StopVideo()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            isPlaying = false;

            if (videoPanel != null)
            {
                videoPanel.SetActive(false);
            }

            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
            }
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            LogDebug("Video prepared, starting playback");

            source.prepareCompleted -= OnVideoPrepared;
            source.Play();

            // Skip kontrolü başlat
            if (skipButton != null)
            {
                StartCoroutine(MonitorVideoProgress());
            }
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            LogDebug("Video finished");

            // Panel'i kapat
            if (videoPanel != null)
            {
                videoPanel.SetActive(false);
            }

            isPlaying = false;

            // Kısa bir delay sonra callback çağır
            StartCoroutine(DelayedCallback());
        }

        private void OnSkipClicked()
        {
            // Minimum izleme yüzdesini kontrol et
            if (videoPlayer != null)
            {
                double watchedPercentage = videoPlayer.time / videoPlayer.length;

                if (watchedPercentage >= minimumWatchPercentage)
                {
                    LogDebug($"Skip clicked - {watchedPercentage:P0} watched");
                    StopVideo();
                    onVideoComplete?.Invoke();
                }
                else
                {
                    LogDebug($"Cannot skip yet - only {watchedPercentage:P0} watched (minimum: {minimumWatchPercentage:P0})");
                }
            }
        }

        private System.Collections.IEnumerator MonitorVideoProgress()
        {
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(false);
            }

            while (isPlaying && videoPlayer.isPlaying)
            {
                double watchedPercentage = videoPlayer.time / videoPlayer.length;

                // %80 izlendiyse skip butonunu aktif et
                if (watchedPercentage >= minimumWatchPercentage && skipButton != null)
                {
                    skipButton.gameObject.SetActive(true);
                }

                yield return null;
            }
        }

        private System.Collections.IEnumerator DelayedCallback()
        {
            yield return new WaitForSeconds(postVideoDelay);

            onVideoComplete?.Invoke();
            onVideoComplete = null;
        }

        private System.Collections.IEnumerator FallbackVideoSimulation()
        {
            LogDebug("Using fallback video simulation (3 seconds)");

            if (videoPanel != null)
            {
                videoPanel.SetActive(true);
            }

            isPlaying = true;
            yield return new WaitForSeconds(3f);

            if (videoPanel != null)
            {
                videoPanel.SetActive(false);
            }

            isPlaying = false;
            onVideoComplete?.Invoke();
            onVideoComplete = null;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ReviveVideoPlayer] {message}");
            }
        }

    }
}
