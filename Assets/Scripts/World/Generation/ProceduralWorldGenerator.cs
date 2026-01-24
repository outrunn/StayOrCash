using UnityEngine;
using System.Collections.Generic;
using StayOrCash.Interfaces;
using StayOrCash.Data;

namespace StayOrCash.World
{
    /// <summary>
    /// Generates procedural terrain and spawns nature objects using configuration data.
    /// Implements IWorldGenerator for modularity.
    /// </summary>
    public class ProceduralWorldGenerator : MonoBehaviour, IWorldGenerator
    {
        [Header("Configuration")]
        [SerializeField] private WorldGenerationConfig config;

        [Header("Nature Prefabs - Trees")]
        [SerializeField] private GameObject[] treePrefabs;

        [Header("Nature Prefabs - Bushes")]
        [SerializeField] private GameObject[] bushPrefabs;

        [Header("Nature Prefabs - Rocks")]
        [SerializeField] private GameObject[] rockPrefabs;

        [Header("Nature Prefabs - Details")]
        [Tooltip("DEPRECATED: Use WorldGenerationConfig grass/flower detail textures instead")]
        [SerializeField] private GameObject[] mushroomPrefabs;

        [Header("Chest")]
        [SerializeField] private GameObject chestPrefab;

        [Header("Terrain Layer")]
        [SerializeField] private LayerMask terrainLayer;

        private Terrain terrain;
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private GameObject currentChest;
        private int currentSeed;

        #region IWorldGenerator Implementation

        public void GenerateWorld(int seed)
        {
            if (config == null)
            {
                Debug.LogError("WorldGenerationConfig not assigned!");
                return;
            }

            currentSeed = seed;
            Random.InitState(seed);

            ClearWorld();
            CreateTerrain();
            ApplyTerrainTexture();
            GenerateTerrainDetails();
            SpawnNatureObjects();
            SpawnChest();

            Debug.Log($"<color=lime>========================================</color>");
            Debug.Log($"<color=lime>WORLD GENERATION COMPLETE (Seed: {seed})</color>");
            Debug.Log($"<color=lime>Total objects spawned: {spawnedObjects.Count}</color>");
            Debug.Log($"<color=lime>========================================</color>");
        }

        public void ClearWorld()
        {
            // Clean up spawned objects
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            spawnedObjects.Clear();

            // Clean up chest
            if (currentChest != null)
            {
                Destroy(currentChest);
                currentChest = null;
            }

            // Clean up terrain
            if (terrain != null)
            {
                Destroy(terrain.gameObject);
                terrain = null;
            }
        }

        public Vector3 GetCenterSpawnPosition()
        {
            // Get terrain height at center (0, 0 in world space)
            float terrainHeight = GetTerrainHeightAtPosition(Vector3.zero);
            return new Vector3(0, terrainHeight + 1.5f, 0);
        }

        public Vector3 GetRandomSpawnPosition()
        {
            Vector3 randomPosXZ;
            int attempts = 0;

            do
            {
                randomPosXZ = new Vector3(
                    Random.Range(-config.worldSize / 2f + 5f, config.worldSize / 2f - 5f),
                    0,
                    Random.Range(-config.worldSize / 2f + 5f, config.worldSize / 2f - 5f)
                );
                attempts++;
            }
            while (Vector3.Distance(randomPosXZ, Vector3.zero) < config.minDistanceFromCenter && attempts < 100);

            // Get terrain height at this position
            float terrainHeight = GetTerrainHeightAtPosition(randomPosXZ);
            return new Vector3(randomPosXZ.x, terrainHeight, randomPosXZ.z);
        }

        #endregion

        #region Terrain Generation

