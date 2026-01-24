using UnityEngine;

namespace StayOrCash.Data
{
    /// <summary>
    /// ScriptableObject that stores world generation settings.
    /// Create via: Assets -> Create -> Stay or Cash -> World Generation Config
    /// </summary>
    [CreateAssetMenu(fileName = "WorldGenerationConfig", menuName = "Stay or Cash/World Generation Config", order = 2)]
    public class WorldGenerationConfig : ScriptableObject
    {
        [Header("Terrain Settings")]
        [Tooltip("Size of the world in units (width and length)")]
        public int worldSize = 100;

        [Tooltip("Maximum height variation of the terrain")]
        public int terrainHeightRange = 10;

        [Tooltip("Scale of the Perlin noise (lower = smoother terrain)")]
        public float noiseScale = 0.1f;

        [Tooltip("Resolution of the terrain heightmap")]
        public int heightmapResolution = 513;

        [Header("Terrain Textures")]
        [Tooltip("Ground texture for the terrain (e.g., Grass01_BigUV)")]
        public Texture2D groundTexture;

        [Tooltip("Normal map for ground texture (optional)")]
        public Texture2D groundNormalMap;

        [Header("Terrain Details - Grass")]
        [Tooltip("Grass detail textures for terrain detail system")]
        public Texture2D[] grassDetailTextures;

        [Tooltip("Density of grass details per square meter (base value, actual will be 2-6x this)")]
        public int grassDensity = 48;

        [Tooltip("Perlin noise scale for grass distribution (lower = smoother blending)")]
        public float grassNoiseScale = 5f;

        [Tooltip("Threshold for grass spawning (0-1, very low for full coverage)")]
        [Range(0f, 1f)]
        public float grassThreshold = 0.05f;

        [Header("Terrain Details - Flowers")]
        [Tooltip("Flower detail textures for terrain detail system")]
        public Texture2D[] flowerDetailTextures;

        [Tooltip("Density of flower details per square meter")]
        public int flowerDensity = 12;

        [Tooltip("Perlin noise scale for flower distribution (higher = smaller patches)")]
        public float flowerNoiseScale = 15f;

        [Tooltip("Threshold for flower spawning (0-1, higher = fewer flowers, flowers overlay grass)")]
        [Range(0f, 1f)]
        public float flowerThreshold = 0.6f;

        [Header("Object Spawn Counts")]
        public int treeCount = 50;
        public int bushCount = 30;
        public int rockCount = 40;
        public int detailCount = 80;

        [Header("Object Scale Ranges")]
        public Vector2 treeScaleRange = new Vector2(0.8f, 1.5f);
        public Vector2 bushScaleRange = new Vector2(0.7f, 1.2f);
        public Vector2 rockScaleRange = new Vector2(0.5f, 2f);
        public Vector2 detailScaleRange = new Vector2(0.8f, 1.2f);

        [Header("Spawn Spacing")]
        [Tooltip("Minimum distance objects of each type must maintain")]
        public float treeMinSpacing = 2f;
        public float bushMinSpacing = 1.5f;
        public float rockMinSpacing = 1f;
        public float detailMinSpacing = 0.5f;

        [Header("Safe Zones")]
        [Tooltip("Radius around center kept clear for player spawn")]
        public float minDistanceFromCenter = 10f;

        [Header("Chest Placement")]
        [Tooltip("Minimum distance from center to spawn chest")]
        public float chestMinDistance = 20f;

        [Tooltip("Maximum distance from center to spawn chest")]
        public float chestMaxDistance = 40f;

        [Tooltip("Height offset above terrain for chest")]
        public float chestHeightOffset = 1f;
    }
}
