using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using TMPro;
using StayOrCash.UI;
using StayOrCash.Managers;

namespace StayOrCash.Editor
{
    /// <summary>
    /// Automatic setup tool for gameplay loop UI and scenes.
    /// Run via: Tools → Stay or Cash → Setup Gameplay Loop
    /// </summary>
    public class GameplayLoopSetup : EditorWindow
    {
        [MenuItem("Tools/Stay or Cash/Setup Gameplay Loop (Auto)")]
        public static void SetupEverything()
        {
            if (EditorUtility.DisplayDialog("Setup Gameplay Loop",
                "This will automatically create:\n\n" +
                "• MainMenu scene with UI\n" +
                "• Configure mainscene UI elements\n" +
                "• Set up Build Settings\n\n" +
                "Continue?", "Yes, Setup!", "Cancel"))
            {
                CreateMainMenuScene();
                SetupMainSceneUI();
                ConfigureBuildSettings();

                EditorUtility.DisplayDialog("Setup Complete!",
                    "Gameplay loop setup complete!\n\n" +
                    "Next steps:\n" +
                    "1. Open MainMenu scene\n" +
                    "2. Test the flow!\n\n" +
                    "MainMenu → Play → Find Chest → Cash Out → Menu",
                    "OK");
            }
        }

        private static void CreateMainMenuScene()
        {
            Debug.Log("Creating MainMenu scene...");

            // Create new scene
            Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // DefaultGameObjects should include a camera, but let's ensure it exists
            Camera mainCamera = Object.FindObjectOfType<Camera>();
            if (mainCamera == null)
            {
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                cameraObj.tag = "MainCamera";
                cameraObj.transform.position = new Vector3(0, 0, -10);
                mainCamera.clearFlags = CameraClearFlags.SolidColor;
                mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
            }

            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Create EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // Create Background Panel
            GameObject bgPanel = new GameObject("BackgroundPanel");
            bgPanel.transform.SetParent(canvasObj.transform, false);
            Image bgImage = bgPanel.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);
            RectTransform bgRect = bgPanel.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;

            // Create Title Text
            GameObject titleObj = CreateTextMeshPro("TitleText", canvasObj.transform);
            TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "POKERWAR";
            titleText.fontSize = 80;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.sizeDelta = new Vector2(800, 100);

            // Create Cash Text
            GameObject cashObj = CreateTextMeshPro("CashText", canvasObj.transform);
            TextMeshProUGUI cashText = cashObj.GetComponent<TextMeshProUGUI>();
            cashText.text = "Cash: $0";
            cashText.fontSize = 40;
            cashText.alignment = TextAlignmentOptions.Center;
            cashText.color = new Color(0.3f, 1f, 0.3f);
            RectTransform cashRect = cashObj.GetComponent<RectTransform>();
            cashRect.anchorMin = new Vector2(0.5f, 0.65f);
            cashRect.anchorMax = new Vector2(0.5f, 0.65f);
            cashRect.sizeDelta = new Vector2(400, 60);

            // Create High Score Text
            GameObject highScoreObj = CreateTextMeshPro("HighScoreText", canvasObj.transform);
            TextMeshProUGUI highScoreText = highScoreObj.GetComponent<TextMeshProUGUI>();
            highScoreText.text = "High Score: 0";
            highScoreText.fontSize = 30;
            highScoreText.alignment = TextAlignmentOptions.Center;
            highScoreText.color = new Color(1f, 0.8f, 0.3f);
            RectTransform highScoreRect = highScoreObj.GetComponent<RectTransform>();
            highScoreRect.anchorMin = new Vector2(0.5f, 0.55f);
            highScoreRect.anchorMax = new Vector2(0.5f, 0.55f);
            highScoreRect.sizeDelta = new Vector2(400, 50);

            // Create Play Button
            GameObject playButtonObj = new GameObject("PlayButton");
            playButtonObj.transform.SetParent(canvasObj.transform, false);
            Image playImage = playButtonObj.AddComponent<Image>();
            playImage.color = new Color(0.2f, 0.6f, 0.2f);
            Button playButton = playButtonObj.AddComponent<Button>();
            RectTransform playRect = playButtonObj.GetComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.5f, 0.4f);
            playRect.anchorMax = new Vector2(0.5f, 0.4f);
            playRect.sizeDelta = new Vector2(300, 80);

