#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PokerWar.World;

[CustomEditor(typeof(ProceduralWorldGenerator))]
public class ProceduralWorldGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ProceduralWorldGenerator generator = (ProceduralWorldGenerator)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Auto-Setup Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Auto-Assign SimpleNaturePack Prefabs"))
        {
            AutoAssignPrefabs(generator);
        }

        if (GUILayout.Button("Auto-Assign Grass Flowers Pack Assets"))
        {
            AutoAssignGrassFlowersPack(generator);
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Generate Test World (Seed: Random)"))
        {
            if (Application.isPlaying)
            {
                generator.GenerateWorld(Random.Range(0, 100000));
            }
            else
            {
                EditorUtility.DisplayDialog("Not Playing", "Enter Play Mode to test world generation.", "OK");
            }
        }
    }

    private void AutoAssignPrefabs(ProceduralWorldGenerator generator)
    {
        SerializedObject so = new SerializedObject(generator);

        // Find all prefabs in SimpleNaturePack
        string prefabPath = "Assets/SimpleNaturePack/Prefabs";

        // Trees
        List<GameObject> trees = new List<GameObject>();
        for (int i = 1; i <= 5; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/Tree_0{i}.prefab");
            if (prefab != null) trees.Add(prefab);
        }
        SetPrefabArray(so, "treePrefabs", trees);

        // Bushes
        List<GameObject> bushes = new List<GameObject>();
        for (int i = 1; i <= 3; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/Bush_0{i}.prefab");
            if (prefab != null) bushes.Add(prefab);
        }
        SetPrefabArray(so, "bushPrefabs", bushes);

        // Rocks
        List<GameObject> rocks = new List<GameObject>();
        for (int i = 1; i <= 5; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/Rock_0{i}.prefab");
            if (prefab != null) rocks.Add(prefab);
        }
        SetPrefabArray(so, "rockPrefabs", rocks);

        // Grass
        List<GameObject> grass = new List<GameObject>();
        for (int i = 1; i <= 2; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/Grass_0{i}.prefab");
            if (prefab != null) grass.Add(prefab);
        }
        SetPrefabArray(so, "grassPrefabs", grass);

        // Flowers
        List<GameObject> flowers = new List<GameObject>();
        for (int i = 1; i <= 2; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/Flowers_0{i}.prefab");
            if (prefab != null) flowers.Add(prefab);
        }
        SetPrefabArray(so, "flowerPrefabs", flowers);

        // Mushrooms
        List<GameObject> mushrooms = new List<GameObject>();
        for (int i = 1; i <= 2; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabPath}/Mushroom_0{i}.prefab");
            if (prefab != null) mushrooms.Add(prefab);
        }
        SetPrefabArray(so, "mushroomPrefabs", mushrooms);

        so.ApplyModifiedProperties();

        Debug.Log($"Auto-assigned prefabs: {trees.Count} trees, {bushes.Count} bushes, {rocks.Count} rocks, " +
                  $"{grass.Count} grass, {flowers.Count} flowers, {mushrooms.Count} mushrooms");
    }

    private void SetPrefabArray(SerializedObject so, string propertyName, List<GameObject> prefabs)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.arraySize = prefabs.Count;
            for (int i = 0; i < prefabs.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = prefabs[i];
            }
        }
    }

    private void AutoAssignGrassFlowersPack(ProceduralWorldGenerator generator)
    {
        SerializedObject so = new SerializedObject(generator);

        // Base path for Grass Flowers Pack
        string basePath = "Assets/ALP_Assets/GrassFlowersFREE";
        string texturePath = $"{basePath}/Textures/GrassFlowers";
        string demoPath = $"{basePath}/Demo/DemoGrassFlowers";

        // Assign terrain layer (from demo scene)
        TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>($"{demoPath}/layer_Grass01_BigUVd261c09ae55ba1e3.terrainlayer");
        if (terrainLayer != null)
        {
            SerializedProperty layerProp = so.FindProperty("groundTerrainLayer");
            if (layerProp != null)
            {
                layerProp.objectReferenceValue = terrainLayer;
            }
        }

        // Load grass textures (grass01.tga, grass02.tga)
        List<Texture2D> grassTextures = new List<Texture2D>();
        for (int i = 1; i <= 2; i++)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{texturePath}/grass0{i}.tga");
            if (tex != null) grassTextures.Add(tex);
        }
        SetTextureArray(so, "grassDetailTextures", grassTextures);

        // Load flower textures (grassFlower01.tga through grassFlower10.tga)
        List<Texture2D> flowerTextures = new List<Texture2D>();
        for (int i = 1; i <= 10; i++)
        {
            string numStr = i.ToString("D2"); // Format as 01, 02, etc.
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{texturePath}/grassFlower{numStr}.tga");
            if (tex != null) flowerTextures.Add(tex);
        }
        SetTextureArray(so, "flowerDetailTextures", flowerTextures);

        so.ApplyModifiedProperties();

        Debug.Log($"Auto-assigned Grass Flowers Pack: Terrain Layer: {terrainLayer != null}, " +
                  $"{grassTextures.Count} grass textures, {flowerTextures.Count} flower textures");
    }

    private void SetTextureArray(SerializedObject so, string propertyName, List<Texture2D> textures)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop != null)
        {
            prop.arraySize = textures.Count;
            for (int i = 0; i < textures.Count; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = textures[i];
            }
        }
    }
}
#endif
