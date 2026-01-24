namespace StayOrCash.Interfaces
{
    /// <summary>
    /// Interface for objects that need to observe game state changes.
    /// Implements the Observer pattern for game events.
    /// </summary>
    public interface IGameStateObserver
    {
        void OnRunStarted(int runNumber, float timeLimit);
        void OnChestCollected(int score);
        void OnTimeUp();
        void OnGameOver(int finalScore);
    }
}
