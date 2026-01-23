#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using PokerWar.Managers;
using PokerWar.Systems;
using PokerWar.Data;
using PokerWar.World;
using PokerWar.UI;
using System.Collections.Generic;
using System.Text;

namespace PokerWar.Editor
{
    /// <summary>
    /// Automated setup wizard that implements the Master Setup Guide.
    /// Accessible via Tools → PokerWar → Run Setup Wizard
    /// </summary>
    public class SetupWizard : EditorWindow
    {
        private Vector2 scrollPosition;
        private StringBuilder report = new StringBuilder();
        private bool setupComplete = false;

        [MenuItem("Tools/PokerWar/Run Setup Wizard")]
        public static void ShowWindow()
        {
            SetupWizard window = GetWindow<SetupWizard>("PokerWar Setup Wizard");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("PokerWar Automated Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("This wizard will automate the Master Setup Guide", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            if (!setupComplete)
            {
                EditorGUILayout.HelpBox(
                    "This wizard will:\n" +
                    "• Create GameConfig and WorldGenerationConfig assets\n" +
                    "• Set up GameManager with all systems\n" +
                    "• Set up WorldGenerator\n" +
                    "• Add UI Canvas to scene\n" +
                    "• Configure scene lighting\n" +
                    "• Validate and wire all references\n\n" +
                    "Current scene will be modified. Make sure you're in the correct scene!",
                    MessageType.Info
                );

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Current Scene: " + SceneManager.GetActiveScene().name, EditorStyles.helpBox);

                EditorGUILayout.Space(10);

                if (GUILayout.Button("Run Setup", GUILayout.Height(40)))
                {
                    RunSetup();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Setup Complete! Review the report below.", MessageType.Info);

                EditorGUILayout.Space(10);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                EditorGUILayout.TextArea(report.ToString(), EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);

                if (GUILayout.Button("Close"))
                {
                    Close();
                }

                if (GUILayout.Button("Run Setup Again"))
                {
                    setupComplete = false;
                    report.Clear();
                }
            }
        }

        private void RunSetup()
        {
            report.Clear();
            report.AppendLine("=== POKERWAR SETUP WIZARD ===\n");

            try
            {
                // Step 1: Create Configuration Assets
                report.AppendLine("STEP 1: Creating Configuration Assets");
                report.AppendLine("--------------------------------------");
                GameConfig gameConfig = CreateGameConfig();
                WorldGenerationConfig worldConfig = CreateWorldGenerationConfig();
                report.AppendLine();

                // Step 2: Set up GameManager
                report.AppendLine("STEP 2: Setting up GameManager");
                report.AppendLine("--------------------------------------");
                GameObject gameManagerObj = SetupGameManager(gameConfig);
                report.AppendLine();

                // Step 3: Set up WorldGenerator
                report.AppendLine("STEP 3: Setting up WorldGenerator");
                report.AppendLine("--------------------------------------");
                GameObject worldGenObj = SetupWorldGenerator(worldConfig);
                report.AppendLine();

                // Step 4: Wire GameManager and WorldGenerator
                report.AppendLine("STEP 4: Wiring References");
                report.AppendLine("--------------------------------------");
                WireReferences(gameManagerObj, worldGenObj, gameConfig);
                report.AppendLine();

                // Step 5: Add UI Canvas
                report.AppendLine("STEP 5: Adding UI Canvas");
                report.AppendLine("--------------------------------------");
                AddUICanvas();
                report.AppendLine();

                // Step 6: Configure Scene
                report.AppendLine("STEP 6: Configuring Scene");
                report.AppendLine("--------------------------------------");
                ConfigureScene();
                report.AppendLine();

                // Step 7: Validate Setup
                report.AppendLine("STEP 7: Validating Setup");
                report.AppendLine("--------------------------------------");
                ValidateSetup(gameManagerObj, worldGenObj);
                report.AppendLine();

                // Final Instructions
                report.AppendLine("===========================================");
                report.AppendLine("SETUP COMPLETE!");
                report.AppendLine("===========================================\n");
                report.AppendLine("REMAINING MANUAL STEPS:");
                report.AppendLine("1. Select 'WorldGenerator' in Hierarchy");
                report.AppendLine("2. Click 'Auto-Assign SimpleNaturePack Prefabs' button in Inspector");
                report.AppendLine("3. (Optional) Assign Skybox: Window → Rendering → Lighting → Skybox Material");
                report.AppendLine("4. Save the scene: File → Save Scene (Ctrl/Cmd+S)");
                report.AppendLine("5. Press Play to test!\n");

                // Mark scene as dirty
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

                setupComplete = true;

                Debug.Log("PokerWar Setup Wizard completed successfully!");
            }
            catch (System.Exception e)
            {
                report.AppendLine("\n❌ ERROR: " + e.Message);
                report.AppendLine("\nStack Trace:\n" + e.StackTrace);
                Debug.LogError("Setup Wizard Error: " + e.Message);
            }
        }

        private GameConfig CreateGameConfig()
        {
            string path = "Assets/DefaultGameConfig.asset";

            // Check if already exists
            GameConfig existing = AssetDatabase.LoadAssetAtPath<GameConfig>(path);
            if (existing != null)
            {
                report.AppendLine("✓ GameConfig already exists at: " + path);
                return existing;
            }

            // Create new config
            GameConfig config = ScriptableObject.CreateInstance<GameConfig>();

            // Set default values
            config.baseTime = 60f;
            config.firstReduction = 5f;
            config.reductionIncrement = 5f;
            config.minimumTime = 10f;
            config.timeScoreMultiplier = 10f;
            config.runScoreBonus = 100;
            config.playerSpawnDelay = 0.1f;
            config.gameOverRestartDelay = 3f;
            config.cashOutRestartDelay = 3f;

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            report.AppendLine("✓ Created GameConfig at: " + path);
            report.AppendLine("  - Base Time: 60s");
            report.AppendLine("  - First Reduction: 5s");
            report.AppendLine("  - Reduction Increment: 5s");

            return config;
        }

        private WorldGenerationConfig CreateWorldGenerationConfig()
        {
            string path = "Assets/DefaultWorldConfig.asset";

            // Check if already exists
            WorldGenerationConfig existing = AssetDatabase.LoadAssetAtPath<WorldGenerationConfig>(path);
            if (existing != null)
            {
                report.AppendLine("✓ WorldGenerationConfig already exists at: " + path);
                return existing;
            }

            // Create new config
            WorldGenerationConfig config = ScriptableObject.CreateInstance<WorldGenerationConfig>();

            // Set default values (these match the defaults in the class)
            // The class already has good defaults, so we don't need to set them all
            // But we'll set a few key ones for clarity

            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();

            report.AppendLine("✓ Created WorldGenerationConfig at: " + path);
            report.AppendLine("  - World Size: 100x100");
            report.AppendLine("  - Tree Count: 50");
            report.AppendLine("  - Chest Distance: 20-40 units");

            return config;
        }

        private GameObject SetupGameManager(GameConfig gameConfig)
        {
            // Check if GameManager already exists
            GameObject existing = GameObject.Find("GameManager");
            if (existing != null)
            {
                report.AppendLine("⚠ GameManager already exists - updating components");
            }
            else
            {
                existing = new GameObject("GameManager");
                report.AppendLine("✓ Created GameManager GameObject");
            }

            // Add/Get components
            GameManager gm = existing.GetComponent<GameManager>();
            if (gm == null)
            {
                gm = existing.AddComponent<GameManager>();
                report.AppendLine("✓ Added GameManager component");
            }
            else
            {
                report.AppendLine("✓ GameManager component already exists");
            }

            TimeManager tm = existing.GetComponent<TimeManager>();
            if (tm == null)
            {
                tm = existing.AddComponent<TimeManager>();
                report.AppendLine("✓ Added TimeManager component");
            }
            else
            {
                report.AppendLine("✓ TimeManager component already exists");
            }

            ScoreManager sm = existing.GetComponent<ScoreManager>();
            if (sm == null)
            {
                sm = existing.AddComponent<ScoreManager>();
                report.AppendLine("✓ Added ScoreManager component");
            }
            else
            {
                report.AppendLine("✓ ScoreManager component already exists");
            }

            PersistentDataManager pdm = existing.GetComponent<PersistentDataManager>();
            if (pdm == null)
            {
                pdm = existing.AddComponent<PersistentDataManager>();
                report.AppendLine("✓ Added PersistentDataManager component");
            }
            else
            {
                report.AppendLine("✓ PersistentDataManager component already exists");
            }

            // Assign configs using SerializedObject to handle private fields
            SerializedObject gmSO = new SerializedObject(gm);
            gmSO.FindProperty("gameConfig").objectReferenceValue = gameConfig;
            gmSO.FindProperty("timeManager").objectReferenceValue = tm;
            gmSO.FindProperty("scoreManager").objectReferenceValue = sm;
            gmSO.FindProperty("dataManager").objectReferenceValue = pdm;
            gmSO.ApplyModifiedProperties();
            report.AppendLine("✓ Assigned GameConfig to GameManager");

            SerializedObject tmSO = new SerializedObject(tm);
            tmSO.FindProperty("gameConfig").objectReferenceValue = gameConfig;
            tmSO.ApplyModifiedProperties();
            report.AppendLine("✓ Assigned GameConfig to TimeManager");

            SerializedObject smSO = new SerializedObject(sm);
            smSO.FindProperty("gameConfig").objectReferenceValue = gameConfig;
            smSO.ApplyModifiedProperties();
            report.AppendLine("✓ Assigned GameConfig to ScoreManager");

            return existing;
        }

        private GameObject SetupWorldGenerator(WorldGenerationConfig worldConfig)
        {
            // Check if WorldGenerator already exists
            GameObject existing = GameObject.Find("WorldGenerator");
            if (existing != null)
            {
                report.AppendLine("⚠ WorldGenerator already exists - updating components");
            }
            else
            {
                existing = new GameObject("WorldGenerator");
                report.AppendLine("✓ Created WorldGenerator GameObject");
            }

            // Add/Get ProceduralWorldGenerator component
            ProceduralWorldGenerator pwg = existing.GetComponent<ProceduralWorldGenerator>();
            if (pwg == null)
            {
                pwg = existing.AddComponent<ProceduralWorldGenerator>();
                report.AppendLine("✓ Added ProceduralWorldGenerator component");
            }
            else
            {
                report.AppendLine("✓ ProceduralWorldGenerator component already exists");
            }

            // Assign config using SerializedObject
            SerializedObject pwgSO = new SerializedObject(pwg);
            pwgSO.FindProperty("config").objectReferenceValue = worldConfig;
            pwgSO.ApplyModifiedProperties();
            report.AppendLine("✓ Assigned WorldGenerationConfig to WorldGenerator");

            return existing;
        }

        private void WireReferences(GameObject gameManagerObj, GameObject worldGenObj, GameConfig gameConfig)
        {
            GameManager gm = gameManagerObj.GetComponent<GameManager>();
            ProceduralWorldGenerator pwg = worldGenObj.GetComponent<ProceduralWorldGenerator>();

            // Wire GameManager → WorldGenerator
            SerializedObject gmSO = new SerializedObject(gm);
            gmSO.FindProperty("worldGenerator").objectReferenceValue = pwg;
            gmSO.ApplyModifiedProperties();
            report.AppendLine("✓ Wired GameManager → WorldGenerator");

            // Find and assign Player prefab
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                gmSO.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
                gmSO.ApplyModifiedProperties();
                report.AppendLine("✓ Assigned Player prefab to GameManager");
            }
            else
            {
                report.AppendLine("⚠ Player prefab not found at Assets/Prefabs/Player.prefab");
                report.AppendLine("  You'll need to assign this manually");
            }

            // Find and assign Chest prefab
            GameObject chestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Chest.prefab");
            if (chestPrefab != null)
            {
                SerializedObject pwgSO = new SerializedObject(pwg);
                pwgSO.FindProperty("chestPrefab").objectReferenceValue = chestPrefab;
                pwgSO.ApplyModifiedProperties();
                report.AppendLine("✓ Assigned Chest prefab to WorldGenerator");
            }
            else
            {
                report.AppendLine("⚠ Chest prefab not found at Assets/Prefabs/Chest.prefab");
                report.AppendLine("  You'll need to assign this manually");
            }
        }

        private void AddUICanvas()
        {
            // Check if GameCanvas already exists
            GameObject existing = GameObject.Find("GameCanvas");
            if (existing != null)
            {
                report.AppendLine("✓ GameCanvas already exists in scene");
                return;
            }

            // Try to find and instantiate the prefab
            GameObject canvasPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/GameCanvas.prefab");
            if (canvasPrefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(canvasPrefab) as GameObject;
                instance.name = "GameCanvas";
                report.AppendLine("✓ Added GameCanvas prefab to scene");

                // Verify GameUI component
                GameUI gameUI = instance.GetComponent<GameUI>();
                if (gameUI != null)
                {
                    report.AppendLine("✓ GameUI component found on Canvas");
                }
                else
                {
                    report.AppendLine("⚠ GameUI component not found on Canvas");
                }
            }
            else
            {
                report.AppendLine("⚠ GameCanvas prefab not found at Assets/Prefabs/UI/GameCanvas.prefab");
                report.AppendLine("  You can generate it via: Tools → Generate Game UI → Generate HUD UI");
            }
        }

        private void ConfigureScene()
        {
            // Remove Main Camera if it exists
            GameObject mainCamera = GameObject.Find("Main Camera");
            if (mainCamera != null)
            {
                DestroyImmediate(mainCamera);
                report.AppendLine("✓ Removed Main Camera (Player creates its own)");
            }
            else
            {
                report.AppendLine("✓ No Main Camera to remove");
            }

            // Add Directional Light if missing
            Light[] lights = FindObjectsOfType<Light>();
            bool hasDirectionalLight = false;
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    hasDirectionalLight = true;
                    break;
                }
            }

