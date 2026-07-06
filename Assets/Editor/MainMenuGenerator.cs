using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuGenerator : MonoBehaviour
{
    [MenuItem("AI Tools/Generate Main Menu UI")]
    public static void GenerateMainMenu()
    {
        // 1. Create Canvas
        GameObject canvasGo = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGo.AddComponent<GraphicRaycaster>();
        
        // 2. Attach MainMenuUI script to Canvas
        MainMenuUI mainMenuUI = canvasGo.AddComponent<MainMenuUI>();
        
        // 3. Background (White with slight orange tint)
        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(canvasGo.transform, false);
        Image bgImage = bgGo.AddComponent<Image>();
        bgImage.color = new Color(0.98f, 0.95f, 0.92f); // Very light orange/white
        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // 4. Title Text
        GameObject titleGo = new GameObject("TitleText");
        titleGo.transform.SetParent(canvasGo.transform, false);
        TextMeshProUGUI titleText = titleGo.AddComponent<TextMeshProUGUI>();
        titleText.text = "CHAOS KITCHEN"; // Based on project name
        titleText.fontSize = 150;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = new Color(1f, 0.45f, 0f); // Bright Orange
        titleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.75f);
        titleRect.anchorMax = new Vector2(0.5f, 0.75f);
        titleRect.sizeDelta = new Vector2(1200, 300);
        
        // 5. Play Button
        GameObject playBtnGo = CreateButton("PlayButton", canvasGo.transform, "PLAY", new Color(1f, 0.5f, 0f), Color.white, new Vector2(0, 50));
        Button playButton = playBtnGo.GetComponent<Button>();
        
        // 6. Quit Button
        GameObject quitBtnGo = CreateButton("QuitButton", canvasGo.transform, "QUIT", Color.white, new Color(1f, 0.5f, 0f), new Vector2(0, -150));
        Button quitButton = quitBtnGo.GetComponent<Button>();
        
        // Add Outline to Quit button for better look on white background
        Outline outline = quitBtnGo.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 0.5f, 0f);
        outline.effectDistance = new Vector2(3, -3);
        
        // 7. Assign References to MainMenuUI via SerializedObject
        SerializedObject so = new SerializedObject(mainMenuUI);
        so.FindProperty("playButton").objectReferenceValue = playButton;
        so.FindProperty("quitButton").objectReferenceValue = quitButton;
        so.ApplyModifiedProperties();
        
        // 8. Ensure EventSystem exists
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }
        
        // Mark scene as dirty so Unity knows we made changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("Main Menu UI Generated Successfully! Colors: Orange & White. References are assigned.");
    }
    
    private static GameObject CreateButton(string name, Transform parent, string textStr, Color bgColor, Color textColor, Vector2 position)
    {
        GameObject buttonGo = new GameObject(name);
        buttonGo.transform.SetParent(parent, false);
        Image buttonImage = buttonGo.AddComponent<Image>();
        buttonImage.color = bgColor;
        Button button = buttonGo.AddComponent<Button>();
        
        RectTransform btnRect = buttonGo.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(400, 100);
        btnRect.anchoredPosition = position;
        
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(buttonGo.transform, false);
        TextMeshProUGUI text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = textStr;
        text.fontSize = 50;
        text.fontStyle = FontStyles.Bold;
        text.color = textColor;
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return buttonGo;
    }
}
