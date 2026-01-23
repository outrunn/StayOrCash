using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PokerWar.Systems;

namespace PokerWar.UI
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
            // Find or create PersistentDataManager
            dataManager = FindObjectOfType<PersistentDataManager>();
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
            SceneManager.LoadScene(gameSceneName);
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
