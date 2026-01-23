using UnityEngine;
using PokerWar.Managers;
using PokerWar.Player;
using PokerWar.World;

/// <summary>
/// Verifies that the game is set up correctly and provides helpful debug information.
/// Attach this to the GameManager GameObject to get setup diagnostics in the console.
/// </summary>
public class SetupVerifier : MonoBehaviour
{
    [Header("Auto-Verify on Start")]
    [SerializeField] private bool verifyOnStart = true;

    private void Start()
    {
        if (verifyOnStart)
        {
            Invoke(nameof(VerifySetup), 0.5f); // Delay to let other components initialize
        }
    }

    [ContextMenu("Verify Setup")]
    public void VerifySetup()
    {
        Debug.Log("=== GAME SETUP VERIFICATION ===");

        // Check GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("❌ No GameManager found in scene!");
            return;
        }
        Debug.Log("✅ GameManager found");

        // Check ProceduralWorldGenerator
        ProceduralWorldGenerator worldGen = FindFirstObjectByType<ProceduralWorldGenerator>();
        if (worldGen == null)
        {
            Debug.LogWarning("⚠️ No ProceduralWorldGenerator found (might be on different GameObject)");
        }
        else
        {
            Debug.Log("✅ ProceduralWorldGenerator found in scene");
        }

        // Check for Player
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player == null)
        {
            Debug.LogWarning("⚠️ No FirstPersonController found (might spawn later)");
        }
        else
        {
            Debug.Log("✅ FirstPersonController found in scene");
        }

        // Check for chest
        ChestInteractable chest = FindFirstObjectByType<ChestInteractable>();
        if (chest == null)
        {
            Debug.LogWarning("⚠️ No ChestInteractable found (might spawn later)");
        }
        else
        {
            Debug.Log("✅ ChestInteractable found in scene");

            // Check chest collider
            Collider chestCollider = chest.GetComponent<Collider>();
            if (chestCollider == null)
            {
                Debug.LogWarning("⚠️ Chest has no Collider component (needed for interaction detection)");
            }
            else
            {
                Debug.Log($"✅ Chest has {chestCollider.GetType().Name}");
            }
        }

        // Check for terrain
        Terrain terrain = FindFirstObjectByType<Terrain>();
        if (terrain == null)
        {
            Debug.LogWarning("⚠️ No Terrain found (might generate later)");
        }
        else
        {
            Debug.Log("✅ Terrain found in scene");
            if (terrain.materialTemplate == null)
            {
                Debug.LogWarning("⚠️ Terrain has no material assigned");
            }
            else
            {
                Debug.Log($"✅ Terrain material: {terrain.materialTemplate.name}");
            }
        }

        Debug.Log("=== VERIFICATION COMPLETE ===");
    }

    [ContextMenu("List All GameObjects")]
    public void ListAllGameObjects()
    {
        Debug.Log("=== ALL GAMEOBJECTS IN SCENE ===");
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.transform.parent == null) // Only root objects
            {
                Debug.Log($"- {obj.name} (Components: {string.Join(", ", System.Array.ConvertAll(obj.GetComponents<Component>(), c => c.GetType().Name))})");
            }
        }
    }
}
