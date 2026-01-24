using UnityEngine;

namespace StayOrCash.Interfaces
{
    /// <summary>
    /// Interface for objects that can be interacted with by the player.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Check if the object is within interaction range of the player.
        /// </summary>
        bool IsInRange();

        /// <summary>
        /// Execute the interaction behavior.
        /// </summary>
        void Interact();

        /// <summary>
        /// Get the prompt text to display to the player.
        /// </summary>
        string GetInteractionPrompt();
    }
}
