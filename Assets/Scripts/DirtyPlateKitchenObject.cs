using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// วัตถุกองจานเปื้อน (Dirty Dishes Stack)
/// 1. รองรับการสะสมและยกเป็นกองสูงสุด 4 ใบ ตาม Q4 ตัวเลือก B
/// 2. มีโมเดลจานเปื้อนซ้อนกันตามจำนวนจริง พร้อมรอยเปื้อนเศษอาหาร (Food Stains & Sauce Drops)
/// 3. มีป้ายบอกจำนวนจานเปื้อนลอยอยู่เหนือกอง (Floating Count Badge) มองเห็นชัดเจน
/// 4. ปฏิบัติตามกฎ 15 ข้อใน REFACTORCODE.md: ปราศจาก GC ใน Update, เช็ค Null ปลอดภัย, คืนค่าทรัพยากรครบถ้วน
/// </summary>
public class DirtyPlateKitchenObject : KitchenObject
{
    private const int MAX_PLATES = 4;
    private const float PLATE_OFFSET_Y = 0.075f;

    [Header("Dirty Plate Data")]
    [SerializeField] private int platesCount = 1;

    private readonly List<GameObject> plateVisualsList = new List<GameObject>();
    private Transform visualContainer;
    private GameObject badgeRoot;
    private TextMeshPro badgeText;
    private Camera targetCamera;

    private void Awake()
    {
        EnsureVisualContainer();
        UpdateVisuals();
    }

