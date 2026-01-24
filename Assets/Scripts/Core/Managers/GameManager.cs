using UnityEngine;
using UnityEngine.SceneManagement;
using StayOrCash.Systems;
using StayOrCash.Data;
using StayOrCash.Interfaces;
using StayOrCash.World;

namespace StayOrCash.Managers
{
    /// <summary>
    /// Main game manager that orchestrates all game systems.
    /// Uses a modular architecture with separated concerns.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private GameConfig gameConfig;

        [Header("System References")]
        [SerializeField] private TimeManager timeManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private ProceduralWorldGenerator worldGenerator;
        [SerializeField] private PersistentDataManager dataManager;

        [Header("Player")]
        [SerializeField] private GameObject playerPrefab;

        [Header("Menu Settings")]
        [SerializeField] private string menuSceneName = "MainMenu";

        private GameObject playerInstance;
        private GameState currentState = GameState.Idle;

        public GameState CurrentState => currentState;
        public GameConfig GameConfig => gameConfig;
        public TimeManager TimeManager => timeManager;
        public ScoreManager ScoreManager => scoreManager;
        public IWorldGenerator WorldGenerator => worldGenerator;
        public PersistentDataManager DataManager => dataManager;

        // Events
        public delegate void GameStateChangedEvent(GameState newState);
        public event GameStateChangedEvent OnGameStateChanged;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            ValidateReferences();
            SubscribeToEvents();
        }

        private void Start()
        {
            // Auto-start game when this scene loads (from menu Play button)
            // Check if we're in the game scene (not menu scene)
            string currentScene = SceneManager.GetActiveScene().name;

            if (currentScene != menuSceneName && currentScene != "MainMenu")
            {
                // We're in the game scene, start playing
                Debug.Log("GameManager: Auto-starting game from scene load");
                StartGameFromMenu();
            }
            else
            {
                // We're in menu scene, just idle
                ChangeState(GameState.Idle);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ValidateReferences()
        {
            if (gameConfig == null)
            {
                Debug.LogError("GameManager: GameConfig not assigned! Create one via Assets -> Create -> Stay or Cash -> Game Config");
            }

            if (timeManager == null)
            {
                Debug.LogError("GameManager: TimeManager not assigned!");
            }

            if (scoreManager == null)
            {
                Debug.LogError("GameManager: ScoreManager not assigned!");
            }

            if (worldGenerator == null)
            {
                Debug.LogError("GameManager: ProceduralWorldGenerator not assigned!");
            }

            if (playerPrefab == null)
            {
                Debug.LogError("GameManager: Player prefab not assigned!");
            }
        }

        private void SubscribeToEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnTimeUp += HandleTimeUp;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnTimeUp -= HandleTimeUp;
            }
        }

        #region Game Flow

        /// <summary>
        /// Called from main menu when player clicks Play button.
        /// Starts a fresh game session.
        /// </summary>
        public void StartGameFromMenu()
        {
            Debug.Log("Starting game from menu...");
            StartNewGame();
        }

        /// <summary>
        /// Start a completely new game session.
        /// </summary>
        public void StartNewGame()
        {
            scoreManager.ResetGame();
            ChangeState(GameState.Starting);
            StartNewRun();
        }

        /// <summary>
        /// Start a new run with increased difficulty.
        /// </summary>
        public void StartNewRun()
        {
            scoreManager.StartNewRun();

            // Calculate time limit
            int runNumber = scoreManager.CurrentRun;

            // Generate new world
            int seed = Random.Range(0, 100000);
            worldGenerator.GenerateWorld(seed);

            // Wait for terrain to settle, then spawn player and start timer
            Invoke(nameof(InitializeRun), gameConfig != null ? gameConfig.playerSpawnDelay : 0.1f);

            ChangeState(GameState.Playing);

            Debug.Log($"Run {runNumber} started!");
        }

        private void InitializeRun()
        {
            SpawnPlayer();
            timeManager.StartTimer(scoreManager.CurrentRun);
        }

        private void SpawnPlayer()
        {
            // Clean up old player
            if (playerInstance != null)
            {
                Destroy(playerInstance);
            }

            // Get spawn position from world generator
            Vector3 spawnPos = worldGenerator.GetCenterSpawnPosition();

            // Spawn player
            if (playerPrefab != null)
            {
                playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                playerInstance.name = "Player";
            }
            else
            {
                Debug.LogError("GameManager: Cannot spawn player - prefab not assigned!");
            }

            // Set player reference in chest
            GameObject chest = worldGenerator.GetCurrentChest();
            if (chest != null)
            {
                var chestInteractable = chest.GetComponent<IInteractable>();
                if (chestInteractable != null && playerInstance != null)
                {
                    // Chest will handle player reference internally
                    var chestScript = chest.GetComponent<ChestInteractable>();
                    chestScript?.SetPlayerReference(playerInstance.transform);
                }
            }
        }

        /// <summary>
        /// Called when the player collects a chest.
        /// </summary>
        public void OnChestCollected()
        {
            if (currentState != GameState.Playing) return;

            // Pause timer
            timeManager.PauseTimer();

            // Calculate and add score
            int runScore = scoreManager.AddChestScore(timeManager.CurrentTime);

            // Change state to decision point
            ChangeState(GameState.ChestCollected);

            Debug.Log($"Chest collected! Run score: {runScore}, Total: {scoreManager.TotalScore}");

            // For now, auto-show cash-out prompt
            // In full version, UI will handle this
            ShowCashOutPrompt();
        }

        private void ShowCashOutPrompt()
        {
            Debug.Log("Cash-out decision point reached. Press 'C' to cash out or 'Space' to continue.");
            // UI system will handle this in the full implementation
        }

        /// <summary>
        /// Continue to the next run (player chose not to cash out).
        /// </summary>
        public void ContinueToNextRun()
        {
            Debug.Log("Continuing to next run...");
            ChangeState(GameState.Transitioning);
            StartNewRun();
        }

        /// <summary>
        /// Cash out and end the game with current score.
        /// Awards cash to player based on score.
        /// </summary>
        public void CashOut()
        {
            int finalScore = scoreManager.TotalScore;
            int runsCompleted = scoreManager.CurrentRun;

            // Award cash (score = cash for simplicity)
            if (dataManager != null)
            {
                dataManager.AddCash(finalScore);
                dataManager.UpdateHighScore(finalScore);
                Debug.Log($"Awarded ${finalScore} cash!");
            }

            Debug.Log($"Cashed out! Final score: {finalScore}, Runs completed: {runsCompleted}");

            ChangeState(GameState.CashedOut);
            ShowFinalScore();
        }

        private void HandleTimeUp()
        {
            Debug.Log("Time's up! All rewards lost!");

            ChangeState(GameState.TimeUp);
            scoreManager.ResetScore();
            ShowGameOver();
        }

        private void ShowFinalScore()
        {
            var stats = scoreManager.GetScoreStats();
            Debug.Log($"=== GAME COMPLETE ===\nFinal Score: {stats.totalScore}\nRuns Completed: {stats.currentRun}");

            // Return to main menu after showing final score
            if (gameConfig != null)
            {
                Invoke(nameof(LoadMainMenu), gameConfig.cashOutRestartDelay);
            }
            else
            {
                Invoke(nameof(LoadMainMenu), 3f);
            }
        }

        private void ShowGameOver()
        {
            int runsCompleted = scoreManager.CurrentRun - 1;
            Debug.Log($"=== GAME OVER ===\nYou lost everything!\nRuns Completed: {runsCompleted}");

            // Return to main menu after game over
            if (gameConfig != null)
            {
                Invoke(nameof(LoadMainMenu), gameConfig.gameOverRestartDelay);
            }
            else
            {
                Invoke(nameof(LoadMainMenu), 3f);
            }
        }

        /// <summary>
        /// Restart the entire game by reloading the scene.
        /// </summary>
        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Load the main menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            Debug.Log("Loading main menu...");
            ChangeState(GameState.InMenu);
            SceneManager.LoadScene(menuSceneName);
        }

        #endregion

        #region State Management

        private void ChangeState(GameState newState)
        {
            if (currentState == newState) return;

            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            Debug.Log($"Game state changed to: {newState}");
        }

        #endregion
    }

    /// <summary>
    /// Enum representing the current state of the game.
    /// </summary>
    public enum GameState
    {
        InMenu,
        Idle,
        Starting,
        Playing,
        ChestCollected,
        Transitioning,
        TimeUp,
        CashedOut,
        GameOver
    }
}