        private void CreateTerrain()
        {
            GameObject terrainObject = new GameObject("ProceduralTerrain");
            terrain = terrainObject.AddComponent<Terrain>();
            TerrainCollider terrainCollider = terrainObject.AddComponent<TerrainCollider>();

            TerrainData terrainData = new TerrainData();
            terrainData.heightmapResolution = config.heightmapResolution;
            terrainData.size = new Vector3(config.worldSize, config.terrainHeightRange, config.worldSize);

            float[,] heights = GenerateHeights(terrainData.heightmapResolution);
            terrainData.SetHeights(0, 0, heights);

            terrain.terrainData = terrainData;
            terrainCollider.terrainData = terrainData;

            terrainObject.transform.position = new Vector3(-config.worldSize / 2f, 0, -config.worldSize / 2f);

            if (terrainLayer != 0)
            {
                terrainObject.layer = (int)Mathf.Log(terrainLayer.value, 2);
            }

            // Configure terrain detail render settings
            terrain.detailObjectDistance = 150f; // Render details up to 150 units
            terrain.detailObjectDensity = 1.0f; // Full density (1.0 = 100%)
            terrain.treeBillboardDistance = 50f;
            terrain.treeDistance = 2000f;
            terrain.heightmapPixelError = 5f; // Quality setting for LOD

            // Force physics update so terrain collider is ready for object spawning
            Physics.SyncTransforms();

            Debug.Log($"Terrain created: world bounds from ({-config.worldSize / 2f}, 0, {-config.worldSize / 2f}) to ({config.worldSize / 2f}, {config.terrainHeightRange}, {config.worldSize / 2f})");
        }

        private float[,] GenerateHeights(int resolution)
        {
            float[,] heights = new float[resolution, resolution];

            float offsetX = Random.Range(0f, 10000f);
            float offsetZ = Random.Range(0f, 10000f);

            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    float xCoord = offsetX + (float)x / resolution * config.noiseScale;
                    float zCoord = offsetZ + (float)z / resolution * config.noiseScale;

                    // Multi-octave Perlin noise
                    float noise = 0f;
                    noise += Mathf.PerlinNoise(xCoord * 10f, zCoord * 10f) * 0.5f;
                    noise += Mathf.PerlinNoise(xCoord * 5f, zCoord * 5f) * 0.3f;
                    noise += Mathf.PerlinNoise(xCoord * 2f, zCoord * 2f) * 0.2f;

                    heights[x, z] = noise;
                }
            }