            // Play Button Text
            GameObject playTextObj = CreateTextMeshPro("Text", playButtonObj.transform);
            TextMeshProUGUI playText = playTextObj.GetComponent<TextMeshProUGUI>();
            playText.text = "PLAY";
            playText.fontSize = 50;
            playText.alignment = TextAlignmentOptions.Center;
            playText.color = Color.white;
            RectTransform playTextRect = playTextObj.GetComponent<RectTransform>();
            playTextRect.anchorMin = Vector2.zero;
            playTextRect.anchorMax = Vector2.one;
            playTextRect.sizeDelta = Vector2.zero;

            // Create MenuUI GameObject with MainMenuUI component
            GameObject menuUIObj = new GameObject("MainMenuUI");
            MainMenuUI menuUI = menuUIObj.AddComponent<MainMenuUI>();

            // Use reflection to set private serialized fields
            var cashTextField = typeof(MainMenuUI).GetField("cashText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var highScoreTextField = typeof(MainMenuUI).GetField("highScoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var playButtonField = typeof(MainMenuUI).GetField("playButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gameSceneNameField = typeof(MainMenuUI).GetField("gameSceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            cashTextField?.SetValue(menuUI, cashText);
            highScoreTextField?.SetValue(menuUI, highScoreText);
            playButtonField?.SetValue(menuUI, playButton);
            gameSceneNameField?.SetValue(menuUI, "mainscene");

            // Save scene
            string scenePath = "Assets/Scenes/MainMenu.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(menuScene, scenePath);

            Debug.Log("MainMenu scene created at: " + scenePath);
        }

        private static void SetupMainSceneUI()
        {
            Debug.Log("Setting up mainscene UI...");

            // Load mainscene
            Scene mainScene = EditorSceneManager.OpenScene("Assets/Scenes/mainscene.unity", OpenSceneMode.Single);

            // Ensure there's a temporary camera (player will create its own)
            Camera existingCamera = Object.FindObjectOfType<Camera>();
            if (existingCamera == null)
            {
                GameObject tempCameraObj = new GameObject("Temporary Camera");
                Camera tempCamera = tempCameraObj.AddComponent<Camera>();
                tempCameraObj.tag = "MainCamera";
                tempCamera.transform.position = new Vector3(0, 10, -10);
                tempCamera.transform.rotation = Quaternion.Euler(30, 0, 0);
                Debug.Log("Created temporary camera (will be replaced by player camera)");
            }

            // Find or create Canvas
            Canvas canvas = Object.FindObjectOfType<Canvas>();
            GameObject canvasObj;

            if (canvas == null)
            {
                canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                var scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                // Create EventSystem if needed
                if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            else
            {
                canvasObj = canvas.gameObject;
            }

            // Find or create GameUI
            GameUI gameUI = Object.FindObjectOfType<GameUI>();
            if (gameUI == null)
            {
                GameObject gameUIObj = new GameObject("GameUI");
                gameUI = gameUIObj.AddComponent<GameUI>();
            }

            // Create HUD Panel
            GameObject hudPanel = new GameObject("HUD");
            hudPanel.transform.SetParent(canvasObj.transform, false);

            // Timer Text
            GameObject timerObj = CreateTextMeshPro("TimerText", hudPanel.transform);
            TextMeshProUGUI timerText = timerObj.GetComponent<TextMeshProUGUI>();
            timerText.text = "Time: 01:00";
            timerText.fontSize = 40;
            timerText.alignment = TextAlignmentOptions.TopLeft;
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0, 1);
            timerRect.anchorMax = new Vector2(0, 1);
            timerRect.anchoredPosition = new Vector2(20, -20);
            timerRect.sizeDelta = new Vector2(300, 60);

            // Score Text
            GameObject scoreObj = CreateTextMeshPro("ScoreText", hudPanel.transform);
            TextMeshProUGUI scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            scoreText.text = "Score: 0";
            scoreText.fontSize = 35;
            scoreText.alignment = TextAlignmentOptions.TopLeft;
            RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0, 1);
            scoreRect.anchorMax = new Vector2(0, 1);
            scoreRect.anchoredPosition = new Vector2(20, -80);
            scoreRect.sizeDelta = new Vector2(300, 50);

            // Run Text
            GameObject runObj = CreateTextMeshPro("RunText", hudPanel.transform);
            TextMeshProUGUI runText = runObj.GetComponent<TextMeshProUGUI>();
            runText.text = "Run: 1";
            runText.fontSize = 35;
            runText.alignment = TextAlignmentOptions.TopLeft;
            RectTransform runRect = runObj.GetComponent<RectTransform>();
            runRect.anchorMin = new Vector2(0, 1);
            runRect.anchorMax = new Vector2(0, 1);
            runRect.anchoredPosition = new Vector2(20, -130);
            runRect.sizeDelta = new Vector2(300, 50);

            // Prompt Text (interaction prompt)
            GameObject promptObj = CreateTextMeshPro("PromptText", canvasObj.transform);
            TextMeshProUGUI promptText = promptObj.GetComponent<TextMeshProUGUI>();
            promptText.text = "Press E to interact";
            promptText.fontSize = 30;
            promptText.alignment = TextAlignmentOptions.Center;
            RectTransform promptRect = promptObj.GetComponent<RectTransform>();
            promptRect.anchorMin = new Vector2(0.5f, 0.3f);
            promptRect.anchorMax = new Vector2(0.5f, 0.3f);
            promptRect.sizeDelta = new Vector2(500, 50);
            promptObj.SetActive(false);

            // Find Chest Text (large centered prompt)
            GameObject findChestObj = CreateTextMeshPro("FindChestText", canvasObj.transform);
            TextMeshProUGUI findChestText = findChestObj.GetComponent<TextMeshProUGUI>();
            findChestText.text = "Find the chest!";
            findChestText.fontSize = 70;
            findChestText.alignment = TextAlignmentOptions.Center;
            findChestText.color = new Color(1f, 0.8f, 0.2f);
            RectTransform findChestRect = findChestObj.GetComponent<RectTransform>();
            findChestRect.anchorMin = new Vector2(0.5f, 0.5f);
            findChestRect.anchorMax = new Vector2(0.5f, 0.5f);
            findChestRect.sizeDelta = new Vector2(800, 100);
            findChestObj.SetActive(false);

            // Create Cash Out Panel
            GameObject cashOutPanel = new GameObject("CashOutPanel");
            cashOutPanel.transform.SetParent(canvasObj.transform, false);
            Image cashOutBg = cashOutPanel.AddComponent<Image>();
            cashOutBg.color = new Color(0, 0, 0, 0.8f);
            RectTransform cashOutRect = cashOutPanel.GetComponent<RectTransform>();
            cashOutRect.anchorMin = Vector2.zero;
            cashOutRect.anchorMax = Vector2.one;
            cashOutRect.sizeDelta = Vector2.zero;
            cashOutPanel.SetActive(false);

            // Cash Out Panel Content
            GameObject cashOutContent = new GameObject("Content");
            cashOutContent.transform.SetParent(cashOutPanel.transform, false);

            GameObject cashOutScoreObj = CreateTextMeshPro("ScoreText", cashOutContent.transform);
            TextMeshProUGUI cashOutScoreText = cashOutScoreObj.GetComponent<TextMeshProUGUI>();
            cashOutScoreText.text = "Current Score: 0";
            cashOutScoreText.fontSize = 50;
            cashOutScoreText.alignment = TextAlignmentOptions.Center;
            RectTransform cashOutScoreRect = cashOutScoreObj.GetComponent<RectTransform>();
            cashOutScoreRect.anchorMin = new Vector2(0.5f, 0.7f);
            cashOutScoreRect.anchorMax = new Vector2(0.5f, 0.7f);
            cashOutScoreRect.sizeDelta = new Vector2(600, 80);

            GameObject nextRunInfoObj = CreateTextMeshPro("NextRunInfo", cashOutContent.transform);
            TextMeshProUGUI nextRunInfoText = nextRunInfoObj.GetComponent<TextMeshProUGUI>();
            nextRunInfoText.text = "Next Run: 2\nTime Limit: 55s";
            nextRunInfoText.fontSize = 35;
            nextRunInfoText.alignment = TextAlignmentOptions.Center;
            RectTransform nextRunInfoRect = nextRunInfoObj.GetComponent<RectTransform>();
            nextRunInfoRect.anchorMin = new Vector2(0.5f, 0.55f);
            nextRunInfoRect.anchorMax = new Vector2(0.5f, 0.55f);
            nextRunInfoRect.sizeDelta = new Vector2(500, 100);

            // Continue Button
            GameObject continueButton = CreateButton("ContinueButton", cashOutContent.transform, "CONTINUE", new Color(0.2f, 0.6f, 0.2f));
            RectTransform continueRect = continueButton.GetComponent<RectTransform>();
            continueRect.anchorMin = new Vector2(0.3f, 0.3f);
            continueRect.anchorMax = new Vector2(0.3f, 0.3f);
            continueRect.sizeDelta = new Vector2(300, 80);
            Button continueBtn = continueButton.GetComponent<Button>();
            continueBtn.onClick.AddListener(() => gameUI?.OnContinueClicked());

            // Cash Out Button
            GameObject cashOutButton = CreateButton("CashOutButton", cashOutContent.transform, "CASH OUT", new Color(0.8f, 0.6f, 0.1f));
            RectTransform cashOutBtnRect = cashOutButton.GetComponent<RectTransform>();
            cashOutBtnRect.anchorMin = new Vector2(0.7f, 0.3f);
            cashOutBtnRect.anchorMax = new Vector2(0.7f, 0.3f);
            cashOutBtnRect.sizeDelta = new Vector2(300, 80);
            Button cashOutBtn = cashOutButton.GetComponent<Button>();
            cashOutBtn.onClick.AddListener(() => gameUI?.OnCashOutClicked());

            // Game Over Panel
            GameObject gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(canvasObj.transform, false);
            Image gameOverBg = gameOverPanel.AddComponent<Image>();
            gameOverBg.color = new Color(0, 0, 0, 0.9f);
            RectTransform gameOverRect = gameOverPanel.GetComponent<RectTransform>();
            gameOverRect.anchorMin = Vector2.zero;
            gameOverRect.anchorMax = Vector2.one;
            gameOverRect.sizeDelta = Vector2.zero;
            gameOverPanel.SetActive(false);

            GameObject gameOverTextObj = CreateTextMeshPro("GameOverText", gameOverPanel.transform);
            TextMeshProUGUI gameOverText = gameOverTextObj.GetComponent<TextMeshProUGUI>();
            gameOverText.text = "GAME OVER";
            gameOverText.fontSize = 60;
            gameOverText.alignment = TextAlignmentOptions.Center;
            RectTransform gameOverTextRect = gameOverTextObj.GetComponent<RectTransform>();
            gameOverTextRect.anchorMin = new Vector2(0.5f, 0.5f);
            gameOverTextRect.anchorMax = new Vector2(0.5f, 0.5f);
            gameOverTextRect.sizeDelta = new Vector2(800, 200);

            // Assign to GameUI using reflection
            var timerTextField = typeof(GameUI).GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var scoreTextField = typeof(GameUI).GetField("scoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var runTextField = typeof(GameUI).GetField("runText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var promptTextField = typeof(GameUI).GetField("promptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var findChestTextField = typeof(GameUI).GetField("findChestText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cashOutPanelField = typeof(GameUI).GetField("cashOutPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cashOutScoreTextField = typeof(GameUI).GetField("cashOutScoreText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nextRunInfoTextField = typeof(GameUI).GetField("nextRunInfoText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gameOverPanelField = typeof(GameUI).GetField("gameOverPanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gameOverTextField = typeof(GameUI).GetField("gameOverText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            timerTextField?.SetValue(gameUI, timerText);
            scoreTextField?.SetValue(gameUI, scoreText);
            runTextField?.SetValue(gameUI, runText);
            promptTextField?.SetValue(gameUI, promptText);
            findChestTextField?.SetValue(gameUI, findChestText);
            cashOutPanelField?.SetValue(gameUI, cashOutPanel);
            cashOutScoreTextField?.SetValue(gameUI, cashOutScoreText);
            nextRunInfoTextField?.SetValue(gameUI, nextRunInfoText);
            gameOverPanelField?.SetValue(gameUI, gameOverPanel);
            gameOverTextField?.SetValue(gameUI, gameOverText);

            // Find GameManager and set menu scene name
            GameManager gameManager = Object.FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                var menuSceneNameField = typeof(GameManager).GetField("menuSceneName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                menuSceneNameField?.SetValue(gameManager, "MainMenu");
            }

            EditorSceneManager.SaveScene(mainScene);
            Debug.Log("mainscene UI setup complete!");
        }

        private static void ConfigureBuildSettings()
        {
            Debug.Log("Configuring Build Settings...");

            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();

            // Add MainMenu scene (index 0)
            string mainMenuPath = "Assets/Scenes/MainMenu.unity";
            if (System.IO.File.Exists(mainMenuPath))
            {
                scenes.Add(new EditorBuildSettingsScene(mainMenuPath, true));
            }

            // Add mainscene (index 1)
            string mainScenePath = "Assets/Scenes/mainscene.unity";
            if (System.IO.File.Exists(mainScenePath))
            {
                scenes.Add(new EditorBuildSettingsScene(mainScenePath, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Build Settings configured!");
        }

        private static GameObject CreateTextMeshPro(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 50);
            return obj;
        }

        private static GameObject CreateButton(string name, Transform parent, string buttonText, Color color)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            Image image = buttonObj.AddComponent<Image>();
            image.color = color;
            Button button = buttonObj.AddComponent<Button>();

            GameObject textObj = CreateTextMeshPro("Text", buttonObj.transform);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.fontSize = 40;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            return buttonObj;
        }
    }
}
