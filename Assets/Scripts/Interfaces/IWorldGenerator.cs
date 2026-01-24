using UnityEngine;

namespace StayOrCash.Interfaces
{
    /// <summary>
    /// Interface for procedural world generation systems.
    /// </summary>
    public interface IWorldGenerator
    {
        /// <summary>
        /// Generate a new world using the specified seed.
        /// </summary>
        void GenerateWorld(int seed);

        /// <summary>
        /// Clear the current world and all spawned objects.
        /// </summary>
        void ClearWorld();

        /// <summary>
        /// Get a spawn position at the center of the world (for player).
        /// </summary>
        Vector3 GetCenterSpawnPosition();

        /// <summary>
        /// Get a random spawn position away from the center (for objectives).
        /// </summary>
        Vector3 GetRandomSpawnPosition();
    }
}
