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
        // 2.1 หากมีจานสะอาดที่ล้างเสร็จแล้ววางอยู่บนตะแกรง ให้ยกกองจานสะอาดทั้งหมดไปเก็บ (Q1 ตัวเลือก A)
        if (cleanPlatesCount > 0)
        {
            if (plateKitchenObjectSO != null)
            {
                int platesToTake = cleanPlatesCount;
                cleanPlatesCount = 0;
                UpdateCleanPlatesVisual();

                KitchenObject spawnedKO = KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                if (spawnedKO.TryGetPlate(out PlateKitchenObject cleanPlateStack))
                {
                    cleanPlateStack.SetStackCount(platesToTake);
                }

                if (!HasKitchenObject())
                {
                    UpdateProgressHUD(0f, false);
                }

                Debug.Log($"✨ [SinkCounter] Player picked up a clean plate stack of {platesToTake} plates!");
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
        DirtyPlateKitchenObject dirtyPlates = GetKitchenObject() as DirtyPlateKitchenObject;
        if (dirtyPlates == null) return;

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
                    promptLabelText.text = UITheme.FormatRackFullPrompt(cleanPlatesCount, MAX_CLEAN_PLATES, dirty.GetPlatesCount());
                }
                else
                {
                    promptLabelText.text = UITheme.FormatScrubPrompt(currentScrubCount, SCRUBS_PER_PLATE, dirty.GetPlatesCount(), cleanPlatesCount, MAX_CLEAN_PLATES);
                }
            }
            else if (cleanPlatesCount > 0)
            {
                promptLabelText.text = UITheme.FormatPickCleanPlatePrompt(cleanPlatesCount, MAX_CLEAN_PLATES);
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
    /// ผู้ช่วยสร้าง GameObject Primitive แบบ 3D Visual พร้อมปลด Collider ออกอัตโนมัติ เพื่อไม่ให้ขัดขวางการตรวจจับ Raycast ของผู้เล่น
    /// </summary>
    private GameObject CreateVisualPrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = localScale;
        obj.transform.localRotation = localRot;

        if (obj.TryGetComponent(out Collider c))
        {
            Destroy(c);
        }

        if (mat != null && obj.TryGetComponent(out MeshRenderer mr))
        {
            mr.material = mat;
        }

        return obj;
    }

    /// <summary>
    /// แปลงโฉมเคาน์เตอร์ให้เป็น "อ่างล้างจานสแตนเลสเชิงพาณิชย์ครบวงจร (Commercial Stainless Steel Sink Station)"
    /// ลบภาพเคาน์เตอร์ไม้เดิมออก 100% พร้อมติดตั้ง:
    /// 1. ท็อปโต๊ะสแตนเลสขัดเงา (Polished Stainless Steel Worktop) คลุมมิดชิด
    /// 2. แผงตู้หน้าสแตนเลสพร้อมบานเปิดคู่ มือจับโครเมียม และแผ่นกันเตะ (Apron, Double Doors & Kickplate)
    /// 3. แผงกันน้ำกระเซ็นทรงสูงด้านหลัง (Commercial High Backsplash Guard)
    /// 4. อ่างล้างจานหลุมลึก (Deep Recessed Wash Basin) พร้อมสะดืออ่าง ตะแกรงระบายน้ำ ผิวน้ำประกายใส และฟองสบู่
    /// 5. ก๊อกน้ำสปริงคอห่านเชิงพาณิชย์ขนาดใหญ่ (Pre-Rinse Gooseneck Faucet) พร้อมหัวฉีดสเปรย์ วาล์วปรับน้ำร้อน-เย็น และละอองน้ำหยด
    /// 6. ตะแกรงสะเด็ดน้ำสแตนเลสมีซี่ร่องระบายน้ำ (Corrugated Wire Drying Rack & Slanted Drainboard)
    /// 7. อุปกรณ์ทำความสะอาดสมจริง: ฟองน้ำสก๊อตช์ไบรต์ 2 ชั้น (เหลือง-เขียว) และขวดน้ำยาล้างจานหัวปั๊มสีเขียวมรกต
    /// 8. ป้ายชื่อสเตชั่น World Space Signboard ("🧼 SINK STATION") ชัดเจนจากทุกมุมมอง
    /// </summary>
    private void EnsureSinkStructureVisuals()
    {
        // 0. ปิดการแสดงผลโมเดลเคาน์เตอร์ไม้เดิม (ClearCounter_Visual) เพื่อไม่ให้สีไม้หรือขอบไม้โผล่ออกมา
        Transform clearVisual = transform.Find("ClearCounter_Visual");
        if (clearVisual != null)
        {
            clearVisual.gameObject.SetActive(false);
        }

        // ตรวจสอบและปิด MeshRenderer ของเคาน์เตอร์ไม้เดิมที่อาจหลงเหลืออยู่ (ยกเว้น Selected และระบบ Sink)
        foreach (Transform child in transform)
        {
            if (child == null) continue;
            string cName = child.name;
            if (cName == "Selected" || cName.StartsWith("Sink") || cName.StartsWith("Drying") || 
                cName.StartsWith("Wash") || cName.StartsWith("Prompt") || cName.StartsWith("Progress") || 
                cName.StartsWith("Fire") || cName.StartsWith("Locked"))
            {
                continue;
            }

            if (cName.Contains("Visual") || cName.Contains("Counter"))
            {
                if (child.TryGetComponent(out MeshRenderer mr)) mr.enabled = false;
                foreach (MeshRenderer cmr in child.GetComponentsInChildren<MeshRenderer>())
                {
                    cmr.enabled = false;
                }
            }
        }

        // 1. จุดวางจานเปื้อนในอ่าง (ฝั่งซ้ายของเคาน์เตอร์ จมลงไปในหลุมอ่างสมจริง)
        Transform basinPoint = transform.Find("SinkBasinPoint");
        if (basinPoint != null)
        {
            sinkBasinPoint = basinPoint;
        }
        else
        {
            GameObject basinObj = new GameObject("SinkBasinPoint");
            basinObj.transform.SetParent(transform, false);
            basinObj.transform.localPosition = new Vector3(-0.30f, 1.21f, 0f);
            sinkBasinPoint = basinObj.transform;
        }

        // 2. จุดวางจานสะอาดบนตะแกรงสะเด็ดน้ำ (ฝั่งขวาของเคาน์เตอร์)
        Transform rackPoint = transform.Find("DryingRackPoint");
        if (rackPoint != null)
        {
            dryingRackPoint = rackPoint;
        }
        else
        {
            GameObject rackObj = new GameObject("DryingRackPoint");
            rackObj.transform.SetParent(transform, false);
            rackObj.transform.localPosition = new Vector3(0.33f, 1.265f, 0f);
            dryingRackPoint = rackObj.transform;
        }

        // หากเคยสร้างโครงสร้าง SinkStationVisual ไว้แล้ว ให้ข้ามการสร้างซ้ำ
        Transform existingStation = transform.Find("SinkStationVisual");
        if (existingStation == null)
        {
            GameObject stationRoot = new GameObject("SinkStationVisual");
            stationRoot.transform.SetParent(transform, false);

            // ==========================================
            // เตรียม Materials คุณภาพสูงสำหรับสเตชั่นสแตนเลส
            // ==========================================
            Material stainlessSteelMat = FireExtinguisher.GetSafeMaterial(new Color(0.86f, 0.89f, 0.93f), 0.94f, 0.88f);
            Material polishedSteelMat = FireExtinguisher.GetSafeMaterial(new Color(0.92f, 0.94f, 0.97f), 0.96f, 0.94f);
            Material darkTrimMat = FireExtinguisher.GetSafeMaterial(new Color(0.32f, 0.35f, 0.38f), 0.85f, 0.70f);
            Material chromeMat = FireExtinguisher.GetSafeMaterial(new Color(0.95f, 0.96f, 0.98f), 0.98f, 0.96f);
            Material wetSteelMat = FireExtinguisher.GetSafeMaterial(new Color(0.68f, 0.72f, 0.78f), 0.85f, 0.92f);
            Material waterMat = FireExtinguisher.GetSafeMaterial(new Color(0.12f, 0.75f, 0.95f, 0.80f), 0.15f, 0.98f);
            Material foamMat = FireExtinguisher.GetSafeMaterial(new Color(0.96f, 0.98f, 1.0f, 0.92f), 0.05f, 0.50f);
            Material yellowSpongeMat = FireExtinguisher.GetSafeMaterial(new Color(0.98f, 0.82f, 0.12f), 0.05f, 0.30f);
            Material greenScourMat = FireExtinguisher.GetSafeMaterial(new Color(0.10f, 0.48f, 0.18f), 0.05f, 0.25f);
            Material soapBottleMat = FireExtinguisher.GetSafeMaterial(new Color(0.12f, 0.85f, 0.35f, 0.85f), 0.20f, 0.92f);
            Material whitePlasticMat = FireExtinguisher.GetSafeMaterial(new Color(0.95f, 0.95f, 0.95f), 0.10f, 0.80f);
            Material redValveMat = FireExtinguisher.GetSafeMaterial(new Color(0.92f, 0.18f, 0.18f), 0.60f, 0.75f);
            Material blueValveMat = FireExtinguisher.GetSafeMaterial(new Color(0.18f, 0.45f, 0.95f), 0.60f, 0.75f);

            // ==========================================
            // [A] ตัวตู้เคาน์เตอร์และท็อปสแตนเลส (Base Cabinet & Stainless Countertop)
            // ==========================================
            // ตู้ฐานหลักสแตนเลสทั้งตัว
            CreateVisualPrimitive(PrimitiveType.Cube, "StationBaseBody", stationRoot.transform,
                new Vector3(0f, 0.61f, 0f), new Vector3(1.36f, 1.22f, 1.36f), Quaternion.identity, stainlessSteelMat);

            // ท็อปโต๊ะสแตนเลสเงางาม คลุมด้านบนทั้งหมด
            CreateVisualPrimitive(PrimitiveType.Cube, "StationTopPlate", stationRoot.transform,
                new Vector3(0f, 1.23f, 0f), new Vector3(1.42f, 0.06f, 1.42f), Quaternion.identity, polishedSteelMat);

            // แผงหน้าตู้และประตูคู่ (Double Doors หันหน้าเข้าหาตัวเชฟ +Z)
            CreateVisualPrimitive(PrimitiveType.Cube, "LeftDoorPanel", stationRoot.transform,
                new Vector3(-0.32f, 0.60f, 0.69f), new Vector3(0.62f, 0.85f, 0.03f), Quaternion.identity, polishedSteelMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "RightDoorPanel", stationRoot.transform,
                new Vector3(0.32f, 0.60f, 0.69f), new Vector3(0.62f, 0.85f, 0.03f), Quaternion.identity, polishedSteelMat);

            // มือจับประตูโครเมียมทรงกระบอกแนวตั้ง (Vertical Chrome Door Handles)
            CreateVisualPrimitive(PrimitiveType.Cylinder, "LeftDoorHandle", stationRoot.transform,
                new Vector3(-0.08f, 0.72f, 0.71f), new Vector3(0.025f, 0.12f, 0.025f), Quaternion.identity, chromeMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "RightDoorHandle", stationRoot.transform,
                new Vector3(0.08f, 0.72f, 0.71f), new Vector3(0.025f, 0.12f, 0.025f), Quaternion.identity, chromeMat);

            // แผ่นกันเตะฐานตู้สีเข้มด้านล่าง (Kickplate)
            CreateVisualPrimitive(PrimitiveType.Cube, "StationKickplate", stationRoot.transform,
                new Vector3(0f, 0.07f, 0.67f), new Vector3(1.36f, 0.12f, 0.04f), Quaternion.identity, darkTrimMat);

            // ==========================================
            // [B] แผงกันน้ำกระเซ็นทรงสูงด้านหลัง (Commercial High Backsplash Guard แนบผนังด้านหลัง -Z)
            // ==========================================
            CreateVisualPrimitive(PrimitiveType.Cube, "BacksplashWall", stationRoot.transform,
                new Vector3(0f, 1.48f, -0.67f), new Vector3(1.42f, 0.46f, 0.06f), Quaternion.identity, polishedSteelMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "BacksplashTrimLip", stationRoot.transform,
                new Vector3(0f, 1.71f, -0.67f), new Vector3(1.44f, 0.04f, 0.08f), Quaternion.identity, darkTrimMat);

            // ==========================================
            // [C] หลุมอ่างล้างจานสแตนเลส (Deep Recessed Wash Basin - ฝั่งซ้าย)
            // ==========================================
            // ก้นหลุมอ่างลึก
            CreateVisualPrimitive(PrimitiveType.Cube, "BasinFloor", stationRoot.transform,
                new Vector3(-0.30f, 1.14f, 0f), new Vector3(0.62f, 0.02f, 0.62f), Quaternion.identity, wetSteelMat);

            // ขอบอ่างยกสูง 4 ด้าน (Front, Back, Left, Right Rim Walls)
            CreateVisualPrimitive(PrimitiveType.Cube, "BasinFrontRim", stationRoot.transform,
                new Vector3(-0.30f, 1.22f, 0.31f), new Vector3(0.66f, 0.14f, 0.04f), Quaternion.identity, polishedSteelMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "BasinBackRim", stationRoot.transform,
                new Vector3(-0.30f, 1.22f, -0.31f), new Vector3(0.66f, 0.14f, 0.04f), Quaternion.identity, polishedSteelMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "BasinLeftRim", stationRoot.transform,
                new Vector3(-0.61f, 1.22f, 0f), new Vector3(0.04f, 0.14f, 0.66f), Quaternion.identity, polishedSteelMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "BasinCenterDivider", stationRoot.transform,
                new Vector3(0.01f, 1.22f, 0f), new Vector3(0.04f, 0.14f, 0.66f), Quaternion.identity, polishedSteelMat);

            // สะดืออ่างและตะแกรงระบายน้ำทรงกลม (Circular Drain Strainer)
            CreateVisualPrimitive(PrimitiveType.Cylinder, "DrainStrainerRing", stationRoot.transform,
                new Vector3(-0.30f, 1.155f, 0f), new Vector3(0.15f, 0.015f, 0.15f), Quaternion.identity, chromeMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "DrainHoleCore", stationRoot.transform,
                new Vector3(-0.30f, 1.158f, 0f), new Vector3(0.08f, 0.02f, 0.08f), Quaternion.identity, darkTrimMat);

            // ผิวน้ำประกายใสในอ่าง (Water Surface)
            CreateVisualPrimitive(PrimitiveType.Cube, "SinkWaterSurface", stationRoot.transform,
                new Vector3(-0.30f, 1.19f, 0f), new Vector3(0.58f, 0.015f, 0.58f), Quaternion.identity, waterMat);

            // ฟองสบู่ขาวนวลลอยบนผิวน้ำ 4 จุด (Surface Soap Foam Clusters)
            CreateVisualPrimitive(PrimitiveType.Cylinder, "FoamCluster1", stationRoot.transform,
                new Vector3(-0.42f, 1.20f, -0.15f), new Vector3(0.12f, 0.01f, 0.12f), Quaternion.identity, foamMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "FoamCluster2", stationRoot.transform,
                new Vector3(-0.18f, 1.20f, 0.15f), new Vector3(0.10f, 0.01f, 0.10f), Quaternion.identity, foamMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "FoamCluster3", stationRoot.transform,
                new Vector3(-0.40f, 1.20f, 0.18f), new Vector3(0.09f, 0.01f, 0.09f), Quaternion.identity, foamMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "FoamCluster4", stationRoot.transform,
                new Vector3(-0.20f, 1.20f, -0.18f), new Vector3(0.11f, 0.01f, 0.11f), Quaternion.identity, foamMat);

            // ==========================================
            // [D] ก๊อกน้ำสปริงคอห่านเชิงพาณิชย์ (Commercial Pre-Rinse Spring Gooseneck Faucet ติดตั้งชิดผนังหลัง -Z โค้งพุ่งมาข้างหน้า +Z)
            // ==========================================
            GameObject faucetRoot = new GameObject("SinkFaucet");
            faucetRoot.transform.SetParent(stationRoot.transform, false);
            faucetRoot.transform.localPosition = new Vector3(-0.30f, 1.26f, -0.38f);

            // ฐานยึดก๊อกโครเมียม
            CreateVisualPrimitive(PrimitiveType.Cylinder, "FaucetBase", faucetRoot.transform,
                Vector3.zero, new Vector3(0.11f, 0.035f, 0.11f), Quaternion.identity, chromeMat);

            // วาล์วน้ำร้อนสีแดง และ วาล์วน้ำเย็นสีน้ำเงิน
            CreateVisualPrimitive(PrimitiveType.Cylinder, "HotWaterValve", faucetRoot.transform,
                new Vector3(-0.08f, 0.04f, 0f), new Vector3(0.035f, 0.04f, 0.035f), Quaternion.identity, redValveMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "ColdWaterValve", faucetRoot.transform,
                new Vector3(0.08f, 0.04f, 0f), new Vector3(0.035f, 0.04f, 0.035f), Quaternion.identity, blueValveMat);

            // เสาท่อโครเมียมทรงสูง
            CreateVisualPrimitive(PrimitiveType.Cylinder, "FaucetRiser", faucetRoot.transform,
                new Vector3(0f, 0.22f, 0f), new Vector3(0.045f, 0.22f, 0.045f), Quaternion.identity, chromeMat);

            // สปริงโลหะเสริมความแข็งแรง (Heavy Chrome Spring Coil)
            CreateVisualPrimitive(PrimitiveType.Cylinder, "SpringCoil", faucetRoot.transform,
                new Vector3(0f, 0.26f, 0f), new Vector3(0.065f, 0.14f, 0.065f), Quaternion.identity, darkTrimMat);

            // คอก๊อกโค้งงอพุ่งไปข้างหน้าหาตัวเชฟ (+Z)
            CreateVisualPrimitive(PrimitiveType.Cylinder, "ArchSegment1", faucetRoot.transform,
                new Vector3(0f, 0.44f, 0.10f), new Vector3(0.04f, 0.12f, 0.04f), Quaternion.Euler(-45f, 0f, 0f), chromeMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "ArchSegment2", faucetRoot.transform,
                new Vector3(0f, 0.42f, 0.22f), new Vector3(0.04f, 0.10f, 0.04f), Quaternion.Euler(-85f, 0f, 0f), chromeMat);

            // หัวฉีดสเปรย์ทรงกระดิ่งชี้ตรงลงสู่อ่าง (Pre-Rinse Bell Spray Nozzle)
            CreateVisualPrimitive(PrimitiveType.Cylinder, "SprayBell", faucetRoot.transform,
                new Vector3(0f, 0.32f, 0.28f), new Vector3(0.065f, 0.05f, 0.065f), Quaternion.identity, chromeMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "SprayBumperRing", faucetRoot.transform,
                new Vector3(0f, 0.28f, 0.28f), new Vector3(0.075f, 0.015f, 0.075f), Quaternion.identity, darkTrimMat);

            // ก้านบีบหัวสเปรย์ (Squeeze Lever Handle)
            CreateVisualPrimitive(PrimitiveType.Cube, "SqueezeLever", faucetRoot.transform,
                new Vector3(0f, 0.36f, 0.24f), new Vector3(0.02f, 0.08f, 0.02f), Quaternion.Euler(25f, 0f, 0f), darkTrimMat);

            // ==========================================
            // [E] ตะแกรงสะเด็ดน้ำสแตนเลส (Corrugated Wire Drying Rack & Drainboard - ฝั่งขวา)
            // ==========================================
            // ถาดรองน้ำลาดเอียงระบายลงอ่าง
            CreateVisualPrimitive(PrimitiveType.Cube, "DrainboardBed", stationRoot.transform,
                new Vector3(0.33f, 1.23f, 0f), new Vector3(0.60f, 0.03f, 0.64f), Quaternion.identity, polishedSteelMat);

            // โครงกรอบนอกตะแกรงสะเด็ดน้ำ (Outer Tubular Metal Frame)
            CreateVisualPrimitive(PrimitiveType.Cube, "RackOuterFront", stationRoot.transform,
                new Vector3(0.33f, 1.255f, 0.31f), new Vector3(0.60f, 0.02f, 0.02f), Quaternion.identity, chromeMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "RackOuterBack", stationRoot.transform,
                new Vector3(0.33f, 1.255f, -0.31f), new Vector3(0.60f, 0.02f, 0.02f), Quaternion.identity, chromeMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "RackOuterLeft", stationRoot.transform,
                new Vector3(0.04f, 1.255f, 0f), new Vector3(0.02f, 0.02f, 0.64f), Quaternion.identity, chromeMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "RackOuterRight", stationRoot.transform,
                new Vector3(0.62f, 1.255f, 0f), new Vector3(0.02f, 0.02f, 0.64f), Quaternion.identity, chromeMat);

            // ซี่ตะแกรงสแตนเลส 6 เส้นขนานกัน (6 Parallel Wire Slats)
            float[] slatXCoords = new float[] { 0.12f, 0.20f, 0.28f, 0.36f, 0.44f, 0.52f };
            for (int i = 0; i < slatXCoords.Length; i++)
            {
                CreateVisualPrimitive(PrimitiveType.Cube, $"RackWireSlat_{i + 1}", stationRoot.transform,
                    new Vector3(slatXCoords[i], 1.252f, 0f), new Vector3(0.015f, 0.015f, 0.58f), Quaternion.identity, chromeMat);
            }

            // ==========================================
            // [F] อุปกรณ์ทำความสะอาดสมจริง (Realistic Cleaning Props)
            // ==========================================
            // 1. ฟองน้ำล้างจาน 2 ชั้นแบบ Scotch-Brite (ฐานฟองน้ำสีเหลืองสด + แผ่นใยขัดสีเขียวเข้ม) วางด้านหน้าขอบอ่าง
            CreateVisualPrimitive(PrimitiveType.Cube, "SpongeYellowBase", stationRoot.transform,
                new Vector3(0.02f, 1.255f, 0.24f), new Vector3(0.11f, 0.035f, 0.07f), Quaternion.Euler(0f, 12f, 0f), yellowSpongeMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "SpongeGreenScour", stationRoot.transform,
                new Vector3(0.02f, 1.275f, 0.24f), new Vector3(0.11f, 0.012f, 0.07f), Quaternion.Euler(0f, 12f, 0f), greenScourMat);

            // 2. ขวดน้ำยาล้างจานสีเขียวมรกตใส พร้อมหัวปั๊มสีขาว (Emerald Dish Soap Pump Bottle) วางชิดแผงหลัง
            CreateVisualPrimitive(PrimitiveType.Cylinder, "SoapBottleBody", stationRoot.transform,
                new Vector3(-0.06f, 1.29f, -0.38f), new Vector3(0.075f, 0.075f, 0.075f), Quaternion.identity, soapBottleMat);
            CreateVisualPrimitive(PrimitiveType.Cylinder, "SoapPumpCollar", stationRoot.transform,
                new Vector3(-0.06f, 1.38f, -0.38f), new Vector3(0.035f, 0.02f, 0.035f), Quaternion.identity, whitePlasticMat);
            CreateVisualPrimitive(PrimitiveType.Cube, "SoapPumpNozzle", stationRoot.transform,
                new Vector3(-0.06f, 1.41f, -0.36f), new Vector3(0.025f, 0.02f, 0.06f), Quaternion.identity, whitePlasticMat);

            // ==========================================
            // [G] ป้ายชื่อสเตชั่น World Space Signboard บน Backsplash (หันหน้าเข้าหาตัวเชฟ +Z)
            // ==========================================
            GameObject signPlate = CreateVisualPrimitive(PrimitiveType.Cube, "StationSignboard", stationRoot.transform,
                new Vector3(0f, 1.82f, -0.64f), new Vector3(0.85f, 0.18f, 0.03f), Quaternion.identity,
                FireExtinguisher.GetSafeMaterial(new Color(0.08f, 0.42f, 0.58f), 0.4f, 0.8f));

            GameObject textObj = new GameObject("StationSignText");
            textObj.transform.SetParent(signPlate.transform, false);
            textObj.transform.localPosition = new Vector3(0f, 0f, 0.55f);
            textObj.transform.localRotation = Quaternion.identity;

            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.text = "[ SINK STATION ]";
            tmp.fontSize = 2.4f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
        }

        // 3. ติดตั้งละอองน้ำหยดเบาๆ ตลอดเวลาจากหัวฉีดก๊อกน้ำ (Continuous Gentle Water Drip)
        if (transform.Find("SinkDripParticles") == null)
        {
            GameObject dripObj = new GameObject("SinkDripParticles");
            dripObj.transform.SetParent(transform, false);
            dripObj.transform.localPosition = new Vector3(-0.30f, 1.54f, -0.10f);

            ParticleSystem dripPs = dripObj.AddComponent<ParticleSystem>();
            var main = dripPs.main;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = 0.50f;
            main.startSpeed = 0.6f;
            main.startSize = 0.035f;
            main.startColor = new Color(0.75f, 0.94f, 1.0f, 0.70f);
            main.gravityModifier = 1.0f;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = dripPs.emission;
            emission.rateOverTime = 3f;

            var shape = dripPs.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 4f;
            shape.radius = 0.015f;

            ParticleSystemRenderer psRenderer = dripObj.GetComponent<ParticleSystemRenderer>();
            psRenderer.material = FireExtinguisher.GetSafeMaterial(new Color(0.8f, 0.95f, 1.0f, 0.8f), 0.1f, 0.95f);
        }

        // 4. ติดตั้งระบบอนุภาคฟองสบู่ตอนขัดล้าง (Wash Foam Particles)
        if (bubblesParticleSystem == null)
        {
            GameObject psObj = new GameObject("WashFoamParticles");
            psObj.transform.SetParent(transform, false);
            psObj.transform.localPosition = new Vector3(-0.30f, 1.26f, 0f);

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

        // 5. AudioSource จำลองเสียงน้ำ
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
        bgImage.color = UITheme.ColorDarkBackground;

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
        progressFillImage.color = UITheme.ColorPrimaryCyan; // ฟ้าครามสดใส

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
