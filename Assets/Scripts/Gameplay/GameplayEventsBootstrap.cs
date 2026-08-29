using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ระบบ Auto-Bootstrap สำหรับติดตั้งและเปิดใช้งานระบบเกมเพลย์ความถี่สูง (High Frequency Events) ใน GameScene
/// ออกแบบมาเพื่อรอบการเล่น 60 วินาทีที่รวดเร็ว ดุเดือด และเกิดอุปสรรคบ่อยตลอดทั้งเกม
/// </summary>
public class GameplayEventsBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        // ตรวจสอบว่าอยู่ใน GameScene หรือไม่ (มี KitchenGameManager อยู่ในฉาก)
        if (FindFirstObjectByType<KitchenGameManager>() == null) return;

        // สร้าง GameObject สำหรับ Bootstrapper
        GameObject bootstrapObj = new GameObject("--- GAMEPLAY SYSTEMS BOOTSTRAP ---");
        bootstrapObj.AddComponent<GameplayEventsBootstrap>();
    }

    private void Start()
    {
        Debug.Log("🍳 CaDaCook: Initializing High-Frequency 60-Second Gameplay Systems...");

        SetupRushHourSystem();
        SetupFireHazardAndExtinguisher();
        SetupMultipleSlipperyFloors();
        SetupMultiplePotholeTraps();
        SetupKitchenCatNPC();
        SetupMovingCounters();
        SetupKitchenRaftTilt();

        Debug.Log("✅ CaDaCook: All High-Frequency Gameplay Systems Initialized Successfully!");
    }

    // ==========================================
    // 2.3 RUSH HOUR SYSTEM (ความถี่สูง ทุก 18 วิ)
    // ==========================================
    private void SetupRushHourSystem()
    {
        if (RushHourManager.Instance == null)
        {
            GameObject rushObj = new GameObject("RushHourManager");
            rushObj.transform.SetParent(transform);
            rushObj.AddComponent<RushHourManager>();
        }

        if (FindFirstObjectByType<RushHourUI>() == null)
        {
            // สร้าง Dedicated Screen Space Canvas เพื่อให้ข้อความอยู่บนหน้าจอตลอดเวลาและไม่กลับด้าน
            GameObject canvasObj = new GameObject("RushHourScreenCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject bannerObj = new GameObject("RushHourBannerUI");
            bannerObj.transform.SetParent(canvasObj.transform, false);

            RectTransform rect = bannerObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -40);
            rect.sizeDelta = new Vector2(560, 70);

            Image bg = bannerObj.AddComponent<Image>();
            bg.color = new Color(0.88f, 0.12f, 0.05f, 0.95f);

            GameObject textObj = new GameObject("RushHourText");
            textObj.transform.SetParent(bannerObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = ">> RUSH HOUR: 2X POINTS! <<";
            tmp.fontSize = 32;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.yellow;
            tmp.fontStyle = FontStyles.Bold;

            bannerObj.AddComponent<RushHourUI>();
        }
    }

    // ==========================================
    // 2.1 FIRE HAZARD & EXTINGUISHER
    // ==========================================
    private void SetupFireHazardAndExtinguisher()
    {
        StoveCounter[] stoves = FindObjectsByType<StoveCounter>(FindObjectsSortMode.None);
        foreach (StoveCounter stove in stoves)
        {
            FireHazard hazard = stove.GetComponent<FireHazard>();
            if (hazard == null)
            {
                hazard = stove.gameObject.AddComponent<FireHazard>();
            }

            if (stove.transform.Find("FireVisual") == null)
            {
                GameObject fireVisual = new GameObject("FireVisual");
                fireVisual.transform.SetParent(stove.transform, false);
                fireVisual.transform.localPosition = new Vector3(0, 1.2f, 0);

                Light fireLight = fireVisual.AddComponent<Light>();
                fireLight.color = new Color(1f, 0.4f, 0.05f);
                fireLight.intensity = 4f;
                fireLight.range = 6f;

                GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                flame.transform.SetParent(fireVisual.transform, false);
                flame.transform.localScale = new Vector3(0.6f, 1.0f, 0.6f);
                if (flame.TryGetComponent(out Collider col)) Destroy(col);

                Material flameMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                flameMat.color = new Color(1f, 0.3f, 0f, 0.95f);
                if (flameMat.HasProperty("_EmissionColor"))
                {
                    flameMat.EnableKeyword("_EMISSION");
                    flameMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 3f);
                }
                flame.GetComponent<MeshRenderer>().material = flameMat;

                fireVisual.SetActive(false);
            }
        }

        // ถังดับเพลิงสีแดง วางไว้ใกล้เคาน์เตอร์
        if (FindFirstObjectByType<FireExtinguisher>() == null)
        {
            GameObject extObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            extObj.name = "FireExtinguisher";
            extObj.transform.localScale = new Vector3(0.35f, 0.55f, 0.35f);
            
            Vector3 spawnPos = new Vector3(1.2f, 0.4f, 0f);
            if (stoves.Length > 0)
            {
                spawnPos = stoves[0].transform.position + new Vector3(1.4f, 0f, 0f);
                spawnPos.y = 0.4f;
            }
            extObj.transform.position = spawnPos;

            Material redMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            redMat.color = new Color(0.9f, 0.05f, 0.05f);
            extObj.GetComponent<MeshRenderer>().material = redMat;

            GameObject topNozzle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topNozzle.transform.SetParent(extObj.transform, false);
            topNozzle.transform.localPosition = new Vector3(0, 1.1f, 0.2f);
            topNozzle.transform.localScale = new Vector3(0.3f, 0.3f, 0.6f);
            if (topNozzle.TryGetComponent(out Collider nozCol)) Destroy(nozCol);

            extObj.AddComponent<FireExtinguisher>();
        }
    }

    // ==========================================
    // 2.2 MULTIPLE SLIPPERY FLOORS (คราบน้ำมัน 2 จุด)
    // ==========================================
    private void SetupMultipleSlipperyFloors()
    {
        if (FindObjectsByType<SlipperyFloor>(FindObjectsSortMode.None).Length == 0)
        {
            CreateOilPuddle("OilPuddle_Central", new Vector3(0f, 0.02f, 0.2f), new Vector3(2.8f, 0.01f, 2.8f));
            CreateOilPuddle("OilPuddle_Aisle", new Vector3(-2.0f, 0.02f, 1.8f), new Vector3(2.2f, 0.01f, 2.2f));
        }
    }

    private void CreateOilPuddle(string name, Vector3 pos, Vector3 scale)
    {
        GameObject oilPuddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        oilPuddle.name = name;
        oilPuddle.transform.position = pos;
        oilPuddle.transform.localScale = scale;

        Material oilMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        oilMat.color = new Color(0.1f, 0.1f, 0.12f, 0.9f);
        oilPuddle.GetComponent<MeshRenderer>().material = oilMat;

        Collider col = oilPuddle.GetComponent<Collider>();
        col.isTrigger = true;

        oilPuddle.AddComponent<SlipperyFloor>();
    }

    // ==========================================
    // 2.4 MULTIPLE POTHOLE TRAPS (หลุมดักสะดุด 2 จุด)
    // ==========================================
    private void SetupMultiplePotholeTraps()
    {
        if (FindObjectsByType<PotholeTrap>(FindObjectsSortMode.None).Length == 0)
        {
            CreatePotholeTrap("Pothole_Left", new Vector3(-1.8f, 0.01f, -1.2f), 1.3f);
            CreatePotholeTrap("Pothole_Right", new Vector3(2.2f, 0.01f, -1.0f), 1.3f);
        }
    }

    private void CreatePotholeTrap(string name, Vector3 pos, float radius)
    {
        GameObject trapObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trapObj.name = name;
        trapObj.transform.position = pos;
        trapObj.transform.localScale = new Vector3(radius, 0.01f, radius);

        Material trapMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        trapMat.color = new Color(0.2f, 0.18f, 0.16f);
        trapObj.GetComponent<MeshRenderer>().material = trapMat;

        Collider col = trapObj.GetComponent<Collider>();
        col.isTrigger = true;

        trapObj.AddComponent<PotholeTrap>();
    }

    // ==========================================
    // 2.4 KITCHEN CAT NPCS (แมว 2 ตัว สลับกันป่วนครัว)
    // ==========================================
    private void SetupKitchenCatNPC()
    {
        if (FindObjectsByType<KitchenCatNPC>(FindObjectsSortMode.None).Length == 0)
        {
            // แมวส้ม (Orange Cat) ทางฝั่งขวา
            CreateCatNPC("Cat_Orange", new Vector3(3.2f, 0f, -2.2f), new Color(0.95f, 0.55f, 0.15f));

            // แมวเทา (Gray Cat) ทางฝั่งซ้าย
            CreateCatNPC("Cat_Gray", new Vector3(-3.2f, 0f, 1.8f), new Color(0.45f, 0.45f, 0.48f));
        }
    }

    private void CreateCatNPC(string name, Vector3 pos, Color catColor)
    {
        GameObject catObj = new GameObject(name);
        catObj.transform.position = pos;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "CatBody";
        body.transform.SetParent(catObj.transform, false);
        body.transform.localPosition = new Vector3(0, 0.35f, 0);
        body.transform.localRotation = Quaternion.Euler(90, 0, 0);
        body.transform.localScale = new Vector3(0.45f, 0.45f, 0.6f);
        if (body.TryGetComponent(out Collider bCol)) Destroy(bCol);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "CatHead";
        head.transform.SetParent(catObj.transform, false);
        head.transform.localPosition = new Vector3(0, 0.65f, 0.35f);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        if (head.TryGetComponent(out Collider hCol)) Destroy(hCol);

        Material catMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        catMat.color = catColor;
        body.GetComponent<MeshRenderer>().material = catMat;
        head.GetComponent<MeshRenderer>().material = catMat;

        CapsuleCollider catCol = catObj.AddComponent<CapsuleCollider>();
        catCol.center = new Vector3(0, 0.4f, 0);
        catCol.radius = 0.35f;
        catCol.height = 0.8f;

        // จุดสำหรับคาบอาหาร
        GameObject holdObj = new GameObject("HoldPoint");
        holdObj.transform.SetParent(catObj.transform, false);
        holdObj.transform.localPosition = new Vector3(0, 0.7f, 0.5f);

        catObj.AddComponent<KitchenCatNPC>();
    }

    // ==========================================
    // 2.5 MOVING COUNTER (เคาน์เตอร์เลื่อนเร็ว)
    // ==========================================
    private void SetupMovingCounters()
    {
        if (FindFirstObjectByType<MovingCounter>() == null)
        {
            ClearCounter[] counters = FindObjectsByType<ClearCounter>(FindObjectsSortMode.None);
            if (counters.Length > 0)
            {
                counters[0].gameObject.AddComponent<MovingCounter>();
            }
        }
    }

    // ==========================================
    // 2.5 RAFT / KITCHEN WAVE TILT (คลื่นแพเอียง)
    // ==========================================
    private void SetupKitchenRaftTilt()
    {
        if (FindFirstObjectByType<RaftKitchenTilt>() == null)
        {
            // หา Floor หรือ Base ของครัวเพื่อใส่คลื่นโยก
            GameObject floorObj = GameObject.Find("Floor") ?? GameObject.Find("Ground");
            if (floorObj != null)
            {
                floorObj.AddComponent<RaftKitchenTilt>();
            }
            else
            {
                // ถ้าไม่มี Floor ตั้งชื่อเฉพาะ ให้ใส่ที่ Main Camera หรือ Base
                Camera mainCam = Camera.main;
                if (mainCam != null && mainCam.transform.parent != null)
                {
                    mainCam.transform.parent.gameObject.AddComponent<RaftKitchenTilt>();
                }
            }
        }
    }
}
