using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameOverUI: แสดงผลแดชบอร์ดสรุปคะแนน, ระดับดาว 3 ระดับ (1-3 Stars), สถิติคอมโบ และนับถอยหลัง 10 วินาทีกลับหน้าเมนู
/// ขยายขนาดตัวอักษรและแดชบอร์ดให้ใหญ่ ชัดเจน เต็มตา คมชัด 100%
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipesDeliveryText;

    private Coroutine autoReturnCoroutine;
    private TextMeshProUGUI titleText;

    private void Awake()
    {
        // ปรับ RectTransform ของ GameOverUI ให้เต็มจอ 100%
        RectTransform rootRect = GetComponent<RectTransform>();
        if (rootRect != null)
        {
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;
        }
    }

    private void Start()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged -= KitchenGameManager_OnStateChanged;
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();

            SetupDashboardLayout();

            if (autoReturnCoroutine != null)
            {
                StopCoroutine(autoReturnCoroutine);
            }
            autoReturnCoroutine = StartCoroutine(AutoReturnToMainMenuRoutine());
        }
        else
        {
            Hide();
        }
    }

    /// <summary>
    /// จัดระเบียบ Hierarchy และซ่อน Label ซ้ำซ้อนใน Scene เดิมเพื่อไม่ให้ตัวหนังสือทับกัน
    /// </summary>
    private void SetupDashboardLayout()
    {
        // 1. จัดการลูกทั้งหมดใน GameOverUI
        foreach (Transform child in transform)
        {
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                // ถ้าเป็นข้อความ "GAME OVER!" หรือ Title ให้ปรับขนาดและตำแหน่งให้อยู่ด้านบน
                if (tmp.text.Contains("GAME OVER") || child.name.Contains("Title") || child.name.Contains("GameOver"))
                {
                    titleText = tmp;
                    titleText.transform.localScale = Vector3.one;
                    titleText.fontSize = 84;
                    titleText.fontStyle = FontStyles.Bold;
                    titleText.color = UITheme.ColorTitleOrange;

                    RectTransform titleRect = titleText.GetComponent<RectTransform>();
                    if (titleRect != null)
                    {
                        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
                        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
                        titleRect.pivot = new Vector2(0.5f, 0.5f);
                        titleRect.anchoredPosition = new Vector2(0f, 290f);
                        titleRect.sizeDelta = new Vector2(1000f, 110f);
                    }
                }
                // ถ้าเป็น Label เก่า "Recipes Delivered" ให้ซ่อนไป เพื่อไม่ให้ทับกับ Dashboard
                else if (tmp != recipesDeliveryText)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        // 2. จัดการกล่อง Dashboard หลัก (recipesDeliveryText)
        if (recipesDeliveryText != null)
        {
            recipesDeliveryText.gameObject.SetActive(true);
            recipesDeliveryText.transform.localScale = Vector3.one;

            RectTransform rect = recipesDeliveryText.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, -30f);
                rect.sizeDelta = new Vector2(1350f, 520f);
            }

            recipesDeliveryText.alignment = TextAlignmentOptions.Center;
            recipesDeliveryText.textWrappingMode = TextWrappingModes.Normal;
            recipesDeliveryText.lineSpacing = 18f;
        }
    }

    private void UpdateDashboardContent(float remainingSeconds)
    {
        if (recipesDeliveryText == null || DeliveryManager.Instance == null) return;

        int totalScore = DeliveryManager.Instance.GetTotalScore();
        int deliveredAmount = DeliveryManager.Instance.GetSuccessfulRecipesAmount();
        int maxCombo = DeliveryManager.Instance.GetMaxComboStreak();
        int stars = DeliveryManager.Instance.GetStarRating();
        string rankTitle = DeliveryManager.Instance.GetChefRankTitle();

        recipesDeliveryText.text = UITheme.FormatGameOverDashboard(
            rankTitle,
            stars,
            totalScore,
            deliveredAmount,
            maxCombo,
            remainingSeconds
        );
    }

    /// <summary>
    /// หน่วงเวลา 10 วินาที พร้อมอัปเดตเวลานับถอยหลัง แล้วพากลับสู่หน้า Main Menu อัตโนมัติ
    /// </summary>
    private IEnumerator AutoReturnToMainMenuRoutine()
    {
        float remaining = 10.0f;
        while (remaining > 0f)
        {
            UpdateDashboardContent(remaining);
            yield return new WaitForSecondsRealtime(0.1f);
            remaining -= 0.1f;
        }

        Time.timeScale = 1f;
        Loader.Load(Loader.Scene.MainMenuScene);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
