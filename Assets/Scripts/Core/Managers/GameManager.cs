using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using StayOrCash.Systems;
using StayOrCash.Data;
using StayOrCash.Interfaces;
using StayOrCash.World;
using StayOrCash.UI;

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
        [SerializeField] private GameUI gameUI;

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
            Debug.Log($"[WebGL Debug] GameManager.Start() - Current scene: {currentScene}");

            if (currentScene != menuSceneName && currentScene != "MainMenu")
            {
                // We're in the game scene, start playing
                Debug.Log("[WebGL Debug] Auto-starting game from scene load");

                // Give one frame for all managers to finish their Start() methods
                StartCoroutine(StartGameDelayed());
            }
            else
            {
                // We're in menu scene, just idle
                Debug.Log("[WebGL Debug] In menu scene, staying idle");
                ChangeState(GameState.Idle);
            }
        }

        /// <summary>
        /// Delayed game start to ensure all systems are initialized.
        /// Critical for WebGL builds where timing is sensitive.
        /// </summary>
        private System.Collections.IEnumerator StartGameDelayed()
        {
            // Wait one frame to ensure all Start() methods have executed
            yield return null;

            Debug.Log("[WebGL Debug] Starting game after initialization delay");
            StartGameFromMenu();
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
            bool allValid = true;

            if (gameConfig == null)
            {
                Debug.LogError("GameManager: GameConfig not assigned! Create one via Assets -> Create -> Stay or Cash -> Game Config");
                allValid = false;
            }

            if (timeManager == null)
            {
                Debug.LogError("GameManager: TimeManager not assigned!");
                allValid = false;
            }

            if (scoreManager == null)
            {
                Debug.LogError("GameManager: ScoreManager not assigned!");
                allValid = false;
            }

            if (worldGenerator == null)
            {
                Debug.LogError("GameManager: ProceduralWorldGenerator not assigned!");
                allValid = false;
            }

            if (playerPrefab == null)
            {
                Debug.LogError("GameManager: Player prefab not assigned!");
                allValid = false;
            }

            if (gameUI == null)
            {
                Debug.LogWarning("GameManager: GameUI not assigned - loading screen will not be shown!");
            }

            if (!allValid)
            {
                Debug.LogError("[CRITICAL] GameManager has missing references! Game may not work correctly in WebGL build!");
            }
            else
            {
                Debug.Log("[WebGL Debug] All GameManager references validated successfully");
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
            // Safety check - prevent starting if critical references are missing
            if (scoreManager == null || timeManager == null || worldGenerator == null)
            {
                Debug.LogError("[CRITICAL] Cannot start game - missing manager references! Check Inspector.");
                return;
            }

            Debug.Log("[WebGL Debug] Starting new game");
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
            int runNumber = scoreManager.CurrentRun;

            // Start async world generation with loading screen
            StartCoroutine(GenerateWorldWithLoading(runNumber));
        }

        /// <summary>
        /// Coroutine that generates the world asynchronously with a loading screen.
        /// </summary>
        private IEnumerator GenerateWorldWithLoading(int runNumber)
        {
            Debug.Log($"[WebGL Debug] Starting world generation for run {runNumber}");

            // Ensure UI system is ready
            if (gameUI == null)
            {
                Debug.LogWarning("[WebGL Debug] GameUI is null! Loading screen will not be shown.");
            }

            // Show loading screen
            if (gameUI != null)
            {
                gameUI.ShowLoadingScreen("Generating world...", 0f);
            }

            // Give one frame for UI to update
            yield return null;

            // Generate new world asynchronously
            int seed = Random.Range(0, 100000);
            Debug.Log($"[WebGL Debug] Starting world generation with seed {seed}");

            yield return StartCoroutine(worldGenerator.GenerateWorldAsync(seed, (progress, message) =>
            {
                // Update loading screen with progress bar
                if (gameUI != null)
                {
                    gameUI.ShowLoadingScreen(message, progress);
                }
                Debug.Log($"[WebGL Debug] Generation progress: {progress * 100:F0}% - {message}");
            }));

            Debug.Log("[WebGL Debug] World generation complete");

            // Wait for terrain physics and textures to fully settle
            if (gameUI != null)
            {
                gameUI.ShowLoadingScreen("Finalizing terrain...", 0.95f);
            }

            // Multiple smaller yields for WebGL compatibility (prevents browser freezing)
            for (int i = 0; i < 10; i++)
            {
                yield return null; // Yield every frame instead of one big wait
            }

            // Spawn player (but camera will be disabled)
            if (gameUI != null)
            {
                gameUI.ShowLoadingScreen("Preparing player...", 0.98f);
            }

            Debug.Log("[WebGL Debug] Spawning player");
            SpawnPlayer();

            // Yield a few frames instead of WaitForSeconds for WebGL
            for (int i = 0; i < 3; i++)
            {
                yield return null;
            }
            Debug.Log("[WebGL Debug] Player spawned");

            // Start timer
            Debug.Log("[WebGL Debug] Starting timer");
            timeManager.StartTimer(scoreManager.CurrentRun);

            // Give a few more frames for everything to be ready
            for (int i = 0; i < 3; i++)
            {
                yield return null;
            }

            // Enable player camera NOW that everything is loaded
            Debug.Log("[WebGL Debug] Enabling player camera");
            EnablePlayerCamera();

            // Show 100% complete briefly before hiding
            if (gameUI != null)
            {
                gameUI.ShowLoadingScreen("Ready!", 1.0f);
            }

            // Brief pause before hiding loading screen (using frames instead of time)
            for (int i = 0; i < 10; i++)
            {
                yield return null;
            }

            // Hide loading screen and show the fully loaded scene
            Debug.Log("[WebGL Debug] Hiding loading screen");
            if (gameUI != null)
            {
                gameUI.HideLoadingScreen();
            }

            ChangeState(GameState.Playing);

            Debug.Log($"[WebGL Debug] Run {runNumber} started successfully!");
        }

        private void InitializeRun()
        {
            // Legacy method - no longer used (kept for compatibility)
            // Player spawning is now handled in GenerateWorldWithLoading
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

                // Disable camera initially to prevent seeing the unloaded world
                Camera playerCamera = playerInstance.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    playerCamera.enabled = false;
                    Debug.Log("Player camera disabled during world loading");
                }
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
        /// Enable the player camera after world is fully loaded.
        /// </summary>
        private void EnablePlayerCamera()
        {
            if (playerInstance != null)
            {
                Camera playerCamera = playerInstance.GetComponentInChildren<Camera>();
                if (playerCamera != null)
                {
                    playerCamera.enabled = true;
                    Debug.Log("Player camera enabled - world fully loaded!");
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
