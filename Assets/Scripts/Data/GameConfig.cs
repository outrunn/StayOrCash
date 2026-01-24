using UnityEngine;

namespace StayOrCash.Data
{
    /// <summary>
    /// ScriptableObject that stores game configuration settings.
    /// Create via: Assets -> Create -> Stay or Cash -> Game Config
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Stay or Cash/Game Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Time Settings")]
        [Tooltip("Starting time for Run 1 in seconds")]
        public float baseTime = 60f;

        [Tooltip("First time reduction amount (Run 1 to Run 2)")]
        public float firstReduction = 5f;

        [Tooltip("How much the reduction increases each run")]
        public float reductionIncrement = 5f;

        [Tooltip("Minimum time allowed for any run")]
        public float minimumTime = 10f;

        [Header("Scoring")]
        [Tooltip("Multiplier for remaining time when calculating score")]
        public float timeScoreMultiplier = 10f;

        [Tooltip("Base score bonus per run number")]
        public int runScoreBonus = 100;

        [Header("Player Settings")]
        [Tooltip("Delay before spawning player after world generation")]
        public float playerSpawnDelay = 0.1f;

        [Header("Game Over Settings")]
        [Tooltip("Delay before restarting after game over")]
        public float gameOverRestartDelay = 3f;

        [Tooltip("Delay before restarting after cash out")]
        public float cashOutRestartDelay = 3f;

        /// <summary>
        /// Calculate the time limit for a specific run number.
        /// </summary>
        public float CalculateTimeLimit(int runNumber)
        {
            if (runNumber == 1)
            {
                return baseTime;
            }

            // Run 2: -5s, Run 3: -10s, Run 4: -15s, etc.
            float totalReduction = firstReduction + (reductionIncrement * (runNumber - 2));
            float timeLimit = baseTime - totalReduction;

            return Mathf.Max(timeLimit, minimumTime);
        }

        /// <summary>
        /// Calculate score for a run based on time remaining.
        /// </summary>
        public int CalculateRunScore(float timeRemaining, int runNumber)
        {
            int timeScore = Mathf.RoundToInt(timeRemaining * timeScoreMultiplier);
            int runBonus = runNumber * runScoreBonus;
            return timeScore + runBonus;
        }
    }
}
