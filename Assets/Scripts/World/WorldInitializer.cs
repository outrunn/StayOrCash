using UnityEngine;
using StayOrCash.World;

/// <summary>
/// Initializes the procedural world when the game starts.
/// Attach this to a GameObject in your scene along with ProceduralWorldGenerator.
/// </summary>
public class WorldInitializer : MonoBehaviour
{
    [Header("World Generation Settings")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int customSeed = 12345;

    private ProceduralWorldGenerator worldGenerator;

    private void Awake()
    {
        worldGenerator = GetComponent<ProceduralWorldGenerator>();

        if (worldGenerator == null)
        {
            Debug.LogError("WorldInitializer: ProceduralWorldGenerator component not found on this GameObject!");
            return;
        }
    }

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateWorld();
        }
    }

    public void GenerateWorld()
    {
        if (worldGenerator == null)
        {
            Debug.LogError("WorldInitializer: Cannot generate world - ProceduralWorldGenerator is missing!");
            return;
        }

        int seed = useRandomSeed ? Random.Range(0, 100000) : customSeed;
        Debug.Log($"Generating world with seed: {seed}");

        worldGenerator.GenerateWorld(seed);
    }

    public void RegenerateWorld()
    {
        GenerateWorld();
    }
}
