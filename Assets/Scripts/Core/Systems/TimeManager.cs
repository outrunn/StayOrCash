using UnityEngine;
using PokerWar.Data;

namespace PokerWar.Systems
{
    /// <summary>
    /// Manages the countdown timer for each run with accelerating difficulty.
    /// Separated from GameManager for single responsibility.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private GameConfig gameConfig;

        private float currentTime;
        private float currentTimeLimit;
        private bool isActive;

        public float CurrentTime => currentTime;
        public float CurrentTimeLimit => currentTimeLimit;
        public bool IsActive => isActive;
        public float TimeRemaining => Mathf.Max(0f, currentTime);
        public float TimeRemainingNormalized => currentTimeLimit > 0 ? currentTime / currentTimeLimit : 0f;

        public delegate void TimeUpEvent();
        public event TimeUpEvent OnTimeUp;

        public delegate void TimeUpdateEvent(float timeRemaining);
        public event TimeUpdateEvent OnTimeUpdate;

        private void Update()
        {
            if (!isActive) return;

            currentTime -= Time.deltaTime;

            if (currentTime <= 0f)
            {
                currentTime = 0f;
                isActive = false;
                OnTimeUp?.Invoke();
            }

            OnTimeUpdate?.Invoke(currentTime);
        }

        /// <summary>
        /// Start a new timer for the specified run number.
        /// </summary>
        public void StartTimer(int runNumber)
        {
            if (gameConfig == null)
            {
                Debug.LogError("TimeManager: GameConfig not assigned!");
                return;
            }

            currentTimeLimit = gameConfig.CalculateTimeLimit(runNumber);
            currentTime = currentTimeLimit;
            isActive = true;

            Debug.Log($"Timer started: {currentTimeLimit}s for run {runNumber}");
        }

        /// <summary>
        /// Pause the timer.
        /// </summary>
        public void PauseTimer()
        {
            isActive = false;
        }

        /// <summary>
        /// Resume the timer.
        /// </summary>
        public void ResumeTimer()
        {
            isActive = true;
        }

        /// <summary>
        /// Stop and reset the timer.
        /// </summary>
        public void StopTimer()
        {
            isActive = false;
            currentTime = 0f;
            currentTimeLimit = 0f;
        }

        /// <summary>
        /// Add bonus time (for power-ups, etc.)
        /// </summary>
        public void AddTime(float seconds)
        {
            currentTime += seconds;
            Debug.Log($"Added {seconds}s to timer. New time: {currentTime}s");
        }
    }
}
