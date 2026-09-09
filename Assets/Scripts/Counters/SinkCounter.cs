using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// เคาน์เตอร์อ่างล้างจาน (Sink Counter)
/// 1. รับจานเปื้อน (DirtyPlateKitchenObject) มาวางในอ่าง
/// 2. ตาม Q5 ตัวเลือก A: กดปุ่ม [F] (InteractAlternate) รัวๆ เพื่อขัดล้าง (Interactive Cleaning)
/// 3. เมื่อขัดครบตามกำหนด จานเปื้อนจะกลายเป็นจานสะอาด (PlateKitchenObject) วางบนตะแกรงสะเด็ดน้ำ
/// 4. ผู้เล่นสามารถกด [E] เมื่อมือเปล่าเพื่อหยิบจานสะอาดไปใช้งานต่อ หรือนำไปเก็บที่เคาน์เตอร์จาน
/// </summary>
public class SinkCounter : BaseCounter, IHasProgress, IKitchenObjectParent
{
    public static SinkCounter Instance { get; private set; }

    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event EventHandler OnScrubProgress;
    public event EventHandler OnPlateWashed;

    private const int SCRUBS_PER_PLATE = 4; // กด F 4 ครั้งต่อการล้างจาน 1 ใบ
    private const int MAX_CLEAN_PLATES = 4; // ตะแกรงวางจานสะอาดสะสมได้สูงสุด 4 ใบ
    private const float PLATE_OFFSET_Y = 0.075f;

    [Header("Sink Configuration")]
    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    [Header("Sink State")]
    [SerializeField] private int cleanPlatesCount = 0;
    [SerializeField] private int currentScrubCount = 0;

    // Visual Elements & World Space HUD
    private Transform sinkBasinPoint;
    private Transform dryingRackPoint;
    private readonly List<GameObject> cleanPlateVisualsList = new List<GameObject>();

    private GameObject progressCanvasRoot;
    private Image progressFillImage;
    private TextMeshProUGUI promptLabelText;
    private ParticleSystem bubblesParticleSystem;
    private AudioSource audioSource;
    private Camera targetCamera;

    private void Awake()
    {
        Instance = this;
        EnsureSinkStructureVisuals();
        EnsureWorldProgressUI();
    }

