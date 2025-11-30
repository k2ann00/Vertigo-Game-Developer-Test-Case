using UnityEngine;
using WheelOfFortune.Events;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AudioManager");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1f;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Wheel Audio Clips")]
        [Tooltip("Wheel spin sırasında çalan ses (tek seferlik)")]
        [SerializeField] private AudioClip wheelSpinSound;

        [Tooltip("Wheel durduğunda ve sonuç gösterildiğinde çalan ses")]
        [SerializeField] private AudioClip wheelResultSound;

        [Header("Reward Audio Clips")]
        [SerializeField] private AudioClip rewardCollectedSound;
        [SerializeField] private AudioClip bombHitSound;

        [Header("Zone Audio Clips")]
        [SerializeField] private AudioClip safeZoneSound;
        [SerializeField] private AudioClip superZoneSound;

        [Header("Background Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private bool playMusicOnStart = true;

        private bool isMusicMuted = false;

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

            InitializeAudio();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeAudio()
        {
            if (musicSource == null)
            {
                GameObject musicGo = new GameObject("MusicSource");
                musicGo.transform.SetParent(transform);
                musicSource = musicGo.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxGo = new GameObject("SFXSource");
                sfxGo.transform.SetParent(transform);
                sfxSource = sfxGo.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            UpdateVolumes();
            LogDebug("AudioManager initialized");

            // Background müziği başlat
            if (playMusicOnStart && backgroundMusic != null)
            {
                PlayMusic(backgroundMusic);
                LogDebug("Background music started");
            }
        }

        // Event Subscriptions
        private void SubscribeToEvents()
        {
            GameEvents.OnWheelSpinStarted += HandleWheelSpinStarted;
            GameEvents.OnWheelSpinCompleted += HandleWheelSpinCompleted;
            GameEvents.OnRewardCollected += HandleRewardCollected;
            GameEvents.OnBombHit += HandleBombHit;
            GameEvents.OnSafeZoneEntered += HandleSafeZoneEntered;
            GameEvents.OnSuperZoneEntered += HandleSuperZoneEntered;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnWheelSpinStarted -= HandleWheelSpinStarted;
            GameEvents.OnWheelSpinCompleted -= HandleWheelSpinCompleted;
            GameEvents.OnRewardCollected -= HandleRewardCollected;
            GameEvents.OnBombHit -= HandleBombHit;
            GameEvents.OnSafeZoneEntered -= HandleSafeZoneEntered;
            GameEvents.OnSuperZoneEntered -= HandleSuperZoneEntered;
        }

        private void HandleWheelSpinStarted()
        {
            PlaySFX(wheelSpinSound);
            LogDebug("Wheel spin sound played");
        }

        private void HandleWheelSpinCompleted(int sliceIndex)
        {
            PlaySFX(wheelResultSound);
            LogDebug($"Wheel result sound played for slice {sliceIndex}");
        }

        private void HandleRewardCollected(RewardData reward)
        {
            LogDebug($"Playing reward collected sound for {reward.RewardName}");
            PlaySFX(rewardCollectedSound);
        }

        private void HandleBombHit()
        {
            PlaySFX(bombHitSound);
            LogDebug("Bomb explosion sound played");
        }

        private void HandleSafeZoneEntered(int zoneNumber)
        {
            PlaySFX(safeZoneSound);
            LogDebug($"Safe Zone sound played for Zone {zoneNumber}");
        }

        private void HandleSuperZoneEntered(int zoneNumber)
        {
            PlaySFX(superZoneSound);
            LogDebug($"Super Zone sound played for Zone {zoneNumber}");
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || sfxSource == null)
                return;

            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource == null)
                return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        private void UpdateVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume * masterVolume;
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public float GetMasterVolume()
        {
            return masterVolume;
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void ToggleMusic()
        {
            isMusicMuted = !isMusicMuted;

            if (musicSource != null)
            {
                if (isMusicMuted)
                {
                    musicSource.Pause();
                    LogDebug("Music muted");
                }
                else
                {
                    if (musicSource.clip != null)
                    {
                        musicSource.UnPause();
                    }
                    else if (backgroundMusic != null)
                    {
                        PlayMusic(backgroundMusic);
                    }
                    LogDebug("Music unmuted");
                }
            }
        }

        public bool IsMusicMuted()
        {
            return isMusicMuted;
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[AudioManager] {message}");
            }
        }
    }
}
