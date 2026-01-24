using UnityEngine;
using StayOrCash.Data;

namespace StayOrCash.Systems
{
    /// <summary>
    /// Manages player score and run tracking.
    /// Separated from GameManager for single responsibility.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;

        private int currentRun;
        private int totalScore;
        private int currentRunScore;

        public int CurrentRun => currentRun;
        public int TotalScore => totalScore;
        public int CurrentRunScore => currentRunScore;

        public delegate void ScoreChangedEvent(int newScore, int runScore);
        public event ScoreChangedEvent OnScoreChanged;

        public delegate void RunChangedEvent(int newRun);
        public event RunChangedEvent OnRunChanged;

        /// <summary>
        /// Start a new game session.
        /// </summary>
        public void ResetGame()
        {
            currentRun = 0;
            totalScore = 0;
            currentRunScore = 0;
            OnScoreChanged?.Invoke(totalScore, currentRunScore);
            OnRunChanged?.Invoke(currentRun);
        }

        /// <summary>
        /// Start a new run.
        /// </summary>
        public void StartNewRun()
        {
            currentRun++;
            currentRunScore = 0;
            OnRunChanged?.Invoke(currentRun);
            Debug.Log($"Run {currentRun} started");
        }

        /// <summary>
        /// Calculate and add score for collecting a chest.
        /// </summary>
        public int AddChestScore(float timeRemaining)
        {
            if (gameConfig == null)
            {
                Debug.LogError("ScoreManager: GameConfig not assigned!");
                return 0;
            }

            currentRunScore = gameConfig.CalculateRunScore(timeRemaining, currentRun);
            totalScore += currentRunScore;

            OnScoreChanged?.Invoke(totalScore, currentRunScore);

            Debug.Log($"Chest collected! Run score: {currentRunScore}, Total: {totalScore}");
            return currentRunScore;
        }

        /// <summary>
        /// Reset total score (on time up).
        /// </summary>
        public void ResetScore()
        {
            totalScore = 0;
            currentRunScore = 0;
            OnScoreChanged?.Invoke(totalScore, currentRunScore);
            Debug.Log("Score reset to 0");
        }

        /// <summary>
        /// Get score statistics for display.
        /// </summary>
        public ScoreStats GetScoreStats()
        {
            return new ScoreStats
            {
                totalScore = totalScore,
                currentRun = currentRun,
                currentRunScore = currentRunScore
            };
        }
    }

    /// <summary>
    /// Data structure for score statistics.
    /// </summary>
    [System.Serializable]
    public struct ScoreStats
    {
        public int totalScore;
        public int currentRun;
        public int currentRunScore;
    }
}
