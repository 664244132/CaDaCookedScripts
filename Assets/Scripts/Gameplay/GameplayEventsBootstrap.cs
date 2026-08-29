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
    // ==========================================
    // PLAYABLE CAMERA BOUNDS (ขอบเขตพื้นที่เล่นในมุมกล้องผู้เล่น)
    // ==========================================
    public const float PLAYABLE_MIN_X = -6.14f;
    public const float PLAYABLE_MAX_X = 6.68f;
    public const float PLAYABLE_MIN_Z = -4.16f;
    public const float PLAYABLE_MAX_Z = 3.97f;

    public static Vector3 ClampToPlayableBounds(Vector3 pos, float y = 0f)
    {
        return new Vector3(
            Mathf.Clamp(pos.x, PLAYABLE_MIN_X, PLAYABLE_MAX_X),
            y,
            Mathf.Clamp(pos.z, PLAYABLE_MIN_Z, PLAYABLE_MAX_Z)
        );
    }

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
        Debug.Log("🍳 CaDaCook: [1/6] Initializing Gameplay Systems...");

        try { SetupRushHourSystem(); Debug.Log("✅ [2.3] Rush Hour System Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ [2.3] Rush Hour Error: {ex.Message}"); }

        try { SetupFireHazardAndExtinguisher(); Debug.Log("✅ [2.1] Fire Hazard & Extinguisher Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ [2.1] Fire Hazard Error: {ex.Message}"); }

        try { SetupMultipleSlipperyFloors(); Debug.Log("✅ [2.2] Slippery Floors (4 Puddles) Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ [2.2] Slippery Floor Error: {ex.Message}"); }

        try { SetupMultiplePotholeTraps(); Debug.Log("✅ [2.2] Pothole Traps Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ [2.2] Pothole Trap Error: {ex.Message}"); }

        try { SetupKitchenCatNPC(); Debug.Log("✅ [2.4] Kitchen Cat NPCs (2 Cats) Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ [2.4] Cat NPC Error: {ex.Message}"); }

        try { SetupMovingCounters(); Debug.Log("✅ [2.5] Moving Counters Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ [2.5] Moving Counter Error: {ex.Message}"); }

        try { SetupKitchenRaftTilt(); Debug.Log("✅ [2.5] Raft Kitchen Wave Tilt Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ [2.5] Raft Tilt Error: {ex.Message}"); }

        try { SetupComboUISystem(); Debug.Log("✅ Combo & Tip Streak UI Initialized!"); }
        catch (Exception ex) { Debug.LogError($"❌ Combo UI Error: {ex.Message}"); }

        Debug.Log("🎉 CaDaCook: All Gameplay Systems & UI are ACTIVE & RUNNING!");
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
    // 2.1 FIRE HAZARD, RANDOM FIRE & EXTINGUISHER
    // ==========================================
    private void SetupFireHazardAndExtinguisher()
    {
        // 1. ติดตั้ง RandomFireManager สำหรับสุ่มเกิดไฟไหม้ตามเคาน์เตอร์
        if (FindFirstObjectByType<RandomFireManager>() == null)
        {
            GameObject randomFireObj = new GameObject("RandomFireManager");
            randomFireObj.transform.SetParent(transform);
            randomFireObj.AddComponent<RandomFireManager>();
        }

        // 2. ติดตั้ง FireHazard & Visuals ให้กับทุกเคาน์เตอร์ในครัว
        BaseCounter[] allCounters = FindObjectsByType<BaseCounter>(FindObjectsSortMode.None);
        foreach (BaseCounter counter in allCounters)
        {
            FireHazard hazard = counter.GetComponent<FireHazard>();
            if (hazard == null)
            {
                hazard = counter.gameObject.AddComponent<FireHazard>();
            }

            // สร้าง Fire Visual จาก VFXPACK_FIRE_WALLCOEUR (VFX_Fire)
            if (counter.transform.Find("FireVisual") == null)
            {
                GameObject firePrefab = Resources.Load<GameObject>("VFX_Fire");
                GameObject fireVisual;
                if (firePrefab != null)
                {
                    fireVisual = Instantiate(firePrefab, counter.transform);
                    fireVisual.name = "FireVisual";
                    fireVisual.transform.localPosition = new Vector3(0, 1.25f, 0);
                    fireVisual.transform.localScale = Vector3.one * 0.75f;
                }
                else
                {
                    fireVisual = new GameObject("FireVisual");
                    fireVisual.transform.SetParent(counter.transform, false);
                    fireVisual.transform.localPosition = new Vector3(0, 1.2f, 0);
                }

                Light fireLight = fireVisual.GetComponent<Light>();
                if (fireLight == null)
                {
                    fireLight = fireVisual.AddComponent<Light>();
                }
                fireLight.color = new Color(1f, 0.45f, 0.1f);
                fireLight.intensity = 3.5f;
                fireLight.range = 5f;
                fireLight.shadows = LightShadows.None; // ปิดเงาเพื่อไม่ให้เปลือง URP Shadow Atlas

                // ปิดเงาของหลอดไฟทุกดวงใน Prefab VFX
                Light[] allLights = fireVisual.GetComponentsInChildren<Light>(true);
                foreach (var l in allLights)
                {
                    l.shadows = LightShadows.None;
                }

                // ลบ Collider ออก
                Collider[] cols = fireVisual.GetComponentsInChildren<Collider>();
                foreach (var c in cols) Destroy(c);

                fireVisual.SetActive(false);
            }

            // สร้าง Locked Visual (ควันดำ VFX_BlackSmoke เมื่อดับไฟไม่ทัน)
            if (counter.transform.Find("LockedVisual") == null)
            {
                GameObject smokePrefab = Resources.Load<GameObject>("VFX_BlackSmoke");
                GameObject lockedVisual;
                if (smokePrefab != null)
                {
                    lockedVisual = Instantiate(smokePrefab, counter.transform);
                    lockedVisual.name = "LockedVisual";
                    lockedVisual.transform.localPosition = new Vector3(0, 1.15f, 0);
                    lockedVisual.transform.localScale = Vector3.one * 0.6f;
                }
                else
                {
                    lockedVisual = new GameObject("LockedVisual");
                    lockedVisual.transform.SetParent(counter.transform, false);
                    lockedVisual.transform.localPosition = new Vector3(0, 1.1f, 0);
                }

                // สัญลักษณ์เตือนสีแดงด้านบน
                GameObject lockIcon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lockIcon.name = "LockIcon";
                lockIcon.transform.SetParent(lockedVisual.transform, false);
                lockIcon.transform.localPosition = new Vector3(0, 0.4f, 0);
                lockIcon.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
                if (lockIcon.TryGetComponent(out Collider colIcon)) Destroy(colIcon);

                Material redIconMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                redIconMat.color = Color.red;
                if (redIconMat.HasProperty("_EmissionColor"))
                {
                    redIconMat.EnableKeyword("_EMISSION");
                    redIconMat.SetColor("_EmissionColor", Color.red * 2.5f);
                }
                lockIcon.GetComponent<MeshRenderer>().material = redIconMat;

                // ลบ Collider ออก
                Collider[] cols = lockedVisual.GetComponentsInChildren<Collider>();
                foreach (var c in cols) Destroy(c);

                lockedVisual.SetActive(false);
            }
        }

        // 3. ถังดับเพลิงสีแดง วางไว้ข้างๆ จุดเริ่มต้นของผู้เล่น (Player Starting Area)
        if (FindFirstObjectByType<FireExtinguisher>() == null)
        {
            GameObject extObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            extObj.name = "FireExtinguisher";
            extObj.transform.localScale = new Vector3(0.35f, 0.55f, 0.35f);
            
            // วางตรงจุดเริ่มต้นข้างตัวผู้เล่น อยู่ในขอบเขตมุมมองกล้อง (X: -6.14 ถึง 6.68, Z: -4.16 ถึง 3.97)
            Vector3 playerPos = new Vector3(0f, 0f, 1f);
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                playerPos = player.transform.position;
            }

            Vector3 spawnPos = playerPos + new Vector3(0.75f, 0f, -0.3f);
            spawnPos = ClampToPlayableBounds(spawnPos, 0.35f);
            extObj.transform.position = spawnPos;

            Material redMat = FireExtinguisher.GetSafeMaterial(new Color(0.9f, 0.05f, 0.05f), 0.5f, 0.6f);
            extObj.GetComponent<MeshRenderer>().material = redMat;

            GameObject topNozzle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topNozzle.name = "TopNozzle";
            topNozzle.transform.SetParent(extObj.transform, false);
            topNozzle.transform.localPosition = new Vector3(0, 1.05f, 0.2f);
            topNozzle.transform.localScale = new Vector3(0.32f, 0.28f, 0.55f);
            if (topNozzle.TryGetComponent(out Collider nozCol)) Destroy(nozCol);
            topNozzle.GetComponent<MeshRenderer>().material = FireExtinguisher.GetSafeMaterial(new Color(0.68f, 0.70f, 0.74f), 0.85f, 0.85f);

            extObj.AddComponent<FireExtinguisher>();
        }
    }

    // ==========================================
    // 2.2 MULTIPLE SLIPPERY FLOORS (คราบน้ำมัน 4 จุดทั่วครัว)
    // ==========================================
    private void SetupMultipleSlipperyFloors()
    {
        if (FindObjectsByType<SlipperyFloor>(FindObjectsSortMode.None).Length == 0)
        {
            // 1. กลางห้องครัว (Central Junction)
            CreateOilPuddle("OilPuddle_Central", ClampToPlayableBounds(new Vector3(0f, 0f, 0.2f), 0.01f), new Vector3(2.8f, 0.01f, 2.8f));
            
            // 2. ทางเดินฝั่งซ้าย (หน้าเตาและเขียง)
            CreateOilPuddle("OilPuddle_LeftAisle", ClampToPlayableBounds(new Vector3(-2.2f, 0f, 1.4f), 0.01f), new Vector3(2.4f, 0.01f, 2.4f));

            // 3. ทางเดินฝั่งขวา (หน้าจุดส่งอาหาร)
            CreateOilPuddle("OilPuddle_RightAisle", ClampToPlayableBounds(new Vector3(2.2f, 0f, 1.2f), 0.01f), new Vector3(2.4f, 0.01f, 2.4f));

            // 4. ทางเดินด้านหน้า
            CreateOilPuddle("OilPuddle_FrontAisle", ClampToPlayableBounds(new Vector3(0f, 0f, -1.8f), 0.01f), new Vector3(2.6f, 0.01f, 2.6f));
        }
    }

    private void CreateOilPuddle(string name, Vector3 pos, Vector3 scale)
    {
        GameObject oilPuddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        oilPuddle.name = name;
        oilPuddle.transform.position = pos;
        oilPuddle.transform.localScale = scale;

        // ลบ Collider ทรงแคปซูลของ Cylinder เดิมออก แล้วใส่ BoxCollider ที่มีความสูงครอบคลุม
        if (oilPuddle.TryGetComponent(out Collider defaultCol))
        {
            Destroy(defaultCol);
        }

        BoxCollider boxCol = oilPuddle.AddComponent<BoxCollider>();
        boxCol.isTrigger = true;
        boxCol.center = new Vector3(0, 50f, 0); // ครอบคลุมความสูงเหนือพื้น
        boxCol.size = new Vector3(1.0f, 100f, 1.0f);

        Material oilMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        oilMat.color = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        if (oilMat.HasProperty("_Smoothness")) oilMat.SetFloat("_Smoothness", 0.95f);
        if (oilMat.HasProperty("_Metallic")) oilMat.SetFloat("_Metallic", 0.85f);
        oilPuddle.GetComponent<MeshRenderer>().material = oilMat;

        oilPuddle.AddComponent<SlipperyFloor>();
    }

    // ==========================================
    // 2.4 MULTIPLE POTHOLE TRAPS (หลุมดักสะดุด 2 จุด)
    // ==========================================
    private void SetupMultiplePotholeTraps()
    {
        if (FindObjectsByType<PotholeTrap>(FindObjectsSortMode.None).Length == 0)
        {
            CreatePotholeTrap("Pothole_Left", ClampToPlayableBounds(new Vector3(-1.8f, 0f, -1.2f), 0.01f), 1.3f);
            CreatePotholeTrap("Pothole_Right", ClampToPlayableBounds(new Vector3(2.2f, 0f, -1.0f), 0.01f), 1.3f);
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
    // 2.4 KITCHEN CAT NPCS (โมเดล Neko Cat Free Edition 3D)
    // ==========================================
    private void SetupKitchenCatNPC()
    {
        if (FindObjectsByType<KitchenCatNPC>(FindObjectsSortMode.None).Length == 0)
        {
            // แมวส้ม 3D (Neko Cat 01) ทางฝั่งขวา
            CreateNekoCat("Cat_NekoOrange", "Neko Cat 01", new Vector3(3.2f, 0f, -2.2f), new Color(0.95f, 0.55f, 0.15f));

            // แมวเทา 3D (Neko Cat 02) ทางฝั่งซ้าย
            CreateNekoCat("Cat_NekoGrey", "Neko Cat 02", new Vector3(-3.2f, 0f, 1.8f), new Color(0.45f, 0.45f, 0.48f));
        }
    }

    private void CreateNekoCat(string name, string prefabResourceName, Vector3 pos, Color fallbackColor)
    {
        GameObject catPrefab = Resources.Load<GameObject>(prefabResourceName);
        GameObject catObj = null;

        if (catPrefab != null)
        {
            catObj = Instantiate(catPrefab);
            catObj.name = name;
            catObj.transform.position = pos;
            catObj.transform.localScale = Vector3.one * 0.35f; // ย่อขนาดตัวให้เป็นลูกแมวน้อยน่ารัก ตัวเล็กกะทัดรัด
        }
        else
        {
            // Fallback กรณีหา Prefab ไม่เจอ
            catObj = new GameObject(name);
            catObj.transform.position = pos;
            catObj.transform.localScale = Vector3.one * 0.35f;

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
            catMat.color = fallbackColor;
            body.GetComponent<MeshRenderer>().material = catMat;
            head.GetComponent<MeshRenderer>().material = catMat;
        }

        // ติดตั้ง Collider สำหรับตรวจจับการเข้าใกล้
        if (!catObj.TryGetComponent(out Collider _))
        {
            CapsuleCollider catCol = catObj.AddComponent<CapsuleCollider>();
            catCol.center = new Vector3(0, 0.25f, 0);
            catCol.radius = 0.25f;
            catCol.height = 0.5f;
        }

        // ติดตั้งจุดคาบอาหาร HoldPoint ด้านหน้าปากแมว
        Transform holdPoint = catObj.transform.Find("HoldPoint");
        if (holdPoint == null)
        {
            GameObject holdObj = new GameObject("HoldPoint");
            holdObj.transform.SetParent(catObj.transform, false);
            holdObj.transform.localPosition = new Vector3(0, 0.45f, 0.4f);
        }

        // ติดตั้งระบบแอนิเมชันเดิน-วิ่งส่ายหางดุ๊กดิ๊ก
        if (!catObj.TryGetComponent(out CatProceduralAnimator _))
        {
            catObj.AddComponent<CatProceduralAnimator>();
        }

        // ติดตั้งสมอง AI ของแมวขโมยของ
        if (!catObj.TryGetComponent(out KitchenCatNPC _))
        {
            catObj.AddComponent<KitchenCatNPC>();
        }
    }

    // ==========================================
    // 2.5 MOVING COUNTER (เคาน์เตอร์เลื่อนตำแหน่ง 2 ตัว)
    // ==========================================
    private void SetupMovingCounters()
    {
        MovingCounter[] existingMovingCounters = FindObjectsByType<MovingCounter>(FindObjectsSortMode.None);
        if (existingMovingCounters.Length < 2)
        {
            ClearCounter[] counters = FindObjectsByType<ClearCounter>(FindObjectsSortMode.None);
            System.Collections.Generic.List<ClearCounter> availableCounters = new System.Collections.Generic.List<ClearCounter>();
            
            foreach (var c in counters)
            {
                if (c.GetComponent<MovingCounter>() == null)
                {
                    availableCounters.Add(c);
                }
            }

            // เคาน์เตอร์เลื่อนตัวที่ 1: เลื่อนตามแนวนอน (ซ้าย-ขวา)
            if (existingMovingCounters.Length == 0 && availableCounters.Count > 0)
            {
                MovingCounter mc1 = availableCounters[0].gameObject.AddComponent<MovingCounter>();
                mc1.Setup(new Vector3(2.2f, 0f, 0f), 1.6f, 0f);
                availableCounters.RemoveAt(0);
            }

            // เคาน์เตอร์เลื่อนตัวที่ 2: เลื่อนตามแนวลึก (หน้า-หลัง) พร้อมจังหวะต่างกัน
            if (FindObjectsByType<MovingCounter>(FindObjectsSortMode.None).Length < 2 && availableCounters.Count > 0)
            {
                int targetIndex = availableCounters.Count > 2 ? 2 : (availableCounters.Count - 1);
                MovingCounter mc2 = availableCounters[targetIndex].gameObject.AddComponent<MovingCounter>();
                mc2.Setup(new Vector3(0f, 0f, 1.8f), 1.4f, 0.7f);
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

    // ==========================================
    // 2.6 COMBO & TIP STREAK HUD
    // ==========================================
    private void SetupComboUISystem()
    {
        if (FindFirstObjectByType<ComboUI>() == null)
        {
            Canvas mainCanvas = FindFirstObjectByType<Canvas>();
            GameObject comboObj = new GameObject("ComboHUD");
            if (mainCanvas != null)
            {
                comboObj.transform.SetParent(mainCanvas.transform, false);
            }
            comboObj.AddComponent<ComboUI>();
        }
    }
}