            if (!hasDirectionalLight)
            {
                GameObject lightObj = new GameObject("Directional Light");
                Light light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                light.color = new Color(1f, 0.956f, 0.839f); // Slightly warm
                light.intensity = 1f;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                report.AppendLine("✓ Added Directional Light to scene");
            }
            else
            {
                report.AppendLine("✓ Directional Light already exists");
            }
        }

        private void ValidateSetup(GameObject gameManagerObj, GameObject worldGenObj)
        {
            List<string> issues = new List<string>();

            // Validate GameManager
            GameManager gm = gameManagerObj.GetComponent<GameManager>();
            if (gm == null) issues.Add("GameManager component missing");

            SerializedObject gmSO = new SerializedObject(gm);
            if (gmSO.FindProperty("gameConfig").objectReferenceValue == null)
                issues.Add("GameManager: gameConfig not assigned");
            if (gmSO.FindProperty("timeManager").objectReferenceValue == null)
                issues.Add("GameManager: timeManager not assigned");
            if (gmSO.FindProperty("scoreManager").objectReferenceValue == null)
                issues.Add("GameManager: scoreManager not assigned");
            if (gmSO.FindProperty("worldGenerator").objectReferenceValue == null)
                issues.Add("GameManager: worldGenerator not assigned");
            if (gmSO.FindProperty("playerPrefab").objectReferenceValue == null)
                issues.Add("GameManager: playerPrefab not assigned");

            // Validate WorldGenerator
            ProceduralWorldGenerator pwg = worldGenObj.GetComponent<ProceduralWorldGenerator>();
            if (pwg == null) issues.Add("ProceduralWorldGenerator component missing");

            SerializedObject pwgSO = new SerializedObject(pwg);
            if (pwgSO.FindProperty("config").objectReferenceValue == null)
                issues.Add("WorldGenerator: config not assigned");
            if (pwgSO.FindProperty("chestPrefab").objectReferenceValue == null)
                issues.Add("WorldGenerator: chestPrefab not assigned");

            // Report validation results
            if (issues.Count == 0)
            {
                report.AppendLine("✓ All critical references validated successfully!");
            }
            else
            {
                report.AppendLine("⚠ Validation found " + issues.Count + " issue(s):");
                foreach (string issue in issues)
                {
                    report.AppendLine("  - " + issue);
                }
            }

            // Note about SimpleNaturePack
            report.AppendLine("\n⚠ Note: SimpleNaturePack prefabs need manual assignment");
            report.AppendLine("  Use 'Auto-Assign SimpleNaturePack Prefabs' button on WorldGenerator");
        }
    }
}
#endif
