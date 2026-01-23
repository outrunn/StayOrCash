#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using System.IO;
using PokerWar.UI;

/// <summary>
/// Editor tool to automatically generate the game's UI prefab with proper layout and styling.
/// Access via: Tools → Generate Game UI
/// </summary>
public class UIGenerator : EditorWindow
{
    private static string prefabPath = "Assets/Prefabs/UI/GameCanvas.prefab";

    [MenuItem("Tools/Generate Game UI")]
    public static void ShowWindow()
    {
        UIGenerator window = GetWindow<UIGenerator>("UI Generator");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        GUILayout.Label("Game UI Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This tool generates the game HUD with:\n" +
            "• Timer (top-left, color-coded)\n" +
            "• Score (top-right, gold)\n" +
            "• Run number (top-right)\n" +
            "• Interaction prompt (bottom-center)\n" +
            "• Proper anchoring for all screen sizes\n" +
            "• TextMeshPro with outlines\n\n" +
            "Saved to: " + prefabPath,
            MessageType.Info
        );

        GUILayout.Space(10);

        if (GUILayout.Button("Generate HUD UI", GUILayout.Height(40)))
        {
            GenerateUI();
        }

        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "After generation:\n" +
            "1. Drag the prefab into your scene\n" +
            "2. Prefab will automatically connect to GameUI.cs\n" +
            "3. Customize colors/fonts as needed",
            MessageType.Info
        );
    }

    private static void GenerateUI()
    {
        try
        {
            // Create prefab directory if it doesn't exist
            string directory = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

        // Create Canvas
        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Add GameUI script
        GameUI gameUI = canvasGO.AddComponent<GameUI>();

        // Create HUD Panel
        GameObject hudPanel = CreatePanel("HUD Panel", canvasGO.transform);
        SetAnchors(hudPanel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);

        // Create Timer (Top-Left)
        GameObject timerObj = CreateText("Timer", hudPanel.transform, "Time: 00:00", 36);
        RectTransform timerRT = timerObj.GetComponent<RectTransform>();
        SetAnchors(timerRT, new Vector2(0, 1), new Vector2(0, 1));
        timerRT.anchoredPosition = new Vector2(30, -30);
        timerRT.sizeDelta = new Vector2(300, 50);

        TextMeshProUGUI timerText = timerObj.GetComponent<TextMeshProUGUI>();
        timerText.fontSize = 36;
        timerText.fontStyle = FontStyles.Bold;
        timerText.alignment = TextAlignmentOptions.Left;
        AddTextOutline(timerText);

        // Create Score (Top-Right)
        GameObject scoreObj = CreateText("Score", hudPanel.transform, "Score: 0", 36);
        RectTransform scoreRT = scoreObj.GetComponent<RectTransform>();
        SetAnchors(scoreRT, new Vector2(1, 1), new Vector2(1, 1));
        scoreRT.anchoredPosition = new Vector2(-30, -30);
        scoreRT.sizeDelta = new Vector2(300, 50);

        TextMeshProUGUI scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        scoreText.fontSize = 36;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.alignment = TextAlignmentOptions.Right;
        scoreText.color = new Color(1f, 0.85f, 0f); // Gold color
        AddTextOutline(scoreText);

        // Create Run Number (Top-Right, below score)
        GameObject runObj = CreateText("Run", hudPanel.transform, "Run: 1", 28);
        RectTransform runRT = runObj.GetComponent<RectTransform>();
        SetAnchors(runRT, new Vector2(1, 1), new Vector2(1, 1));
        runRT.anchoredPosition = new Vector2(-30, -90);
        runRT.sizeDelta = new Vector2(200, 40);

        TextMeshProUGUI runText = runObj.GetComponent<TextMeshProUGUI>();
        runText.fontSize = 28;
        runText.alignment = TextAlignmentOptions.Right;
        AddTextOutline(runText);

        // Create Interaction Prompt (Bottom-Center)
        GameObject promptObj = CreateText("Prompt", hudPanel.transform, "Press E to collect chest", 32);
        RectTransform promptRT = promptObj.GetComponent<RectTransform>();
        SetAnchors(promptRT, new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        promptRT.anchoredPosition = new Vector2(0, 100);
        promptRT.sizeDelta = new Vector2(600, 50);

        TextMeshProUGUI promptText = promptObj.GetComponent<TextMeshProUGUI>();
        promptText.fontSize = 32;
        promptText.fontStyle = FontStyles.Bold;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = new Color(1f, 1f, 0.5f); // Light yellow
        AddTextOutline(promptText);
        promptObj.SetActive(false); // Start hidden

        // Assign references to GameUI using SerializedObject
        SerializedObject serializedUI = new SerializedObject(gameUI);
        serializedUI.FindProperty("timerText").objectReferenceValue = timerText;
        serializedUI.FindProperty("scoreText").objectReferenceValue = scoreText;
        serializedUI.FindProperty("runText").objectReferenceValue = runText;
        serializedUI.FindProperty("promptText").objectReferenceValue = promptText;
        serializedUI.ApplyModifiedProperties();

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath);

        // Clean up the scene object
        DestroyImmediate(canvasGO);

        // Select the prefab
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);

        Debug.Log($"✅ UI Prefab generated successfully at: {prefabPath}\n\nNext steps:\n1. Drag prefab into your scene\n2. UI will automatically update during gameplay");

            EditorUtility.DisplayDialog("UI Generated!",
                "Game UI prefab created successfully!\n\n" +
                "Location: " + prefabPath + "\n\n" +
                "Drag it into your scene to use it.",
                "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to generate UI: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error",
                "Failed to generate UI!\n\n" +
                "Error: " + e.Message + "\n\n" +
                "Check the Console for details.",
                "OK");
        }
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        return panel;
    }

    private static GameObject CreateText(string name, Transform parent, string initialText, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = initialText;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        return textObj;
    }

    private static void SetAnchors(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = new Vector2(
            (anchorMin.x + anchorMax.x) / 2f,
            (anchorMin.y + anchorMax.y) / 2f
        );
    }

    private static void AddTextOutline(TextMeshProUGUI text)
    {
        text.fontSharedMaterial = new Material(text.fontSharedMaterial);
        text.outlineWidth = 0.2f;
        text.outlineColor = new Color(0, 0, 0, 0.8f);
    }
}
#endif
