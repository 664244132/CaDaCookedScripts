#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneUIDesigner
{
    [MenuItem("AI Tools/Apply GameScene UI Design")]
    public static void ApplyDesign()
    {
        ApplyDeliveryManagerUI();
        ApplyGamePlayingClockUI();
        ApplyPopups();
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("GameScene UI Design Applied Successfully!");
    }

    private static void ApplyDeliveryManagerUI()
    {
        DeliveryManagerUI deliveryUI = Object.FindObjectOfType<DeliveryManagerUI>(true);
        if (deliveryUI != null)
        {
            RectTransform rect = deliveryUI.GetComponent<RectTransform>();
            // Anchor to Top-Left
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            
            // Adjust background if any
            Image bg = deliveryUI.GetComponent<Image>();
            if (bg != null) bg.color = Color.white;
            
            // Change LayoutGroup spacing
            VerticalLayoutGroup vlg = deliveryUI.GetComponentInChildren<VerticalLayoutGroup>(true);
            if (vlg != null)
            {
                vlg.padding = new RectOffset(10, 10, 10, 10);
                vlg.spacing = 15;
            }
        }
    }

    private static void ApplyGamePlayingClockUI()
    {
        GamePlayingClockUI clockUI = Object.FindObjectOfType<GamePlayingClockUI>(true);
        if (clockUI != null)
        {
            RectTransform rect = clockUI.GetComponent<RectTransform>();
            // Anchor to Top-Right
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            
            // Fix colors to White and Orange
            Image[] images = clockUI.GetComponentsInChildren<Image>(true);
            if (images.Length > 0)
            {
                // Background
                images[0].color = Color.white;
                // Fill (Assuming second image is the fill or timer graphic)
                if (images.Length > 1)
                {
                    Color orange = new Color(1f, 0.65f, 0f); // #FFA500
                    images[1].color = orange;
                }
            }
        }
    }

    private static void ApplyPopups()
    {
        Color orange = new Color(1f, 0.65f, 0f);
        
        GamePauseUI pauseUI = Object.FindObjectOfType<GamePauseUI>(true);
        if (pauseUI != null)
        {
            ApplyStyleToPopup(pauseUI.gameObject, orange);
        }

        GameOverUI gameOverUI = Object.FindObjectOfType<GameOverUI>(true);
        if (gameOverUI != null)
        {
            ApplyStyleToPopup(gameOverUI.gameObject, orange);
        }
    }

    private static void ApplyStyleToPopup(GameObject popup, Color accentColor)
    {
        // Darken background overlay if exists
        Image bg = popup.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = new Color(0, 0, 0, 0.7f);
        }

        // Colorize all texts and buttons
        TextMeshProUGUI[] texts = popup.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            if (t.gameObject.name.Contains("Title") || t.fontSize > 40)
                t.color = accentColor;
            else
                t.color = Color.white;
        }

        Button[] buttons = popup.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            Image btnImg = b.GetComponent<Image>();
            if (btnImg != null) btnImg.color = accentColor;
        }
    }
}
#endif