    private void Start()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }

        // ค้นหา PlateKitchenObjectSO จาก PlatesCounter ในฉากอัตโนมัติหากยังไม่ได้ตั้งค่า
        if (plateKitchenObjectSO == null)
        {
            PlatesCounter platesCounter = FindFirstObjectByType<PlatesCounter>();
            if (platesCounter != null)
            {
                plateKitchenObjectSO = platesCounter.GetPlateKitchenObjectSO();
            }
        }

        // ค้นหาและผูก SelectedCounterVisual บนเคาน์เตอร์นี้ให้ชี้มาที่ SinkCounter ตัวใหม่อย่างถูกต้อง
        SelectedCounterVisual visual = GetComponentInChildren<SelectedCounterVisual>();
        if (visual != null)
        {
            visual.SetBaseCounter(this);
        }

        UpdateCleanPlatesVisual();
        UpdateProgressHUD(0f, false);
    }

    private void LateUpdate()
    {
        if (progressCanvasRoot == null) return;

        // ตรวจสอบระยะห่างจากตัวผู้เล่น: หากอยู่ไกลเกิน 2.2 เมตร ให้ซ่อน UI เหมือนกับเคาน์เตอร์อื่นๆ ในครัว
        Player player = Player.Instance;
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist > 2.2f)
            {
                if (progressCanvasRoot.activeSelf)
                {
                    progressCanvasRoot.SetActive(false);
                }
                return;
            }
        }

        // ป้าย Progress Bar หันเข้าหากล้องเสมอ
        if (targetCamera != null && progressCanvasRoot.activeSelf)
        {
            progressCanvasRoot.transform.rotation = targetCamera.transform.rotation;
        }
    }

    public override void Interact(Player player)
    {
        // กรณีที่ 1: ผู้เล่นถือวัตถุมาที่อ่างล้างจาน
        if (player.HasKitchenObject())
        {
            // รับเฉพาะจานเปื้อน (DirtyPlateKitchenObject) เท่านั้น
            if (player.GetKitchenObject() is DirtyPlateKitchenObject incomingDirtyPlates)
            {
                if (!HasKitchenObject())
                {
                    // อ่างว่าง วางกองจานเปื้อนลงในอ่าง
                    incomingDirtyPlates.SetKitchenObjectParent(this);
                    currentScrubCount = 0;
                    UpdateProgressHUD(0f, true);

                    OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                    {
                        progressNormalized = 0f
                    });

                    Debug.Log($"🧽 [SinkCounter] Placed {incomingDirtyPlates.GetPlatesCount()} dirty plates in the sink!");
                }
                else if (GetKitchenObject() is DirtyPlateKitchenObject existingDirtyPlates)
                {
                    // มีจานเปื้อนในอ่างอยู่แล้ว รวมกองจานเปื้อนเข้าด้วยกัน
                    while (incomingDirtyPlates.GetPlatesCount() > 0 && existingDirtyPlates.TryAddPlate())
                    {
                        incomingDirtyPlates.SetPlatesCount(incomingDirtyPlates.GetPlatesCount() - 1);
                    }

                    if (incomingDirtyPlates.GetPlatesCount() <= 0)
                    {
                        incomingDirtyPlates.DestroySelf();
                    }
                }
            }
            return;
        }

        // กรณีที่ 2: ผู้เล่นมือเปล่า
        // 2.1 หากมีจานสะอาดที่ล้างเสร็จแล้ววางอยู่บนตะแกรง ให้หยิบจานสะอาดก่อน
        if (cleanPlatesCount > 0)
        {
            if (plateKitchenObjectSO != null)
            {
                cleanPlatesCount--;
                UpdateCleanPlatesVisual();

                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                Debug.Log($"✨ [SinkCounter] Player picked up a clean plate! ({cleanPlatesCount} remaining on rack)");
            }
            return;
        }

        // 2.2 หากไม่มีจานสะอาด แต่มีจานเปื้อนวางคาอยู่ในอ่าง ให้ผู้เล่นหยิบจานเปื้อนกลับคืนได้
        if (HasKitchenObject() && GetKitchenObject() is DirtyPlateKitchenObject heldDirtyPlates)
        {
            heldDirtyPlates.SetKitchenObjectParent(player);
            currentScrubCount = 0;
            UpdateProgressHUD(0f, false);

            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
            {
                progressNormalized = 0f
            });
        }
    }

    public override void InteractAlternate(Player player)
    {
        // ทำงานเฉพาะเมื่อมีจานเปื้อนอยู่ในอ่างล้างจาน
        if (!HasKitchenObject()) return;
        if (!(GetKitchenObject() is DirtyPlateKitchenObject dirtyPlates)) return;

        // Q1 ตัวเลือก A: ล็อคการกดขัดล้างทันทีเมื่อตะแกรงสะเด็ดน้ำเต็ม 4 ใบ ป้องกันจานหายในอากาศ 100%
        if (cleanPlatesCount >= MAX_CLEAN_PLATES)
        {
            UpdateProgressHUD(0f, true);
            Debug.Log("⚠️ [SinkCounter] Drying rack is FULL (4/4)! Pick up clean plates with [E] before scrubbing more.");
            return;
        }

        // กด [F] เพื่อขัดล้างจาน (Interactive Cleaning)
        currentScrubCount++;

        // แสดงเอฟเฟกต์ฟองสบู่และน้ำกระเซ็น
        TriggerWashFX();

        float progress = (float)currentScrubCount / SCRUBS_PER_PLATE;
        float progressNormalized = Mathf.Clamp01(progress);

        UpdateProgressHUD(progressNormalized, true);

        OnScrubProgress?.Invoke(this, EventArgs.Empty);
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = progressNormalized
        });

        // เมื่อขัดครบ 4 ครั้ง -> จานเปื้อน 1 ใบกลายเป็นจานสะอาด
        if (currentScrubCount >= SCRUBS_PER_PLATE)
        {
            currentScrubCount = 0;
            cleanPlatesCount = Mathf.Min(cleanPlatesCount + 1, MAX_CLEAN_PLATES);
            UpdateCleanPlatesVisual();

            OnPlateWashed?.Invoke(this, EventArgs.Empty);

            int remainingDirty = dirtyPlates.GetPlatesCount() - 1;
            if (remainingDirty > 0)
            {
                // ยังมีจานเปื้อนเหลือในกอง ขัดล้างใบต่อไป
                dirtyPlates.SetPlatesCount(remainingDirty);
                UpdateProgressHUD(0f, true);

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });

                Debug.Log($"🧼 [SinkCounter] 1 plate cleaned! {remainingDirty} dirty plates left in sink.");
            }
            else
            {
                // ล้างจานเปื้อนในกองหมดเกลี้ยงแล้ว
                dirtyPlates.DestroySelf();
                UpdateProgressHUD(0f, false);

                OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
                {
                    progressNormalized = 0f
                });

                Debug.Log("🎉 [SinkCounter] All dirty plates washed successfully!");
            }
        }
    }

    /// <summary>
    /// เล่นเอฟเฟกต์ละอองฟองสบู่และเสียงน้ำขัดจาน
    /// </summary>
    private void TriggerWashFX()
    {
        if (bubblesParticleSystem != null)
        {
            bubblesParticleSystem.Emit(8);
        }

        if (audioSource != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.2f);
            audioSource.Play();
        }
    }

    /// <summary>
    /// อัปเดตการแสดงผลหลอด Progress Bar และข้อความปุ่มกดบนหัวอ่างล้างจาน
    /// </summary>
    private void UpdateProgressHUD(float normalized, bool isActive)
    {
        if (progressCanvasRoot == null) return;

        if (!isActive || (!HasKitchenObject() && cleanPlatesCount == 0))
        {
            progressCanvasRoot.SetActive(false);
            return;
        }

        progressCanvasRoot.SetActive(true);

        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = normalized;
        }

        if (promptLabelText != null)
        {
            if (HasKitchenObject() && GetKitchenObject() is DirtyPlateKitchenObject dirty)
            {
                if (cleanPlatesCount >= MAX_CLEAN_PLATES)
                {
                    promptLabelText.text = $"<color=#FF9100><b>⚠️ RACK FULL! PICK UP [E]</b></color>\n<size=75%>Rack Full ({cleanPlatesCount}/{MAX_CLEAN_PLATES}) | Sink: {dirty.GetPlatesCount()}</size>";
                }
                else
                {
                    promptLabelText.text = $"<color=#00E5FF><b>[F] SCRUB ({currentScrubCount}/{SCRUBS_PER_PLATE})</b></color>\n<size=75%>Plates in Sink: {dirty.GetPlatesCount()} (Rack: {cleanPlatesCount}/{MAX_CLEAN_PLATES})</size>";
                }
            }
            else if (cleanPlatesCount > 0)
            {
                promptLabelText.text = $"<color=#76FF03><b>[E] PICK CLEAN PLATE</b></color>\n<size=75%>Clean Plates: {cleanPlatesCount}/{MAX_CLEAN_PLATES}</size>";
            }
        }
    }

    /// <summary>
    /// อัปเดตโมเดล 3D กองจานสะอาดที่ล้างเสร็จแล้วบนตะแกรงสะเด็ดน้ำ
    /// </summary>
    private void UpdateCleanPlatesVisual()
    {
        if (dryingRackPoint == null) return;

        // ล้างโมเดลเดิม
        for (int i = cleanPlateVisualsList.Count - 1; i >= 0; i--)
        {
            if (cleanPlateVisualsList[i] != null)
            {
                Destroy(cleanPlateVisualsList[i]);
            }
        }
        cleanPlateVisualsList.Clear();

        if (cleanPlatesCount <= 0)
        {
            if (!HasKitchenObject()) UpdateProgressHUD(0f, false);
            return;
        }

        // สร้างโมเดลจานเซรามิกสีขาวสะอาดเงางามซ้อนกันบนตะแกรง
        for (int i = 0; i < cleanPlatesCount; i++)
        {
            GameObject plateObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plateObj.name = $"CleanPlate_{i + 1}";
            plateObj.transform.SetParent(dryingRackPoint, false);
            plateObj.transform.localPosition = new Vector3(0f, i * PLATE_OFFSET_Y, 0f);
            plateObj.transform.localScale = new Vector3(0.65f, 0.035f, 0.65f);

            if (plateObj.TryGetComponent(out Collider col)) Destroy(col);

            // เซรามิกสีขาวบริสุทธิ์ สะอาด ไร้คราบ
            Material cleanPlateMat = FireExtinguisher.GetSafeMaterial(new Color(0.96f, 0.96f, 0.98f), 0.15f, 0.85f);
            plateObj.GetComponent<MeshRenderer>().material = cleanPlateMat;

            cleanPlateVisualsList.Add(plateObj);
        }

        if (!HasKitchenObject())
        {
            UpdateProgressHUD(1f, true);
        }
    }

    /// <summary>
    /// สร้างโมเดลอ่างล้างจาน ก๊อกน้ำ และตะแกรงสะเด็ดน้ำแบบ Procedural
    /// </summary>
    private void EnsureSinkStructureVisuals()
    {
        // 1. จุดวางจานเปื้อนในอ่าง (ฝั่งซ้ายของเคาน์เตอร์)
        Transform basinPoint = transform.Find("SinkBasinPoint");
        if (basinPoint != null)
        {
            sinkBasinPoint = basinPoint;
        }
        else
        {
            GameObject basinObj = new GameObject("SinkBasinPoint");
            basinObj.transform.SetParent(transform, false);
            basinObj.transform.localPosition = new Vector3(-0.25f, 1.28f, 0f);
            sinkBasinPoint = basinObj.transform;
        }

        // 2. ตะแกรงสะเด็ดน้ำสำหรับวางจานสะอาด (ฝั่งขวาของเคาน์เตอร์)
        Transform rackPoint = transform.Find("DryingRackPoint");
        if (rackPoint != null)
        {
            dryingRackPoint = rackPoint;
        }
        else
        {
            GameObject rackObj = new GameObject("DryingRackPoint");
            rackObj.transform.SetParent(transform, false);
            rackObj.transform.localPosition = new Vector3(0.32f, 1.28f, 0f);
            dryingRackPoint = rackObj.transform;

            // ตะแกรงโลหะสีเทา
            GameObject rackMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rackMesh.name = "RackMesh";
            rackMesh.transform.SetParent(dryingRackPoint, false);
            rackMesh.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            rackMesh.transform.localScale = new Vector3(0.68f, 0.03f, 0.68f);
            if (rackMesh.TryGetComponent(out Collider rc)) Destroy(rc);
            rackMesh.GetComponent<MeshRenderer>().material = FireExtinguisher.GetSafeMaterial(new Color(0.45f, 0.48f, 0.52f), 0.8f, 0.8f);
        }

        // 3. ก๊อกน้ำสแตนเลส (Faucet)
        if (transform.Find("SinkFaucet") == null)
        {
            GameObject faucetRoot = new GameObject("SinkFaucet");
            faucetRoot.transform.SetParent(transform, false);
            faucetRoot.transform.localPosition = new Vector3(-0.25f, 1.3f, 0.35f);

            // เสาก๊อก
            GameObject stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stem.transform.SetParent(faucetRoot.transform, false);
            stem.transform.localPosition = new Vector3(0f, 0.18f, 0f);
            stem.transform.localScale = new Vector3(0.06f, 0.18f, 0.06f);
            if (stem.TryGetComponent(out Collider sc)) Destroy(sc);
            Material chromeMat = FireExtinguisher.GetSafeMaterial(new Color(0.85f, 0.88f, 0.92f), 0.95f, 0.9f);
            stem.GetComponent<MeshRenderer>().material = chromeMat;

            // ปากก๊อกน้ำยื่นออกมา
            GameObject spout = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spout.transform.SetParent(faucetRoot.transform, false);
            spout.transform.localPosition = new Vector3(0f, 0.35f, -0.1f);
            spout.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);
            spout.transform.localScale = new Vector3(0.05f, 0.12f, 0.05f);
            if (spout.TryGetComponent(out Collider spc)) Destroy(spc);
            spout.GetComponent<MeshRenderer>().material = chromeMat;
        }

        // 4. ผิวน้ำในอ่างล้างจานสีฟ้าใส
        if (transform.Find("SinkWaterSurface") == null)
        {
            GameObject waterObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            waterObj.name = "SinkWaterSurface";
            waterObj.transform.SetParent(transform, false);
            waterObj.transform.localPosition = new Vector3(-0.25f, 1.22f, 0f);
            waterObj.transform.localScale = new Vector3(0.65f, 0.04f, 0.65f);
            if (waterObj.TryGetComponent(out Collider wc)) Destroy(wc);

            Material waterMat = FireExtinguisher.GetSafeMaterial(new Color(0.1f, 0.65f, 0.92f, 0.75f), 0.2f, 0.95f);
            waterObj.GetComponent<MeshRenderer>().material = waterMat;
        }

        // 5. ติดตั้งระบบอนุภาคฟองสบู่ (Soap Foam Particles)
        if (bubblesParticleSystem == null)
        {
            GameObject psObj = new GameObject("WashFoamParticles");
            psObj.transform.SetParent(transform, false);
            psObj.transform.localPosition = new Vector3(-0.25f, 1.35f, 0f);

            bubblesParticleSystem = psObj.AddComponent<ParticleSystem>();
            var main = bubblesParticleSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 1.2f;
            main.startSize = 0.14f;
            main.startColor = new Color(0.92f, 0.98f, 1.0f, 0.85f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = bubblesParticleSystem.emission;
            emission.rateOverTime = 0;

            var shape = bubblesParticleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.22f;

            ParticleSystemRenderer psRenderer = psObj.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = FireExtinguisher.GetSafeMaterial(Color.white, 0f, 0.9f);
        }

        // 6. AudioSource จำลองเสียงน้ำ
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
            audioSource.maxDistance = 12f;
            audioSource.volume = 0.55f;
        }
    }

    /// <summary>
    /// สร้าง World Space UI สำหรับแสดง Progress Bar และปุ่มกด
    /// </summary>
    private void EnsureWorldProgressUI()
    {
        if (progressCanvasRoot != null) return;

        Transform existingCanvas = transform.Find("SinkProgressCanvas");
        if (existingCanvas != null)
        {
            progressCanvasRoot = existingCanvas.gameObject;
            progressFillImage = existingCanvas.Find("ProgressBG/ProgressFill")?.GetComponent<Image>();
            promptLabelText = existingCanvas.Find("PromptLabel")?.GetComponent<TextMeshProUGUI>();
            return;
        }

        GameObject canvasObj = new GameObject("SinkProgressCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0f, 2.05f, 0f);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 30;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(260, 90);
        canvasRect.localScale = Vector3.one * 0.005f;

        // Background Bar
        GameObject bgObj = new GameObject("ProgressBG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.35f);
        bgRect.anchorMax = new Vector2(0.5f, 0.35f);
        bgRect.sizeDelta = new Vector2(220, 24);

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.12f, 0.12f, 0.15f, 0.92f);

        // Fill Bar
        GameObject fillObj = new GameObject("ProgressFill");
        fillObj.transform.SetParent(bgObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = new Vector2(-4, -4);

        progressFillImage = fillObj.AddComponent<Image>();
        progressFillImage.type = Image.Type.Filled;
        progressFillImage.fillMethod = Image.FillMethod.Horizontal;
        progressFillImage.fillAmount = 0f;
        progressFillImage.color = new Color(0.0f, 0.85f, 1.0f); // ฟ้าครามสดใส

        // Prompt Text
        GameObject textObj = new GameObject("PromptLabel");
        textObj.transform.SetParent(canvasObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.75f);
        textRect.anchorMax = new Vector2(0.5f, 0.75f);
        textRect.sizeDelta = new Vector2(240, 50);

        promptLabelText = textObj.AddComponent<TextMeshProUGUI>();
        promptLabelText.fontSize = 24;
        promptLabelText.alignment = TextAlignmentOptions.Center;
        promptLabelText.color = Color.white;
        promptLabelText.fontStyle = FontStyles.Bold;
        promptLabelText.text = "";

        progressCanvasRoot = canvasObj;
        progressCanvasRoot.SetActive(false);
    }

    /// <summary>
    /// ส่งจุดวางวัตถุ (วางในอ่างล้างจาน)
    /// </summary>
    public new Transform GetKitchenObjectFollowTranform()
    {
        return sinkBasinPoint != null ? sinkBasinPoint : transform;
    }

    Transform IKitchenObjectParent.GetKitchenObjectFollowTranform()
    {
        return sinkBasinPoint != null ? sinkBasinPoint : transform;
    }
}