            return heights;
        }

        private void ApplyTerrainTexture()
        {
            if (terrain == null)
            {
                Debug.LogWarning("Cannot apply terrain texture: terrain is null");
                return;
            }

            if (config.groundTexture == null)
            {
                Debug.LogWarning("Cannot apply terrain texture: groundTexture is null in config");
                return;
            }

            TerrainData terrainData = terrain.terrainData;

            // Create a new terrain layer
            TerrainLayer terrainLayer = new TerrainLayer();
            terrainLayer.diffuseTexture = config.groundTexture;

            if (config.groundNormalMap != null)
            {
                terrainLayer.normalMapTexture = config.groundNormalMap;
            }

            // Set tiling based on world size for reasonable texture scale
            terrainLayer.tileSize = new Vector2(config.worldSize / 4f, config.worldSize / 4f);
            terrainLayer.tileOffset = Vector2.zero;

            // Metallic and smoothness settings for natural ground
            terrainLayer.metallic = 0f;
            terrainLayer.smoothness = 0.3f;

            // Assign the layer to the terrain
            terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };

            // Ensure terrain has the correct URP material
            Material terrainMaterial = terrain.materialTemplate;
            if (terrainMaterial == null)
            {
                // Try to find the default URP terrain material
                terrainMaterial = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
                terrain.materialTemplate = terrainMaterial;
                Debug.Log("Applied URP Terrain/Lit shader to terrain");
            }

            Debug.Log($"Terrain texture applied successfully: {config.groundTexture.name}");
        }

        private void GenerateTerrainDetails()
        {
            if (terrain == null)
            {
                Debug.LogWarning("Cannot generate terrain details: terrain is null");
                return;
            }

            // Check if we have any textures configured
            bool hasGrass = config.grassDetailTextures != null && config.grassDetailTextures.Length > 0;
            bool hasFlowers = config.flowerDetailTextures != null && config.flowerDetailTextures.Length > 0;

            if (!hasGrass && !hasFlowers)
            {
                Debug.LogWarning("No grass or flower detail textures configured in WorldGenerationConfig!");
                return;
            }

            TerrainData terrainData = terrain.terrainData;

            // Always set detail resolution for procedural terrain
            int detailResolution = 512; // Lower resolution for better performance, but still dense
            int resolutionPerPatch = 8; // Higher = better performance, lower = better quality
            terrainData.SetDetailResolution(detailResolution, resolutionPerPatch);
            Debug.Log($"Set terrain detail resolution to {detailResolution} with {resolutionPerPatch} per patch");

            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();

            // Add grass detail prototypes
            if (config.grassDetailTextures != null && config.grassDetailTextures.Length > 0)
            {
                foreach (Texture2D grassTexture in config.grassDetailTextures)
                {
                    if (grassTexture != null)
                    {
                        DetailPrototype grassPrototype = new DetailPrototype();
                        grassPrototype.prototypeTexture = grassTexture;
                        grassPrototype.renderMode = DetailRenderMode.GrassBillboard;
                        grassPrototype.usePrototypeMesh = false;
                        grassPrototype.healthyColor = new Color(0.7f, 0.9f, 0.5f);
                        grassPrototype.dryColor = new Color(0.6f, 0.7f, 0.4f);
                        grassPrototype.minHeight = 0.8f;
                        grassPrototype.maxHeight = 1.5f;
                        grassPrototype.minWidth = 0.8f;
                        grassPrototype.maxWidth = 1.5f;
                        grassPrototype.noiseSpread = 0.4f;
                        detailPrototypes.Add(grassPrototype);
                        Debug.Log($"Added grass detail: {grassTexture.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Null grass texture found in grassDetailTextures array");
                    }
                }
            }

            // Add flower detail prototypes
            if (config.flowerDetailTextures != null && config.flowerDetailTextures.Length > 0)
            {
                foreach (Texture2D flowerTexture in config.flowerDetailTextures)
                {
                    if (flowerTexture != null)
                    {
                        DetailPrototype flowerPrototype = new DetailPrototype();
                        flowerPrototype.prototypeTexture = flowerTexture;
                        flowerPrototype.renderMode = DetailRenderMode.GrassBillboard;
                        flowerPrototype.healthyColor = Color.white;
                        flowerPrototype.dryColor = new Color(0.9f, 0.9f, 0.9f);
                        flowerPrototype.minHeight = 0.3f;
                        flowerPrototype.maxHeight = 0.8f;
                        flowerPrototype.minWidth = 0.3f;
                        flowerPrototype.maxWidth = 0.8f;
                        flowerPrototype.noiseSpread = 0.2f;
                        detailPrototypes.Add(flowerPrototype);
                        Debug.Log($"Added flower detail: {flowerTexture.name}");
                    }
                    else
                    {
                        Debug.LogWarning("Null flower texture found in flowerDetailTextures array");
                    }
                }
            }

            if (detailPrototypes.Count == 0)
            {
                Debug.LogWarning("No detail prototypes configured");
                return;
            }

            // Apply prototypes to terrain
            terrainData.detailPrototypes = detailPrototypes.ToArray();

            // Generate detail maps using Perlin noise
            int grassCount = config.grassDetailTextures != null ? config.grassDetailTextures.Length : 0;
            int flowerCount = config.flowerDetailTextures != null ? config.flowerDetailTextures.Length : 0;

            // Random offset for noise variation
            float offsetX = Random.Range(0f, 10000f);
            float offsetZ = Random.Range(0f, 10000f);

            // Generate grass detail layers
            for (int i = 0; i < grassCount; i++)
            {
                if (config.grassDetailTextures[i] != null)
                {
                    int[,] detailMap = GenerateDetailMap(
                        detailResolution,
                        config.grassDensity,
                        config.grassNoiseScale,
                        config.grassThreshold,
                        offsetX + i * 100f, // Offset each layer differently
                        offsetZ + i * 100f
                    );
                    terrainData.SetDetailLayer(0, 0, i, detailMap);
                }
            }

            // Generate flower detail layers
            for (int i = 0; i < flowerCount; i++)
            {
                if (config.flowerDetailTextures[i] != null)
                {
                    int[,] detailMap = GenerateDetailMap(
                        detailResolution,
                        config.flowerDensity,
                        config.flowerNoiseScale,
                        config.flowerThreshold,
                        offsetX + (grassCount + i) * 100f, // Offset differently from grass
                        offsetZ + (grassCount + i) * 100f
                    );
                    terrainData.SetDetailLayer(0, 0, grassCount + i, detailMap);
                }
            }

            // Final summary
            Debug.Log($"<color=cyan>=== TERRAIN DETAILS COMPLETE ===</color>");
            Debug.Log($"<color=cyan>Grass types: {grassCount}, Flower types: {flowerCount}</color>");
            Debug.Log($"<color=cyan>Detail resolution: {detailResolution}, Patches: {resolutionPerPatch}</color>");
            Debug.Log($"<color=cyan>Render distance: {terrain.detailObjectDistance}m, Density: {terrain.detailObjectDensity}</color>");

            // Verify detail layers were set
            for (int i = 0; i < grassCount + flowerCount; i++)
            {
                int[,] layer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);
                int layerTotal = 0;
                foreach (int val in layer) layerTotal += val;
                Debug.Log($"<color=cyan>Layer {i}: {layerTotal:N0} total instances</color>");
            }
        }

        private int[,] GenerateDetailMap(int resolution, int density, float noiseScale, float threshold, float offsetX, float offsetZ)
        {
            int[,] detailMap = new int[resolution, resolution];
            int totalDetails = 0;
            int minDetail = int.MaxValue;
            int maxDetail = 0;

            // Calculate center exclusion radius in detail map coordinates
            float centerExclusionRadius = (config.minDistanceFromCenter / config.worldSize) * resolution;

            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    // Check if we're in the center exclusion zone
                    float centerX = resolution / 2f;
                    float centerZ = resolution / 2f;
                    float distanceFromCenter = Mathf.Sqrt(
                        Mathf.Pow(x - centerX, 2) + Mathf.Pow(z - centerZ, 2)
                    );

                    if (distanceFromCenter < centerExclusionRadius)
                    {
                        detailMap[x, z] = 0; // Clear area around spawn
                        continue;
                    }

                    // Use Perlin noise for natural distribution
                    // Fixed: Properly scale noise coordinates across world size
                    float normalizedX = (float)x / resolution; // 0 to 1
                    float normalizedZ = (float)z / resolution; // 0 to 1
                    float xCoord = offsetX + normalizedX * noiseScale;
                    float zCoord = offsetZ + normalizedZ * noiseScale;
                    float noise = Mathf.PerlinNoise(xCoord, zCoord);

                    int detailCount = 0;

                    // For grass (low threshold ~0.05): guarantee base coverage everywhere with noise variation
                    // For flowers (high threshold ~0.6): only spawn in high-noise patches as accents
                    if (threshold < 0.2f)
                    {
                        // Grass mode: always spawn with VERY HIGH density for lush appearance
                        // Base coverage: Minimum grass density everywhere
                        // Noise variation: adds significant variation on top
                        float baseCoverage = density * 2f; // Increased from 0.5 to 2 (4x more)
                        float noiseVariation = noise * density * 3f; // Increased from 1.5 to 3 (2x more)
                        detailCount = Mathf.RoundToInt(baseCoverage + noiseVariation);
                        // Clamp to reasonable max to avoid performance issues
                        detailCount = Mathf.Min(detailCount, density * 6);
                    }
                    else
                    {
                        // Flower/accent mode: traditional threshold-based spawning
                        if (noise > threshold)
                        {
                            detailCount = Mathf.RoundToInt((noise - threshold) * density * 3f);
                        }
                    }

                    detailMap[x, z] = detailCount;

                    // Track statistics
                    if (detailCount > 0)
                    {
                        totalDetails += detailCount;
                        minDetail = Mathf.Min(minDetail, detailCount);
                        maxDetail = Mathf.Max(maxDetail, detailCount);
                    }
                }
            }

            // Log detail statistics
            if (totalDetails > 0)
            {
                int nonZeroCells = 0;
                for (int x = 0; x < resolution; x++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        if (detailMap[x, z] > 0) nonZeroCells++;
                    }
                }
                float coverage = (float)nonZeroCells / (resolution * resolution) * 100f;
                float avgPerCell = (float)totalDetails / nonZeroCells;
                Debug.Log($"<color=green>Detail map: {totalDetails:N0} instances, {coverage:F1}% coverage, {avgPerCell:F1} avg/cell, range: {minDetail}-{maxDetail}</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>Detail map generated NO details! threshold={threshold}, noiseScale={noiseScale}, density={density}</color>");
            }

            return detailMap;
        }

        /// <summary>
        /// Gets the terrain height at a world position using Terrain.SampleHeight
        /// More reliable than raycasting for procedurally generated terrain
        /// </summary>
        private float GetTerrainHeightAtPosition(Vector3 worldPosition)
        {
            if (terrain == null) return 0f;

            // Convert world position to terrain-local position
            Vector3 terrainLocalPos = worldPosition - terrain.transform.position;

            // Sample height from terrain
            float height = terrain.SampleHeight(worldPosition);

            return height;
        }

        #endregion

        #region Object Spawning

        private void SpawnNatureObjects()
        {
            SpawnObjects(treePrefabs, config.treeCount, config.treeScaleRange, config.treeMinSpacing);
            SpawnObjects(bushPrefabs, config.bushCount, config.bushScaleRange, config.bushMinSpacing);
            SpawnObjects(rockPrefabs, config.rockCount, config.rockScaleRange, config.rockMinSpacing);

            // Grass and flowers are now handled by terrain detail system
            // Only spawn mushroom prefabs if assigned
            if (mushroomPrefabs != null && mushroomPrefabs.Length > 0)
            {
                SpawnObjects(mushroomPrefabs, config.detailCount, config.detailScaleRange, config.detailMinSpacing);
            }
        }

        private void SpawnObjects(GameObject[] prefabs, int count, Vector2 scaleRange, float minSpacing)
        {
            if (prefabs == null || prefabs.Length == 0) return;

            int attempts = 0;
            int maxAttempts = count * 10;
            int spawned = 0;
            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            while (spawned < count && attempts < maxAttempts)
            {
                attempts++;

                // Generate random position in world space (terrain center is at 0,0)
                float randomX = Random.Range(-config.worldSize / 2f + 5f, config.worldSize / 2f - 5f);
                float randomZ = Random.Range(-config.worldSize / 2f + 5f, config.worldSize / 2f - 5f);
                Vector3 randomPosXZ = new Vector3(randomX, 0, randomZ);

                // Check center exclusion
                if (Vector3.Distance(randomPosXZ, Vector3.zero) < config.minDistanceFromCenter)
                    continue;

                // Get terrain height at this position (more reliable than raycasting)
                float terrainHeight = GetTerrainHeightAtPosition(randomPosXZ);
                Vector3 spawnPos = new Vector3(randomX, terrainHeight, randomZ);

                // Check spacing with existing objects
                bool tooClose = false;
                foreach (GameObject obj in spawnedObjects)
                {
                    if (obj != null && Vector3.Distance(obj.transform.position, spawnPos) < minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                // Spawn the object
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                GameObject spawnedObj = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

                spawnedObj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                float scale = Random.Range(scaleRange.x, scaleRange.y);
                spawnedObj.transform.localScale = Vector3.one * scale;

                spawnedObjects.Add(spawnedObj);
                spawned++;

                // Track spawn distribution for debugging
                minX = Mathf.Min(minX, randomX);
                maxX = Mathf.Max(maxX, randomX);
                minZ = Mathf.Min(minZ, randomZ);
                maxZ = Mathf.Max(maxZ, randomZ);
            }

            if (spawned > 0)
            {
                string prefabName = prefabs[0].name;
                Debug.Log($"<color=yellow>Spawned {spawned}/{count} {prefabName}(s) across X:[{minX:F1}, {maxX:F1}] Z:[{minZ:F1}, {maxZ:F1}]</color>");
            }
            else
            {
                Debug.LogWarning($"<color=red>Failed to spawn any objects from {prefabs[0].name} after {attempts} attempts!</color>");
            }
        }

        #endregion

        #region Chest Spawning

        private void SpawnChest()
        {
            if (chestPrefab == null)
            {
                Debug.LogWarning("Chest prefab not assigned!");
                return;
            }

            // Generate position at specified distance from center
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(config.chestMinDistance, config.chestMaxDistance);

            Vector3 randomPosXZ = new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance
            );

            // Get terrain height at this position
            float terrainHeight = GetTerrainHeightAtPosition(randomPosXZ);
            Vector3 chestPosition = new Vector3(randomPosXZ.x, terrainHeight + config.chestHeightOffset, randomPosXZ.z);

            currentChest = Instantiate(chestPrefab, chestPosition, Quaternion.identity, transform);

            var chestScript = currentChest.GetComponent<ChestInteractable>();
            if (chestScript == null)
            {
                chestScript = currentChest.AddComponent<ChestInteractable>();
            }

            Debug.Log($"<color=magenta>Chest spawned at ({chestPosition.x:F1}, {chestPosition.y:F1}, {chestPosition.z:F1}), distance from center: {distance:F1}m</color>");
        }

        public GameObject GetCurrentChest()
        {
            return currentChest;
        }

        #endregion
    }
}
