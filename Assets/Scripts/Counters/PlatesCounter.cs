using System;
using TMPro;
using UnityEngine;

/// <summary>
/// แท่นจ่ายจานสะอาด (Plates Counter)
/// 1. เริ่มต้นเกมมีจานจำกัด 4 ใบ (platesSpawnedAmountMax = 4)
/// 2. ปิดระบบ Auto-Spawn อัตโนมัติ เพื่อให้ผู้เล่นต้องพึ่งพาการล้างจานจาก SinkCounter
/// 3. รองรับการนำกองจานสะอาด (Clean Plate Stack) จาก SinkCounter มาเติมคืนที่แท่น
/// 4. แสดงป้ายเตือน World Space 3D "⚠️ NO PLATES! WASH AT SINK" เมื่อจานหมด (Q4 ตัวเลือก A)
/// </summary>
public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    private const int MAX_PLATES_AMOUNT = 4; // จำกัดเพดานจานสูงสุด 4 ใบสำหรับวงจร Closed Loop

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    public KitchenObjectSO GetPlateKitchenObjectSO() => plateKitchenObjectSO;

    private int platesSpawnedAmount;
    private int platesSpawnedAmountMax = MAX_PLATES_AMOUNT;

    // World Space 3D Warning Badge
    private GameObject warningBadgeRoot;
    private TextMeshPro warningBadgeText;
    private Camera targetCamera;

    private void Awake()
    {
        EnsureWarningBadge();
    }

    private void Start()
    {
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindFirstObjectByType<Camera>();
        }

        // เริ่มต้นเกมให้มีจานสะอาดพร้อมใช้งาน 4 ใบ
        platesSpawnedAmount = platesSpawnedAmountMax;
        for (int i = 0; i < platesSpawnedAmount; i++)
        {
            OnPlateSpawned?.Invoke(this, EventArgs.Empty);
        }

        UpdateWarningBadge();
    }

    private void LateUpdate()
    {
        // ป้ายเตือนหันหน้าเข้าหากล้องเสมอ (Billboard Effect) โดยไม่สร้าง GC
        if (warningBadgeRoot != null && targetCamera != null && warningBadgeRoot.activeSelf)
        {
            warningBadgeRoot.transform.rotation = targetCamera.transform.rotation;
        }
    }

    public override void Interact(Player player)
    {
        // กรณีที่ 1: ผู้เล่นถือจานสะอาดเปล่า (หรือกองจานสะอาด) นำกลับมาเก็บเข้าแท่นวางจาน
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject heldPlate))
            {
                // ตรวจสอบว่าเป็นจานเปล่าที่ยังไม่ได้ใส่อาหาร และแท่นวางยังไม่เต็ม
                if (heldPlate.GetKitchenObjectSOList().Count == 0 && platesSpawnedAmount < platesSpawnedAmountMax)
                {
                    int incomingCount = heldPlate.GetStackCount();
                    int spaceAvailable = platesSpawnedAmountMax - platesSpawnedAmount;
                    int amountToStore = Mathf.Min(incomingCount, spaceAvailable);

                    if (amountToStore >= incomingCount)
                    {
                        // เก็บจานได้ทั้งกอง
                        platesSpawnedAmount += amountToStore;
                        heldPlate.DestroySelf();
                    }
                    else
                    {
                        // เก็บได้บางส่วน (กองจานลดจำนวนลง)
                        platesSpawnedAmount += amountToStore;
                        heldPlate.SetStackCount(incomingCount - amountToStore);
                    }

                    for (int i = 0; i < amountToStore; i++)
                    {
                        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
                    }

                    UpdateWarningBadge();
                    Debug.Log($"🍽️ [PlatesCounter] Stored {amountToStore} clean plates back. Total: {platesSpawnedAmount}/{platesSpawnedAmountMax}");
                }
            }
            return;
        }

        // กรณีที่ 2: ผู้เล่นมือเปล่า
        if (platesSpawnedAmount > 0)
        {
            // หยิบจานสะอาดไปใช้งานทีละ 1 ใบ
            platesSpawnedAmount--;
            KitchenObject spawnedObj = KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            if (spawnedObj.TryGetPlate(out PlateKitchenObject singlePlate))
            {
                singlePlate.SetStackCount(1);
            }

            OnPlateRemoved?.Invoke(this, EventArgs.Empty);
            UpdateWarningBadge();
        }
        else
        {
            // จานหมดเกลี้ยง! เล่นเสียงเตือน Beep ตาม Q4 ตัวเลือก A
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayWarningSound(transform.position);
            }
            Debug.LogWarning("⚠️ [PlatesCounter] No plates available! Wash dirty plates at the Sink Station!");
        }
    }

    /// <summary>
    /// เตรียม GameObject ป้ายเตือน World Space 3D
    /// </summary>
    private void EnsureWarningBadge()
    {
        if (warningBadgeRoot != null) return;

        Transform existingBadge = transform.Find("NoPlatesWarningBadge");
        if (existingBadge != null)
        {
            warningBadgeRoot = existingBadge.gameObject;
            warningBadgeText = existingBadge.GetComponent<TextMeshPro>();
        }
        else
        {
            GameObject badgeObj = new GameObject("NoPlatesWarningBadge");
            badgeObj.transform.SetParent(transform, false);
            badgeObj.transform.localPosition = new Vector3(0f, 1.85f, 0f);

            warningBadgeText = badgeObj.AddComponent<TextMeshPro>();
            warningBadgeText.fontSize = 2.4f;
            warningBadgeText.alignment = TextAlignmentOptions.Center;
            warningBadgeText.color = new Color(1.0f, 0.45f, 0.0f); // ส้มเตือนสดใส
            warningBadgeText.fontStyle = FontStyles.Bold;
            warningBadgeText.text = "[ ! ] NO PLATES!\nWASH AT SINK";

            warningBadgeRoot = badgeObj;
            warningBadgeRoot.SetActive(false);
        }
    }

    /// <summary>
    /// สลับการแสดงผลป้ายเตือนเมื่อจานหมด
    /// </summary>
    private void UpdateWarningBadge()
    {
        EnsureWarningBadge();

        if (warningBadgeRoot != null)
        {
            bool isEmpty = platesSpawnedAmount <= 0;
            warningBadgeRoot.SetActive(isEmpty);
        }
    }
}
