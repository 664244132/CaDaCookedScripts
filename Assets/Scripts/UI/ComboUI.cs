using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ComboUI: แสดงผลป้าย Combo Streak บน HUD ข้างๆ นาฬิกาจับเวลา (Beside Clock UI)
/// ไม่มีพื้นหลังสีดำ (Transparent) และใช้ตัวอักษรสีส้มสดใส คมชัด 100%
/// </summary>
public class ComboUI : MonoBehaviour
{
    public static ComboUI Instance { get; private set; }

    [SerializeField] private GameObject comboBannerObj;
    [SerializeField] private TextMeshProUGUI comboText;

    private float punchScaleTimer = 0f;
    private Vector3 originalScale = Vector3.one;

    private void Awake()
    {
        Instance = this;
        EnsureUIElements();
        Hide();
    }

    private void Start()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnComboStreakChanged += DeliveryManager_OnComboStreakChanged;
        }

        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnComboStreakChanged -= DeliveryManager_OnComboStreakChanged;
        }
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged -= KitchenGameManager_OnStateChanged;
        }
    }

    private void Update()
    {
        // เอฟเฟกต์ Scale Pop เล็กน้อยเมื่อคอมโบเพิ่มขึ้น
        if (punchScaleTimer > 0f && comboBannerObj != null)
        {
            punchScaleTimer -= Time.deltaTime;
            float scaleMultiplier = 1.0f + (punchScaleTimer * 0.35f);
            comboBannerObj.transform.localScale = originalScale * scaleMultiplier;
        }
    }

    private void DeliveryManager_OnComboStreakChanged(object sender, DeliveryManager.OnComboChangedEventArgs e)
    {
        if (e.comboStreak >= 3)
        {
            Show(e.comboStreak, e.comboMultiplier);
        }
        else
        {
            Hide();
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Hide();
        }
    }

    public void Show(int streak, float multiplier)
    {
        EnsureUIElements();
        if (comboBannerObj == null || comboText == null) return;

        comboBannerObj.SetActive(true);

        if (streak >= 5)
        {
            // Super Combo 2.0x (ตัวอักษรสีส้มเพลิงสดใส)
            comboText.text = $"<size=26><color=#FF4500><b>>>> SUPER COMBO x{streak} <<<</b></color></size>\n<size=20><color=#FFA500><b>BONUS x{multiplier:F1} SCORE!</b></color></size>";
        }
        else
        {
            // Combo 1.5x (ตัวอักษรสีส้มทองสดใส)
            comboText.text = $"<size=24><color=#FF7700><b>[ COMBO x{streak} ]</b></color></size>\n<size=19><color=#FFA500><b>BONUS x{multiplier:F1} SCORE!</b></color></size>";
        }

        punchScaleTimer = 0.35f;
    }

    public void Hide()
    {
        if (comboBannerObj != null)
        {
            comboBannerObj.SetActive(false);
        }
    }

    private void EnsureUIElements()
    {
        if (comboBannerObj != null && comboText != null) return;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            parentCanvas = FindFirstObjectByType<Canvas>();
            if (parentCanvas != null)
            {
                transform.SetParent(parentCanvas.transform, false);
            }
        }

        if (comboBannerObj == null)
        {
            comboBannerObj = new GameObject("ComboBanner");
            comboBannerObj.transform.SetParent(transform, false);

            RectTransform rect = comboBannerObj.AddComponent<RectTransform>();
            // จัดวางไว้มุมบนขวา ข้างๆ UI นาฬิกาจับเวลา
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-155f, -50f);
            rect.sizeDelta = new Vector2(300f, 85f);

            // ไม่มีพื้นหลังสีดำ (โปร่งใส 100%)
            Image bg = comboBannerObj.GetComponent<Image>();
            if (bg != null)
            {
                Destroy(bg);
            }

            // สร้าง Text ตัวอักษรสีส้มสดใส
            GameObject textObj = new GameObject("ComboText");
            textObj.transform.SetParent(comboBannerObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            comboText = textObj.AddComponent<TextMeshProUGUI>();
            comboText.alignment = TextAlignmentOptions.Right;
            comboText.fontSize = 24;
            comboText.fontStyle = FontStyles.Bold;
            comboText.color = new Color(1.0f, 0.55f, 0.0f); // Bright Orange
            comboText.textWrappingMode = TextWrappingModes.NoWrap;
            comboText.text = "<size=24><color=#FF7700><b>[ COMBO x3 ]</b></color></size>\n<size=19><color=#FFA500><b>BONUS x1.5 SCORE!</b></color></size>";
        }

        originalScale = comboBannerObj.transform.localScale;
    }
}
