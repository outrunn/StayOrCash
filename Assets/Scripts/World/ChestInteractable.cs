using UnityEngine;
using PokerWar.Interfaces;
using PokerWar.Managers;

namespace PokerWar.World
{
    /// <summary>
    /// Interactable chest that the player must find and collect.
    /// Implements IInteractable for consistent interaction system.
    /// </summary>
    public class ChestInteractable : MonoBehaviour, IInteractable
    {
        [Header("Visual Settings")]
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private float bobHeight = 0.5f;
        [SerializeField] private float bobSpeed = 2f;

        [Header("Interaction")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private string interactionPrompt = "Press E to collect chest";

        private Vector3 startPosition;
        private Transform playerTransform;
        private bool isCollected = false;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            if (isCollected) return;

            // Rotate and bob for visibility
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }

        #region IInteractable Implementation

        public bool IsInRange()
        {
            if (playerTransform == null)
            {
                // Try to find player if not set
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    return false;
                }
            }

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            return distance <= interactionRange;
        }

        public void Interact()
        {
            Collect();
        }

        public string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        #endregion

        #region Player Reference

        public void SetPlayerReference(Transform player)
        {
            playerTransform = player;
            Debug.Log("ChestInteractable: Player reference set");
        }

        #endregion

        #region Collection

        public void Collect()
        {
            if (isCollected) return;

            isCollected = true;
            Debug.Log("Chest collected!");

            // Notify GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnChestCollected();
            }
            else
            {
                Debug.LogError("GameManager.Instance is null! Cannot notify of chest collection.");
            }

            // Visual feedback
            gameObject.SetActive(false);
        }

        #endregion

        #region Debug Helpers

        private void OnDrawGizmosSelected()
        {
            // Show interaction range in editor when selected
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        private void OnDrawGizmos()
        {
            // Always show interaction range for debugging
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // Draw line to player if reference exists
            if (playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                Gizmos.color = distance <= interactionRange ? Color.green : Color.red;
                Gizmos.DrawLine(transform.position, playerTransform.position);
            }
        }

        #endregion
    }
}
