#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using StayOrCash.World;

namespace StayOrCash.Editor
{
    /// <summary>
    /// Custom editor for ProceduralWorldGenerator with helpful automation tools.
    /// </summary>
    [CustomEditor(typeof(ProceduralWorldGenerator))]
    public class ProceduralWorldGeneratorEditor : UnityEditor.Editor
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
    }
}
#endif
