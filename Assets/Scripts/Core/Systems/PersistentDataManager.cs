using UnityEngine;

namespace StayOrCash.Systems
{
    /// <summary>
    /// Manages persistent player data (cash, high scores, etc.) using PlayerPrefs.
    /// Simple implementation for 2-hour game jam scope.
    /// </summary>
    public class PersistentDataManager : MonoBehaviour
    {
        private const string CASH_KEY = "PlayerCash";
        private const string HIGH_SCORE_KEY = "HighScore";
        private const int DEFAULT_STARTING_CASH = 1000;

        private int currentCash;
        private int highScore;

        public int CurrentCash => currentCash;
        public int HighScore => highScore;

        // Events
        public delegate void CashChangedEvent(int newAmount);
        public event CashChangedEvent OnCashChanged;

        private void Awake()
        {
            LoadData();
        }

        /// <summary>
        /// Load player data from PlayerPrefs.
        /// </summary>
        public void LoadData()
        {
            currentCash = PlayerPrefs.GetInt(CASH_KEY, DEFAULT_STARTING_CASH);
            highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

            Debug.Log($"Loaded data - Cash: {currentCash}, High Score: {highScore}");
        }

        /// <summary>
        /// Save player data to PlayerPrefs.
        /// </summary>
        public void SaveData()
        {
            PlayerPrefs.SetInt(CASH_KEY, currentCash);
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();

            Debug.Log($"Saved data - Cash: {currentCash}, High Score: {highScore}");
        }

        /// <summary>
        /// Add cash to player's account.
        /// </summary>
        public void AddCash(int amount)
        {
            currentCash += amount;
            SaveData();
            OnCashChanged?.Invoke(currentCash);

            Debug.Log($"Added {amount} cash. New total: {currentCash}");
        }

        /// <summary>
        /// Remove cash from player's account.
        /// Returns false if insufficient funds.
        /// </summary>
        public bool SpendCash(int amount)
        {
            if (currentCash < amount)
            {
                Debug.LogWarning($"Insufficient cash! Need {amount}, have {currentCash}");
                return false;
            }

            currentCash -= amount;
            SaveData();
            OnCashChanged?.Invoke(currentCash);

            Debug.Log($"Spent {amount} cash. Remaining: {currentCash}");
            return true;
        }

        /// <summary>
        /// Update high score if current score is higher.
        /// </summary>
        public void UpdateHighScore(int score)
        {
            if (score > highScore)
            {
                highScore = score;
                SaveData();
                Debug.Log($"New high score: {highScore}!");
            }
        }

        /// <summary>
        /// Reset all player data (for debugging).
        /// </summary>
        public void ResetData()
        {
            currentCash = DEFAULT_STARTING_CASH;
            highScore = 0;
            SaveData();
            OnCashChanged?.Invoke(currentCash);

            Debug.Log("Player data reset to defaults");
        }
    }
}
