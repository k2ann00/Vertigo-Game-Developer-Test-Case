using UnityEngine;
using WheelOfFortune.Data;
using WheelOfFortune.Events;
using WheelOfFortune.Interfaces;

namespace WheelOfFortune.Core
{
    public class GameManager : MonoBehaviour, IGameState
    {
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("GameManager");
                        instance = go.AddComponent<GameManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Game Settings")]
        [SerializeField] private GameState initialState = GameState.Idle;
        [SerializeField] private bool enableDebugLogs = true;

        private GameState currentState;
        private bool isGameActive;

        public GameState CurrentState => currentState;
        public bool IsGameActive => isGameActive;

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

            InitializeGame();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                GameEvents.ClearAllEvents();
            }
        }

        private void InitializeGame()
        {
            currentState = initialState;
            isGameActive = true;

            LogDebug("GameManager initialized");
        }

        // Event Subscriptions
        private void SubscribeToEvents()
        {
            GameEvents.OnWheelSpinStarted += HandleWheelSpinStarted;
            GameEvents.OnWheelSpinCompleted += HandleWheelSpinCompleted;
            GameEvents.OnBombHit += HandleBombHit;
            GameEvents.OnGameRestart += HandleGameRestart;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnWheelSpinStarted -= HandleWheelSpinStarted;
            GameEvents.OnWheelSpinCompleted -= HandleWheelSpinCompleted;
            GameEvents.OnBombHit -= HandleBombHit;
            GameEvents.OnGameRestart -= HandleGameRestart;
        }

        private void HandleWheelSpinStarted()
        {
            ChangeState(GameState.Spinning);
            LogDebug("Wheel spin started");
        }

        private void HandleWheelSpinCompleted(int sliceIndex)
        {
            ChangeState(GameState.ShowingResult);
            LogDebug($"Wheel spin completed at index: {sliceIndex}");
        }

        private void HandleBombHit()
        {
            ChangeState(GameState.GameOver);
            isGameActive = false;
            LogDebug("Bomb hit! Game Over");
        }

        private void HandleGameRestart()
        {
            RestartGame();
        }

        public void ChangeState(GameState newState)
        {
            if (!CanTransitionTo(newState))
            {
                LogDebug($"Cannot transition from {currentState} to {newState}");
                return;
            }

            GameState previousState = currentState;
            currentState = newState;

            OnStateChanged(previousState, newState);
            LogDebug($"State changed: {previousState} -> {newState}");
        }

        public GameState GetCurrentState()
        {
            return currentState;
        }

        public bool CanTransitionTo(GameState targetState)
        {
            // GameOver'dan sadece Idle'a dönülebilir (restart)
            if (currentState == GameState.GameOver && targetState != GameState.Idle)
                return false;

            // Spinning sırasında sadece ShowingResult veya GameOver olabilir
            if (currentState == GameState.Spinning &&
                targetState != GameState.ShowingResult &&
                targetState != GameState.GameOver)
                return false;

            return true;
        }

        private void OnStateChanged(GameState previousState, GameState newState)
        {
            // State'e göre özel işlemler yapılabilir
            switch (newState)
            {
                case GameState.Idle:
                    // UI'ı aktif et, butonları göster
                    break;

                case GameState.Spinning:
                    // Kullanıcı etkileşimini engelle
                    break;

                case GameState.ShowingResult:
                    // Sonuç popup'ını göster
                    break;

                case GameState.GameOver:
                    // Game Over ekranını göster
                    GameEvents.TriggerGameOver();
                    break;
            }
        }

        public void RestartGame()
        {
            currentState = GameState.Idle;
            isGameActive = true;

            LogDebug("Game restarted");
        }

        public void QuitGame()
        {
            LogDebug("Quitting game");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }
    }
}
