using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// เคาน์เตอร์ส่งอาหาร (Delivery Counter)
/// 1. รับอาหารจากผู้เล่นเพื่อตรวจสอบและส่งให้ลูกค้า
/// 2. ตาม Q4 ตัวเลือก B: เมื่อส่งอาหารสำเร็จ จานเปื้อนจะสะสมเป็นกอง (Stack สูงสุด 4 ใบ)
/// 3. ผู้เล่นสามารถกด [E] เมื่อมือเปล่าเพื่อยกกองจานเปื้อนทั้งหมดไปล้างที่อ่างล้างจาน
/// </summary>
public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance { get; private set; }

    private const int MAX_DIRTY_PLATES = 4;
    private const float PLATE_OFFSET_Y = 0.075f;

    [Header("Dirty Dishes Stacking")]
    [SerializeField] private int dirtyPlatesAmount = 0;

    private readonly List<GameObject> visualDirtyPlatesList = new List<GameObject>();
    private Transform dirtyPlatesContainer;
    private GameObject badgeRoot;
    private TextMeshPro badgeText;
    private Camera targetCamera;

    private void Awake()
    {
        Instance = this;
        EnsureDirtyVisualContainer();
    }

    private void Start()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }

        UpdateDirtyPlatesVisual();
    }

    private void LateUpdate()
    {
        // ป้ายบอกจำนวนจานเปื้อนหันเข้าหากล้องเสมอ
        if (badgeRoot != null && targetCamera != null && badgeRoot.activeSelf)
        {
            badgeRoot.transform.rotation = targetCamera.transform.rotation;
        }
    }

    /// <summary>
    /// เพิ่มจานเปื้อนสะสม (สูงสุด 4 ใบ)
    /// </summary>
    public void AddDirtyPlate()
    {
        if (dirtyPlatesAmount < MAX_DIRTY_PLATES)
        {
            dirtyPlatesAmount++;
            UpdateDirtyPlatesVisual();
            Debug.Log($"🍽️ [DeliveryCounter] Dirty plate added! Total dirty plates: {dirtyPlatesAmount}/{MAX_DIRTY_PLATES}");
        }
    }

    public override void Interact(Player player)
    {
        // กรณีที่ 1: ผู้เล่นถือสิ่งของมาส่ง
        if (player.HasKitchenObject())
        {
            KitchenObject heldObject = player.GetKitchenObject();

            // ตรวจสอบกรณีถือถังดับเพลิงมาส่ง -> แจ้งเตือนส่งผิดสูตร และสั่งให้ถังดับเพลิง Respawn กลับไปจุดเริ่มต้น
            if (heldObject is FireExtinguisher fireExtinguisher)
            {
                DeliveryManager.Instance.DeliverIncorrectRecipe();
                fireExtinguisher.ScheduleRespawn(1.0f);
                Debug.Log("🧯 [DeliveryCounter] FireExtinguisher submitted! Incorrect recipe, respawning at initial position in 1.0s...");
                return;
            }

            if (heldObject.TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                DeliveryManager.Instance.DeliveryRecipe(plateKitchenObject);
                heldObject.DestroySelf();

                // ตาม Q2 ตัวเลือก A: จานอาหารที่ส่ง (ทั้งสูตรถูกและผิด) จะกลายเป็นจานเปื้อนสะสมเสมอ เพื่อคงจำนวนจานครบ 4 ใบในครัว 100% ไร้ความเสี่ยง Softlock
                AddDirtyPlate();
            }
            else
            {
                // สิ่งของไม่ใช่จานอาหาร (เช่น วัตถุดิบเดี่ยวๆ)
                DeliveryManager.Instance.DeliverIncorrectRecipe();
                heldObject.DestroySelf();
            }
            return;
        }

        // กรณีที่ 2: ผู้เล่นมือเปล่า และมีจานเปื้อนสะสมอยู่บนเคาน์เตอร์ส่งอาหาร
        if (dirtyPlatesAmount > 0)
        {
            // ยกกองจานเปื้อนทั้งหมดไปล้าง (Q4 ตัวเลือก B)
            DirtyPlateKitchenObject.SpawnDirtyPlates(dirtyPlatesAmount, player);
            Debug.Log($"🧽 [DeliveryCounter] Player picked up {dirtyPlatesAmount} dirty plates to wash!");

            dirtyPlatesAmount = 0;
            UpdateDirtyPlatesVisual();
        }
    }

    /// <summary>
    /// เตรียม Container สำหรับวางโมเดล 3D จานเปื้อนบนเคาน์เตอร์
    /// </summary>
    private void EnsureDirtyVisualContainer()
    {
        if (dirtyPlatesContainer != null) return;

        Transform existing = transform.Find("DirtyPlatesContainer");
        if (existing != null)
        {
            dirtyPlatesContainer = existing;
        }
        else
        {
            GameObject cont = new GameObject("DirtyPlatesContainer");
            cont.transform.SetParent(transform, false);

            // วางกองจานเปื้อนไว้บริเวณมุมด้านข้างของเคาน์เตอร์ส่งอาหาร เพื่อไม่ให้เกะกะจานส่งปกติ
            cont.transform.localPosition = new Vector3(-0.35f, 1.25f, 0.15f);
            dirtyPlatesContainer = cont.transform;
        }

        // ป้ายข้อความเตือนให้ผู้เล่นมากด [E] หยิบไปล้าง
        Transform existingBadge = transform.Find("DirtyPlatesBadge");
        if (existingBadge != null)
        {
            badgeRoot = existingBadge.gameObject;
            badgeText = existingBadge.GetComponent<TextMeshPro>();
        }
        else
        {
            GameObject badgeObj = new GameObject("DirtyPlatesBadge");
            badgeObj.transform.SetParent(transform, false);
            badgeObj.transform.localPosition = new Vector3(-0.35f, 1.85f, 0.15f);

            badgeText = badgeObj.AddComponent<TextMeshPro>();
            badgeText.fontSize = 2.8f;
            badgeText.alignment = TextAlignmentOptions.Center;
            badgeText.color = UITheme.ColorWarningOrange;
            badgeText.fontStyle = FontStyles.Bold;
            badgeText.text = "";

            badgeRoot = badgeObj;
        }
    }

    /// <summary>
    /// อัปเดตโมเดล 3D จานเปื้อนที่ซ้อนกันบนเคาน์เตอร์ส่งอาหาร
    /// </summary>
    private void UpdateDirtyPlatesVisual()
    {
        EnsureDirtyVisualContainer();

        // ล้างโมเดลเดิม
        for (int i = visualDirtyPlatesList.Count - 1; i >= 0; i--)
        {
            if (visualDirtyPlatesList[i] != null)
            {
                Destroy(visualDirtyPlatesList[i]);
            }
        }
        visualDirtyPlatesList.Clear();

        if (dirtyPlatesAmount <= 0)
        {
            if (badgeRoot != null) badgeRoot.SetActive(false);
            return;
        }

        // สร้างโมเดลจานเปื้อนซ้อนกัน
        for (int i = 0; i < dirtyPlatesAmount; i++)
        {
            GameObject plateObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plateObj.name = $"VisualDirtyPlate_{i + 1}";
            plateObj.transform.SetParent(dirtyPlatesContainer, false);
            plateObj.transform.localPosition = new Vector3(0f, i * PLATE_OFFSET_Y, 0f);
            plateObj.transform.localScale = new Vector3(0.62f, 0.035f, 0.62f);

            if (plateObj.TryGetComponent(out Collider col)) Destroy(col);

            Material plateMat = FireExtinguisher.GetSafeMaterial(new Color(0.85f, 0.82f, 0.75f), 0.1f, 0.6f);
            plateObj.GetComponent<MeshRenderer>().material = plateMat;

            // คราบอาหารบนจานเปื้อน
            GameObject stain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stain.name = "Stain";
            stain.transform.SetParent(plateObj.transform, false);
            stain.transform.localPosition = new Vector3(0.08f, 0.55f, 0.08f);
            stain.transform.localScale = new Vector3(0.26f, 0.14f, 0.24f);
            if (stain.TryGetComponent(out Collider sc)) Destroy(sc);
            stain.GetComponent<MeshRenderer>().material = FireExtinguisher.GetSafeMaterial(new Color(0.6f, 0.2f, 0.08f), 0f, 0.5f);

            visualDirtyPlatesList.Add(plateObj);
        }

        // แสดงป้ายบอกสถานะ
        if (badgeRoot != null && badgeText != null)
        {
            badgeRoot.transform.localPosition = new Vector3(-0.35f, 1.4f + (dirtyPlatesAmount * PLATE_OFFSET_Y), 0.15f);
            badgeText.text = UITheme.FormatDirtyPlateBadge(dirtyPlatesAmount);
            badgeRoot.SetActive(true);
        }
    }
}
