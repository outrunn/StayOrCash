using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PokerWar.Managers;
using PokerWar.Systems;
using PokerWar.Interfaces;
using PokerWar.Data;
using PokerWar.World;

namespace PokerWar.UI
{
    /// <summary>
    /// Main UI controller that subscribes to game events and updates displays.
    /// Uses event-driven architecture for clean separation of concerns.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI runText;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI findChestText;

        [Header("Panels")]
        [SerializeField] private GameObject cashOutPanel;
        [SerializeField] private TextMeshProUGUI cashOutScoreText;
        [SerializeField] private TextMeshProUGUI nextRunInfoText;
        [SerializeField] private UnityEngine.UI.Button continueButton;
        [SerializeField] private UnityEngine.UI.Button cashOutButton;

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;

        [Header("Timer Color Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color dangerColor = Color.red;
        [SerializeField] private float warningThreshold = 20f;
        [SerializeField] private float dangerThreshold = 10f;

        [Header("Prompt Settings")]
        [SerializeField] private float findChestPromptDuration = 3f;

        private TimeManager timeManager;
        private ScoreManager scoreManager;
        private PokerWar.Player.FirstPersonController playerController;

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameUI: GameManager.Instance is null!");
                return;
            }

            timeManager = GameManager.Instance.TimeManager;
            scoreManager = GameManager.Instance.ScoreManager;

            SetupButtons();
            SubscribeToEvents();
            InitializeUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnTimeUpdate += UpdateTimerDisplay;
                timeManager.OnTimeUp += HandleTimeUp;
            }

            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += UpdateScoreDisplay;
                scoreManager.OnRunChanged += UpdateRunDisplay;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (timeManager != null)
            {
                timeManager.OnTimeUpdate -= UpdateTimerDisplay;
                timeManager.OnTimeUp -= HandleTimeUp;
            }

            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= UpdateScoreDisplay;
                scoreManager.OnRunChanged -= UpdateRunDisplay;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }

            // Unsubscribe from buttons
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
            if (cashOutButton != null)
                cashOutButton.onClick.RemoveListener(OnCashOutClicked);
        }

        private void SetupButtons()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
            else
            {
                Debug.LogWarning("GameUI: Continue button not assigned!");
            }

            if (cashOutButton != null)
            {
                cashOutButton.onClick.AddListener(OnCashOutClicked);
            }
            else
            {
                Debug.LogWarning("GameUI: Cash out button not assigned!");
            }
        }

        private void InitializeUI()
        {
            if (cashOutPanel != null) cashOutPanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (promptText != null) promptText.gameObject.SetActive(false);
            if (findChestText != null) findChestText.gameObject.SetActive(false);
        }

        /// <summary>
        /// Finds and caches the player's FirstPersonController.
        /// Called dynamically since player is spawned at runtime.
        /// </summary>
        private void FindPlayerController()
        {
            if (playerController == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerController = player.GetComponent<PokerWar.Player.FirstPersonController>();
                }
            }
        }

        private void Update()
        {
            UpdatePrompt();
        }

        #region HUD Updates

        private void UpdateTimerDisplay(float timeRemaining)
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);

            timerText.text = $"Time: {minutes:00}:{seconds:00}";

            // Update color based on remaining time
            if (timeRemaining < dangerThreshold)
                timerText.color = dangerColor;
            else if (timeRemaining < warningThreshold)
                timerText.color = warningColor;
            else
                timerText.color = normalColor;
        }

        private void UpdateScoreDisplay(int newScore, int runScore)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {newScore}";
            }
        }

        private void UpdateRunDisplay(int newRun)
        {
            if (runText != null)
            {
                runText.text = $"Run: {newRun}";
            }
        }

        private void UpdatePrompt()
        {
            if (promptText == null) return;

            // Check if near an interactable
            IInteractable nearestInteractable = FindNearestInteractable();

            if (nearestInteractable != null && nearestInteractable.IsInRange())
            {
                promptText.text = nearestInteractable.GetInteractionPrompt();
                promptText.gameObject.SetActive(true);
            }
            else
            {
                promptText.gameObject.SetActive(false);
            }
        }

        private IInteractable FindNearestInteractable()
        {
            if (GameManager.Instance == null) return null;

            var worldGen = GameManager.Instance.WorldGenerator as PokerWar.World.ProceduralWorldGenerator;
            if (worldGen == null) return null;

            GameObject chest = worldGen.GetCurrentChest();
            if (chest != null)
            {
                return chest.GetComponent<IInteractable>();
            }

            return null;
        }

        #endregion

        #region Event Handlers

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.ChestCollected:
                    ShowCashOutPanel();
                    break;

                case GameState.Playing:
                    HideCashOutPanel();
                    HideGameOver();
                    ShowFindChestPrompt();
                    break;

                case GameState.TimeUp:
                    ShowGameOver("TIME'S UP!\nYou lost everything!");
                    break;

                case GameState.CashedOut:
                    var stats = scoreManager.GetScoreStats();
                    ShowGameOver($"CASHED OUT!\nFinal Score: {stats.totalScore}\nRuns Completed: {stats.currentRun}");
                    break;
            }
        }

        private void HandleTimeUp()
        {
            // Handled by game state change
        }

        #endregion

        #region Panel Management

        private void ShowFindChestPrompt()
        {
            if (findChestText == null) return;

            findChestText.text = "Find the chest!";
            findChestText.gameObject.SetActive(true);

            // Auto-hide after duration
            Invoke(nameof(HideFindChestPrompt), findChestPromptDuration);
        }

        private void HideFindChestPrompt()
        {
            if (findChestText != null)
            {
                findChestText.gameObject.SetActive(false);
            }
        }

        private void ShowCashOutPanel()
        {
            if (cashOutPanel == null) return;

            var stats = scoreManager.GetScoreStats();
            int nextRun = stats.currentRun + 1;
            float nextTimeLimit = timeManager != null && GameManager.Instance.GameConfig != null ?
                GameManager.Instance.GameConfig.CalculateTimeLimit(nextRun) : 0f;

            cashOutPanel.SetActive(true);

            if (cashOutScoreText != null)
            {
                cashOutScoreText.text = $"Current Score: {stats.totalScore}";
            }

            if (nextRunInfoText != null)
            {
                nextRunInfoText.text = $"Next Run: {nextRun}\nTime Limit: {nextTimeLimit:F0}s";
            }

            // Disable player controller and show cursor
            FindPlayerController();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HideCashOutPanel()
        {
            if (cashOutPanel != null)
            {
                cashOutPanel.SetActive(false);
            }

            // Re-enable player controller and lock cursor
            FindPlayerController();
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ShowGameOver(string message)
        {
            if (gameOverPanel == null) return;

            gameOverPanel.SetActive(true);

            if (gameOverText != null)
            {
                gameOverText.text = message;
            }

            // Disable player controller and show cursor
            FindPlayerController();
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HideGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Re-enable player controller when hiding game over
            FindPlayerController();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }

        #endregion

        #region UI Button Callbacks

        public void OnContinueClicked()
        {
            HideCashOutPanel();
            GameManager.Instance?.ContinueToNextRun();
        }

        public void OnCashOutClicked()
        {
            HideCashOutPanel();
            GameManager.Instance?.CashOut();
        }

        public void OnRestartClicked()
        {
            GameManager.Instance?.RestartGame();
        }

        #endregion
    }
}
