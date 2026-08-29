using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameStartCountdownUI: หน้าต่างสอนเล่น (How to Play Tutorial Overlay)
/// ใช้ตัวอักษร ASCII สากล 100% ปราศจาก Unicode พิเศษ เพื่อรับประกันไม่มีกล่องสี่เหลี่ยมตกหล่น
/// มีกล่องเตือนพิเศษสีส้มเด่นชัดเรื่องถังดับเพลิงและแมว พร้อมปุ่มกดเริ่มเกมได้ทันที (Press Any Button to Start)
/// </summary>
public class GameStartCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;

    private GameObject tutorialPanelObject;
    private TextMeshProUGUI tutorialHeaderCountdownText;
    private TextMeshProUGUI leftColumnText;
    private TextMeshProUGUI rightColumnText;
    private TextMeshProUGUI orangeWarningText;

    private void Awake()
    {
        EnsureRootRectTransform();
        InitializeTutorialOverlay();
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

    /// <summary>
    /// ปรับขยาย RectTransform ของ GameStartCountdownUI ให้เต็มจอ 100% ป้องกันลูกหลานหดเหลือ 0x0
    /// </summary>
    private void EnsureRootRectTransform()
    {
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

    /// <summary>
    /// สร้างหน้าต่างสอนเล่นแบบเต็มจอ จัดวางเนื้อหาเป็นระเบียบ พร้อมกล่องเตือนสีส้มเด่นชัด
    /// </summary>
    private void InitializeTutorialOverlay()
    {
        if (tutorialPanelObject != null) return;

        // ลบ panel เก่าหากมี
        Transform oldPanel = transform.Find("TutorialBlackPanel");
        if (oldPanel != null)
        {
            Destroy(oldPanel.gameObject);
        }

        // 1. แผ่นพื้นหลังสีดำสนิทเต็มจอ
        tutorialPanelObject = new GameObject("TutorialBlackPanel");
        tutorialPanelObject.transform.SetParent(transform, false);
        tutorialPanelObject.transform.SetAsFirstSibling();

        RectTransform panelRect = tutorialPanelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bgImage = tutorialPanelObject.AddComponent<Image>();
        bgImage.color = new Color(0.02f, 0.02f, 0.04f, 0.97f);

        // 2. กล่องหัวเรื่อง & ข้อความเริ่มเกม (Header & Press Any Button Banner)
        GameObject headerObj = new GameObject("TutorialHeader");
        headerObj.transform.SetParent(tutorialPanelObject.transform, false);
        RectTransform headerRect = headerObj.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0, -20f);
        headerRect.sizeDelta = new Vector2(1600f, 90f);

        tutorialHeaderCountdownText = headerObj.AddComponent<TextMeshProUGUI>();
        tutorialHeaderCountdownText.fontSize = 42;
        tutorialHeaderCountdownText.alignment = TextAlignmentOptions.Center;
        tutorialHeaderCountdownText.color = Color.white;
        tutorialHeaderCountdownText.fontStyle = FontStyles.Bold;
        tutorialHeaderCountdownText.text = "<b>HOW TO PLAY   -   <color=#FFD700>>>> PRESS ANY BUTTON TO START <<<</color></b>";

        // 3. คอลัมน์ซ้าย: ปุ่มควบคุม (Controls Column - กว้าง 720px)
        GameObject leftColObj = new GameObject("LeftColumn_Controls");
        leftColObj.transform.SetParent(tutorialPanelObject.transform, false);
        RectTransform leftRect = leftColObj.AddComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0.5f, 0.5f);
        leftRect.anchorMax = new Vector2(0.5f, 0.5f);
        leftRect.pivot = new Vector2(0.5f, 0.5f);
        leftRect.anchoredPosition = new Vector2(-430f, 10f);
        leftRect.sizeDelta = new Vector2(720f, 520f);

        leftColumnText = leftColObj.AddComponent<TextMeshProUGUI>();
        leftColumnText.fontSize = 24;
        leftColumnText.alignment = TextAlignmentOptions.TopLeft;
        leftColumnText.color = Color.white;
        leftColumnText.lineSpacing = 16f;
        leftColumnText.text =
            "<b><size=30>CONTROLS / KEYS</size></b>\n\n" +
            "* <b>[ W ][ A ][ S ][ D ]</b>\n" +
            "   Move Chef Character\n\n" +
            "* <b>[ E ]</b>\n" +
            "   Pick Up / Put Down Ingredients and Plates\n" +
            "   Deliver Order / Grab and Drop Extinguisher\n\n" +
            "* <b>[ F ]</b>\n" +
            "   Chop Ingredients on Cutting Counter\n" +
            "   Hold to Spray White Foam from Extinguisher\n\n" +
            "* <b>[ ESC ]</b>\n" +
            "   Pause / Resume Game";

        // 4. คอลัมน์ขวา: ขั้นตอนการทำอาหาร & อุปสรรค (Cooking & Hazards - กว้าง 860px)
        GameObject rightColObj = new GameObject("RightColumn_Cooking");
        rightColObj.transform.SetParent(tutorialPanelObject.transform, false);
        RectTransform rightRect = rightColObj.AddComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.5f, 0.5f);
        rightRect.anchorMax = new Vector2(0.5f, 0.5f);
        rightRect.pivot = new Vector2(0.5f, 0.5f);
        rightRect.anchoredPosition = new Vector2(430f, 35f);
        rightRect.sizeDelta = new Vector2(860f, 470f);

        rightColumnText = rightColObj.AddComponent<TextMeshProUGUI>();
        rightColumnText.fontSize = 24;
        rightColumnText.alignment = TextAlignmentOptions.TopLeft;
        rightColumnText.color = Color.white;
        rightColumnText.lineSpacing = 15f;
        rightColumnText.text =
            "<b><size=30>HOW TO COOK AND SERVE</size></b>\n\n" +
            "1. <b>Get Ingredients</b> : Grab Bread, Tomato, Cabbage, Cheese, or Meat\n" +
            "2. <b>Chop and Prep</b> : Place Veggies/Cheese on Cutting Counter and press <b>[ F ]</b>\n" +
            "3. <b>Cook Meat</b> : Fry Raw Meat on Stove <i>(Warning: Burns in 3s!)</i>\n" +
            "4. <b>Assemble and Deliver</b> : Put food on Plate and deliver to Delivery counter\n\n" +
            "<b><size=30>KITCHEN HAZARDS</size></b>\n\n" +
            "* <b>Stove Fire</b> : Pick up Extinguisher with <b>[ E ]</b>, Hold <b>[ F ]</b> to spray foam\n" +
            "* <b>Oil and Potholes</b> : Avoid slipping or tripping on the floor";

        // 5. กล่องข้อความเตือนเด่นชัดสีส้ม (Orange Warning Box Panel)
        GameObject warningPanelObj = new GameObject("OrangeWarningPanel");
        warningPanelObj.transform.SetParent(tutorialPanelObject.transform, false);
        RectTransform warnRect = warningPanelObj.AddComponent<RectTransform>();
        warnRect.anchorMin = new Vector2(0.5f, 0f);
        warnRect.anchorMax = new Vector2(0.5f, 0f);
        warnRect.pivot = new Vector2(0.5f, 0f);
        warnRect.anchoredPosition = new Vector2(0, 30f);
        warnRect.sizeDelta = new Vector2(1500f, 130f);

        // พื้นหลังกล่องเตือนโทนสีส้มเข้มตัดขอบ
        Image warnBg = warningPanelObj.AddComponent<Image>();
        warnBg.color = new Color(0.25f, 0.11f, 0.01f, 0.95f);

        // ข้อความเตือนสีส้มสดใส
        GameObject warnTextObj = new GameObject("OrangeWarningText");
        warnTextObj.transform.SetParent(warningPanelObj.transform, false);
        RectTransform warnTextRect = warnTextObj.AddComponent<RectTransform>();
        warnTextRect.anchorMin = Vector2.zero;
        warnTextRect.anchorMax = Vector2.one;
        warnTextRect.offsetMin = new Vector2(25f, 10f);
        warnTextRect.offsetMax = new Vector2(-25f, -10f);

        orangeWarningText = warnTextObj.AddComponent<TextMeshProUGUI>();
        orangeWarningText.fontSize = 24;
        orangeWarningText.alignment = TextAlignmentOptions.Center;
        orangeWarningText.color = new Color(1.0f, 0.65f, 0.0f); // Bright Orange (#FFA500)
        orangeWarningText.lineSpacing = 14f;
        orangeWarningText.text =
            "<b><size=28><color=#FFA500>[ ! ] IMPORTANT WARNING : FIRE EXTINGUISHER</color></size></b>\n" +
            "<b><color=#FFA500>* DO NOT place Fire Extinguisher on counters! The CATS will STEAL it!</color></b>\n" +
            "<b><color=#FFD700>* Always press [ E ] to DROP Extinguisher safely on the FLOOR!</color></b>";

        // ซ่อนข้อความนับถอยหลังตัวเลขเดี่ยวเดิม
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            EnsureRootRectTransform();
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Update()
    {
        if (KitchenGameManager.Instance == null || !KitchenGameManager.Instance.IsCountdownToStartActive()) return;

        // แสดงข้อความกระพริบสวยงามชวนให้กดปุ่มเริ่มเกม
        float pulse = Mathf.PingPong(Time.unscaledTime * 3.5f, 1f);
        string promptColor = pulse > 0.4f ? "#FFD700" : "#FFFFFF";

        if (tutorialHeaderCountdownText != null)
        {
            tutorialHeaderCountdownText.text = $"<b>HOW TO PLAY   -   <color={promptColor}>>>> PRESS ANY BUTTON TO START <<<</color></b>";
        }

        // ตรวจจับการกดปุ่มใดๆ เพื่อเริ่มเกมทันที
        if (CheckAnyInputPressed())
        {
            KitchenGameManager.Instance.StartGameImmediately();
        }
    }

    /// <summary>
    /// ตรวจจับการกดปุ่มใดๆ จากคีย์บอร์ด เมาส์ หรือจอยสติ๊ก
    /// </summary>
    private bool CheckAnyInputPressed()
    {
        // 1. ตรวจจับผ่าน Input เก่า (Keyboard / Mouse)
        if (Input.anyKeyDown) return true;

        // 2. ตรวจจับผ่าน Unity New Input System
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame) return true;
        if (UnityEngine.InputSystem.Mouse.current != null && (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame || UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame)) return true;
        if (UnityEngine.InputSystem.Gamepad.current != null)
        {
            var gp = UnityEngine.InputSystem.Gamepad.current;
            if (gp.buttonSouth.wasPressedThisFrame || gp.buttonNorth.wasPressedThisFrame || gp.buttonEast.wasPressedThisFrame || gp.buttonWest.wasPressedThisFrame || gp.startButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    private void Show()
    {
        if (tutorialPanelObject != null)
        {
            tutorialPanelObject.SetActive(true);
        }
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        if (tutorialPanelObject != null)
        {
            tutorialPanelObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }
}
