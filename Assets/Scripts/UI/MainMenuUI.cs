using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using StayOrCash.Systems;

namespace StayOrCash.UI
{
    /// <summary>
    /// Simple main menu UI showing player cash and play button.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI cashText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        [Header("Settings")]
        [SerializeField] private string gameSceneName = "mainscene";

        private PersistentDataManager dataManager;

        private void Start()
        {
            // Find or create PersistentDataManager (Unity 6+ API)
            dataManager = FindFirstObjectByType<PersistentDataManager>();
            if (dataManager == null)
            {
                GameObject dataObj = new GameObject("PersistentDataManager");
                dataManager = dataObj.AddComponent<PersistentDataManager>();
                DontDestroyOnLoad(dataObj);
            }

            // Subscribe to events
            dataManager.OnCashChanged += UpdateCashDisplay;

            // Setup buttons
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            // Initial display update
            UpdateDisplay();
        }

        private void OnDestroy()
        {
            if (dataManager != null)
            {
                dataManager.OnCashChanged -= UpdateCashDisplay;
            }

            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void UpdateDisplay()
        {
            UpdateCashDisplay(dataManager.CurrentCash);
            UpdateHighScoreDisplay();
        }

        private void UpdateCashDisplay(int cash)
        {
            if (cashText != null)
            {
                cashText.text = $"Cash: ${cash}";
            }
        }

        private void UpdateHighScoreDisplay()
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"High Score: {dataManager.HighScore}";
            }
        }

        private void OnPlayClicked()
        {
            Debug.Log("Play button clicked - Loading game scene");

            // Disable play button to prevent double-clicks
            if (playButton != null)
            {
                playButton.interactable = false;
            }

            // Use async loading for WebGL compatibility
            StartCoroutine(LoadGameSceneAsync());
        }

        private System.Collections.IEnumerator LoadGameSceneAsync()
        {
            // Load scene asynchronously to prevent WebGL freezing
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName);

            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                // You could update a loading bar here if desired
                // float progress = asyncLoad.progress;
                yield return null;
            }

            Debug.Log("Game scene loaded successfully!");
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quit button clicked");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
