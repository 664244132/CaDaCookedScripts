using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    public event EventHandler<OnIngredientRemovedEventArgs> OnIngredientRemoved;
    public class OnIngredientRemovedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }

    private const float PLATE_OFFSET_Y = 0.075f;

    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> kitchenObjectSOList;

    [Header("Clean Plate Stacking (Q1 Option A)")]
    [SerializeField] private int stackCount = 1;

    private readonly List<GameObject> stackVisualsList = new List<GameObject>();
    private Transform stackVisualContainer;
    private GameObject stackBadgeRoot;
    private TextMeshPro stackBadgeText;
    private Camera targetCamera;

    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
        EnsureStackContainer();
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
        // หันป้ายบอกจำนวนจานเข้าหากล้องเสมอ (Billboard) โดยไม่สร้างขยะ GC
        if (stackBadgeRoot != null && targetCamera != null && stackBadgeRoot.activeSelf)
        {
            stackBadgeRoot.transform.rotation = targetCamera.transform.rotation;
        }
    }

    public int GetStackCount() => stackCount;

    public void SetStackCount(int count)
    {
        stackCount = Mathf.Clamp(count, 1, 4);
        UpdateStackVisuals();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        // หากถือจานเป็นกองมากกว่า 1 ใบ ไม่อนุญาตให้ใส่อาหาร ต้องเป็นจานเดี่ยวใบเดียวเท่านั้น
        if (stackCount > 1)
        {
            return false;
        }

        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }

        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        else
        {
            kitchenObjectSOList.Add(kitchenObjectSO);

            OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
            {
                kitchenObjectSO = kitchenObjectSO
            });

            return true;
        }
    }

    public event EventHandler OnIngredientsCleared;

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }

    public bool HasIngredients()
    {
        return kitchenObjectSOList != null && kitchenObjectSOList.Count > 0;
    }

    /// <summary>
    /// ดึงวัตถุดิบชั้นบนสุดออกจากจานอาหาร (เช่น เมื่อแมวขโมยวัตถุดิบ)
    /// คืนค่า true หากมีวัตถุดิบและดึงสำเร็จ พร้อมยิง OnIngredientRemoved
    /// </summary>
    public bool TryRemoveTopIngredient(out KitchenObjectSO removedIngredient)
    {
        if (kitchenObjectSOList != null && kitchenObjectSOList.Count > 0)
        {
            int lastIndex = kitchenObjectSOList.Count - 1;
            removedIngredient = kitchenObjectSOList[lastIndex];
            kitchenObjectSOList.RemoveAt(lastIndex);

            OnIngredientRemoved?.Invoke(this, new OnIngredientRemovedEventArgs
            {
                kitchenObjectSO = removedIngredient
            });

            return true;
        }

        removedIngredient = null;
        return false;
    }

    /// <summary>
    /// เทวัตถุดิบและอาหารทั้งหมดออกจากจาน (เช่น เมื่อนำไปเทลงถังขยะ TrashCounter)
    /// </summary>
    public void ClearIngredients()
    {
        kitchenObjectSOList.Clear();
        OnIngredientsCleared?.Invoke(this, EventArgs.Empty);
    }

    private void EnsureStackContainer()
    {
        if (stackVisualContainer != null) return;

        Transform existingContainer = transform.Find("CleanStackContainer");
        if (existingContainer != null)
        {
            stackVisualContainer = existingContainer;
        }
        else
        {
            GameObject cont = new GameObject("CleanStackContainer");
            cont.transform.SetParent(transform, false);
            stackVisualContainer = cont.transform;
        }

        Transform existingBadge = transform.Find("CleanStackBadge");
        if (existingBadge != null)
        {
            stackBadgeRoot = existingBadge.gameObject;
            stackBadgeText = existingBadge.GetComponent<TextMeshPro>();
        }
        else
        {
            GameObject badgeObj = new GameObject("CleanStackBadge");
            badgeObj.transform.SetParent(transform, false);
            badgeObj.transform.localPosition = new Vector3(0f, 0.50f, 0f);

            stackBadgeText = badgeObj.AddComponent<TextMeshPro>();
            stackBadgeText.fontSize = 2.4f;
            stackBadgeText.alignment = TextAlignmentOptions.Center;
            stackBadgeText.color = UITheme.ColorPrimaryCyan;
            stackBadgeText.fontStyle = FontStyles.Bold;
            stackBadgeText.text = "";

            stackBadgeRoot = badgeObj;
            stackBadgeRoot.SetActive(false);
        }
    }

    private void UpdateStackVisuals()
    {
        EnsureStackContainer();

        // ล้างโมเดลจานเสริมเดิม
        for (int i = stackVisualsList.Count - 1; i >= 0; i--)
        {
            if (stackVisualsList[i] != null)
            {
                Destroy(stackVisualsList[i]);
            }
        }
        stackVisualsList.Clear();

        if (stackCount <= 1)
        {
            if (stackBadgeRoot != null) stackBadgeRoot.SetActive(false);
            return;
        }

        // สร้างโมเดลจานสะอาดซ้อนขึ้นไปตามจำนวน (stackCount - 1)
        for (int i = 1; i < stackCount; i++)
        {
            GameObject plateObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plateObj.name = $"VisualCleanPlate_{i + 1}";
            plateObj.transform.SetParent(stackVisualContainer, false);
            plateObj.transform.localPosition = new Vector3(0f, i * PLATE_OFFSET_Y, 0f);
            plateObj.transform.localScale = new Vector3(0.62f, 0.035f, 0.62f);

            if (plateObj.TryGetComponent(out Collider col)) Destroy(col);

            Material cleanMat = FireExtinguisher.GetSafeMaterial(new Color(0.96f, 0.96f, 0.98f), 0.1f, 0.9f);
            plateObj.GetComponent<MeshRenderer>().material = cleanMat;

            stackVisualsList.Add(plateObj);
        }

        // แสดงป้ายบอกจำนวนกองจานสะอาด
        if (stackBadgeRoot != null && stackBadgeText != null)
        {
            stackBadgeRoot.transform.localPosition = new Vector3(0f, 0.40f + (stackCount * PLATE_OFFSET_Y), 0f);
            stackBadgeText.text = UITheme.FormatCleanPlateBadge(stackCount);
            stackBadgeRoot.SetActive(true);
        }
    }
}
