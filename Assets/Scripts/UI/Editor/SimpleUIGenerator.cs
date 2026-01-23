#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// Simplified UI generator that creates HUD elements without complex dependencies.
/// Use this if the main UIGenerator has errors.
/// Access via: Tools → Generate Simple HUD
/// </summary>
public class SimpleUIGenerator
{
    [MenuItem("Tools/Generate Simple HUD")]
    public static void GenerateSimpleHUD()
    {
        try
        {
            // Create directory
            string prefabPath = "Assets/Prefabs/UI";
            if (!Directory.Exists(prefabPath))
            {
                Directory.CreateDirectory(prefabPath);
                AssetDatabase.Refresh();
            }

            // Create Canvas
            GameObject canvasGO = new GameObject("GameCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Create HUD Panel
            GameObject hudPanel = new GameObject("HUD Panel");
            hudPanel.transform.SetParent(canvasGO.transform, false);
            RectTransform hudRT = hudPanel.AddComponent<RectTransform>();
            hudRT.anchorMin = Vector2.zero;
            hudRT.anchorMax = Vector2.one;
            hudRT.sizeDelta = Vector2.zero;

            // Timer (Top-Left)
            GameObject timerObj = CreateTextObject("Timer", hudPanel.transform);
            RectTransform timerRT = timerObj.GetComponent<RectTransform>();
            timerRT.anchorMin = new Vector2(0, 1);
            timerRT.anchorMax = new Vector2(0, 1);
            timerRT.pivot = new Vector2(0, 1);
            timerRT.anchoredPosition = new Vector2(30, -30);
            timerRT.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI timerText = timerObj.GetComponent<TextMeshProUGUI>();
            timerText.text = "Time: 00:00";
            timerText.fontSize = 36;
            timerText.fontStyle = FontStyles.Bold;
            timerText.alignment = TextAlignmentOptions.Left;
            timerText.outlineWidth = 0.2f;
            timerText.outlineColor = new Color(0, 0, 0, 0.8f);

            // Score (Top-Right)
            GameObject scoreObj = CreateTextObject("Score", hudPanel.transform);
            RectTransform scoreRT = scoreObj.GetComponent<RectTransform>();
            scoreRT.anchorMin = new Vector2(1, 1);
            scoreRT.anchorMax = new Vector2(1, 1);
            scoreRT.pivot = new Vector2(1, 1);
            scoreRT.anchoredPosition = new Vector2(-30, -30);
            scoreRT.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            scoreText.text = "Score: 0";
            scoreText.fontSize = 36;
            scoreText.fontStyle = FontStyles.Bold;
            scoreText.alignment = TextAlignmentOptions.Right;
            scoreText.color = new Color(1f, 0.85f, 0f);
            scoreText.outlineWidth = 0.2f;
            scoreText.outlineColor = new Color(0, 0, 0, 0.8f);

            // Run (Top-Right, below score)
            GameObject runObj = CreateTextObject("Run", hudPanel.transform);
            RectTransform runRT = runObj.GetComponent<RectTransform>();
            runRT.anchorMin = new Vector2(1, 1);
            runRT.anchorMax = new Vector2(1, 1);
            runRT.pivot = new Vector2(1, 1);
            runRT.anchoredPosition = new Vector2(-30, -90);
            runRT.sizeDelta = new Vector2(200, 40);

            TextMeshProUGUI runText = runObj.GetComponent<TextMeshProUGUI>();
            runText.text = "Run: 1";
            runText.fontSize = 28;
            runText.alignment = TextAlignmentOptions.Right;
            runText.outlineWidth = 0.2f;
            runText.outlineColor = new Color(0, 0, 0, 0.8f);

            // Prompt (Bottom-Center)
            GameObject promptObj = CreateTextObject("Prompt", hudPanel.transform);
            RectTransform promptRT = promptObj.GetComponent<RectTransform>();
            promptRT.anchorMin = new Vector2(0.5f, 0);
            promptRT.anchorMax = new Vector2(0.5f, 0);
            promptRT.pivot = new Vector2(0.5f, 0);
            promptRT.anchoredPosition = new Vector2(0, 100);
            promptRT.sizeDelta = new Vector2(600, 50);

            TextMeshProUGUI promptText = promptObj.GetComponent<TextMeshProUGUI>();
            promptText.text = "Press E to collect chest";
            promptText.fontSize = 32;
            promptText.fontStyle = FontStyles.Bold;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = new Color(1f, 1f, 0.5f);
            promptText.outlineWidth = 0.2f;
            promptText.outlineColor = new Color(0, 0, 0, 0.8f);
            promptObj.SetActive(false);

            // Save as prefab
            string fullPrefabPath = prefabPath + "/GameCanvas.prefab";
            PrefabUtility.SaveAsPrefabAsset(canvasGO, fullPrefabPath);

            // Clean up
            Object.DestroyImmediate(canvasGO);

            // Select
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPrefabPath);
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"✅ Simple HUD created at: {fullPrefabPath}\n\n" +
                     "NEXT STEPS:\n" +
                     "1. Drag prefab into scene\n" +
                     "2. Select it in Hierarchy\n" +
                     "3. Add Component → Search for 'GameUI'\n" +
                     "4. Drag the text elements to the script fields:\n" +
                     "   - Timer Text → timerText field\n" +
                     "   - Score Text → scoreText field\n" +
                     "   - Run Text → runText field\n" +
                     "   - Prompt Text → promptText field");

            EditorUtility.DisplayDialog("HUD Created!",
                "Simple HUD prefab created!\n\n" +
                "Location: " + fullPrefabPath + "\n\n" +
                "Next:\n" +
                "1. Drag into scene\n" +
                "2. Add GameUI component\n" +
                "3. Assign text references\n\n" +
                "See Console for details.",
                "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating HUD: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error", "Failed to create HUD!\n\n" + e.Message, "OK");
        }
    }

    private static GameObject CreateTextObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.color = Color.white;
        return obj;
    }
}
#endif