    private void Start()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }
    }

    private void LateUpdate()
    {
        // หันป้ายบอกจำนวนจานเข้าหากล้องเสมอ (Billboard Effect) โดยไม่สร้าง GC
        if (badgeRoot != null && targetCamera != null && badgeRoot.activeSelf)
        {
            badgeRoot.transform.rotation = targetCamera.transform.rotation;
        }
    }

    /// <summary>
    /// เตรียม Container สำหรับเก็บโมเดล 3D ของจานเปื้อนแต่ละใบ
    /// </summary>
    private void EnsureVisualContainer()
    {
        if (visualContainer != null) return;

        Transform existingContainer = transform.Find("VisualContainer");
        if (existingContainer != null)
        {
            visualContainer = existingContainer;
        }
        else
        {
            GameObject containerObj = new GameObject("VisualContainer");
            containerObj.transform.SetParent(transform, false);
            visualContainer = containerObj.transform;
        }

        // ป้ายข้อความบอกจำนวนจานเปื้อนเหนือกอง (3D World Space Text)
        Transform existingBadge = transform.Find("CountBadge");
        if (existingBadge != null)
        {
            badgeRoot = existingBadge.gameObject;
            badgeText = existingBadge.GetComponent<TextMeshPro>();
        }
        else
        {
            GameObject badgeObj = new GameObject("CountBadge");
            badgeObj.transform.SetParent(transform, false);
            badgeObj.transform.localPosition = new Vector3(0f, 0.45f, 0f);

            badgeText = badgeObj.AddComponent<TextMeshPro>();
            badgeText.fontSize = 3.6f;
            badgeText.alignment = TextAlignmentOptions.Center;
            badgeText.color = new Color(1f, 0.85f, 0.2f); // สีส้มอมเหลือง
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.text = "";

            badgeRoot = badgeObj;
        }
    }

    /// <summary>
    /// กำหนดจำนวนจานเปื้อนในกอง และอัปเดตโมเดล 3D
    /// </summary>
    public void SetPlatesCount(int count)
    {
        platesCount = Mathf.Clamp(count, 1, MAX_PLATES);
        UpdateVisuals();
    }

    /// <summary>
    /// เพิ่มจานเปื้อนลงในกองอีก 1 ใบ (สูงสุด 4 ใบ)
    /// </summary>
    public bool TryAddPlate()
    {
        if (platesCount >= MAX_PLATES) return false;

        platesCount++;
        UpdateVisuals();
        return true;
    }

    /// <summary>
    /// ดึงจำนวนจานเปื้อนปัจจุบัน
    /// </summary>
    public int GetPlatesCount()
    {
        return platesCount;
    }

    /// <summary>
    /// อัปเดตและสร้างโมเดลจานเปื้อนซ้อนกันตามจำนวนจริง
    /// </summary>
    private void UpdateVisuals()
    {
        EnsureVisualContainer();

        // ล้างโมเดลจานเดิม
        for (int i = plateVisualsList.Count - 1; i >= 0; i--)
        {
            if (plateVisualsList[i] != null)
            {
                Destroy(plateVisualsList[i]);
            }
        }
        plateVisualsList.Clear();

        // สร้างโมเดลจานแต่ละใบซ้อนขึ้นไปตามแนวแกน Y
        for (int i = 0; i < platesCount; i++)
        {
            GameObject plateObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plateObj.name = $"DirtyPlate_{i + 1}";
            plateObj.transform.SetParent(visualContainer, false);
            plateObj.transform.localPosition = new Vector3(0f, i * PLATE_OFFSET_Y, 0f);
            plateObj.transform.localScale = new Vector3(0.68f, 0.035f, 0.68f);

            // ปิด Collider ของชิ้นส่วนย่อยเพื่อป้องกันการติดขัดของการชน
            if (plateObj.TryGetComponent(out Collider col))
            {
                Destroy(col);
            }

            // วัสดุเซรามิกจานสีขาวนวลอมเทาเปื้อน
            Material plateMat = FireExtinguisher.GetSafeMaterial(new Color(0.86f, 0.84f, 0.78f), 0.1f, 0.6f);
            plateObj.GetComponent<MeshRenderer>().material = plateMat;

            // เพิ่มจุดคราบซอส / เศษอาหารเปื้อนบนจาน
            CreateFoodStains(plateObj.transform, i);

            plateVisualsList.Add(plateObj);
        }

        // อัปเดตตำแหน่งและข้อความของป้ายจำนวน
        if (badgeRoot != null && badgeText != null)
        {
            badgeRoot.transform.localPosition = new Vector3(0f, (platesCount * PLATE_OFFSET_Y) + 0.35f, 0f);
            badgeText.text = platesCount > 1 ? $"DIRTY x{platesCount}" : "DIRTY";
            badgeRoot.SetActive(true);
        }
    }

    /// <summary>
    /// สร้างจุดคราบอาหาร / ซอสเปื้อนบนจาน
    /// </summary>
    private void CreateFoodStains(Transform plateParent, int index)
    {
        // คราบซอสมะเขือเทศสีแดงเข้ม
        GameObject stainRed = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stainRed.name = "SauceStain_Red";
        stainRed.transform.SetParent(plateParent, false);
        stainRed.transform.localPosition = new Vector3(0.12f + (index * 0.03f), 0.55f, 0.08f);
        stainRed.transform.localScale = new Vector3(0.24f, 0.15f, 0.22f);
        if (stainRed.TryGetComponent(out Collider c1)) Destroy(c1);
        stainRed.GetComponent<MeshRenderer>().material = FireExtinguisher.GetSafeMaterial(new Color(0.65f, 0.12f, 0.08f), 0.0f, 0.7f);

        // คราบซอสเกรวี่ / มัสตาร์ดสีน้ำตาลอมส้ม
        GameObject stainBrown = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stainBrown.name = "SauceStain_Brown";
        stainBrown.transform.SetParent(plateParent, false);
        stainBrown.transform.localPosition = new Vector3(-0.1f, 0.55f, -0.09f - (index * 0.02f));
        stainBrown.transform.localScale = new Vector3(0.28f, 0.12f, 0.24f);
        if (stainBrown.TryGetComponent(out Collider c2)) Destroy(c2);
        stainBrown.GetComponent<MeshRenderer>().material = FireExtinguisher.GetSafeMaterial(new Color(0.48f, 0.28f, 0.10f), 0.0f, 0.5f);
    }

    /// <summary>
    /// ฟังก์ชัน Factory สำหรับสร้างกองจานเปื้อนและมอบให้ผู้เล่นหรือเคาน์เตอร์ถือไว้ทันที
    /// </summary>
    public static DirtyPlateKitchenObject SpawnDirtyPlates(int count, IKitchenObjectParent parent)
    {
        GameObject dirtyPlateObj = new GameObject("DirtyPlatesStack");
        DirtyPlateKitchenObject dirtyPlateKO = dirtyPlateObj.AddComponent<DirtyPlateKitchenObject>();
        dirtyPlateKO.SetPlatesCount(count);
        dirtyPlateKO.SetKitchenObjectParent(parent);
        return dirtyPlateKO;
    }
}
