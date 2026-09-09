using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DeliveryManagerUI: แสดงผลรายการออเดอร์ทั้งหมดบน HUD พร้อมอัปเดตเวลานับถอยหลังของออเดอร์ VIP แบบเรียลไทม์
/// </summary>
public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private Transform recipeTemplate;

    private List<DeliveryManagerSingleUI> activeCardsList = new List<DeliveryManagerSingleUI>();

    private void Awake()
    {
        recipeTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSpawned += DeliveryManager_OnRecipeSpawned;
            DeliveryManager.Instance.OnRecipeCompleted += DeliveryManager_OnRecipeCompleted;
        }
        UpdateVisual();
    }

    private void OnDestroy()
    {
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSpawned -= DeliveryManager_OnRecipeSpawned;
            DeliveryManager.Instance.OnRecipeCompleted -= DeliveryManager_OnRecipeCompleted;
        }
    }

    private void Update()
    {
        // อัปเดตแถบเวลาของการ์ดออเดอร์ VIP ทุกใบ
        for (int i = 0; i < activeCardsList.Count; i++)
        {
            if (activeCardsList[i] != null)
            {
                activeCardsList[i].UpdateTimerVisual();
            }
        }
    }

    private void DeliveryManager_OnRecipeCompleted(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void DeliveryManager_OnRecipeSpawned(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        activeCardsList.Clear();

        foreach (Transform child in container)
        {
            if (child == recipeTemplate) continue;
            Destroy(child.gameObject);
        }

        if (DeliveryManager.Instance == null) return;

        List<DeliveryManager.OrderData> ordersList = DeliveryManager.Instance.GetWaitingOrdersList();
        if (ordersList != null && ordersList.Count > 0)
        {
            foreach (DeliveryManager.OrderData orderData in ordersList)
            {
                Transform recipeTransform = Instantiate(recipeTemplate, container);
                recipeTransform.gameObject.SetActive(true);
                if (recipeTransform.TryGetComponent(out DeliveryManagerSingleUI singleUI))
                {
                    singleUI.SetOrderData(orderData);
                    activeCardsList.Add(singleUI);
                }
            }
        }
        else
        {
            // Fallback กรณีใช้ waitingRecipeSOList เดิม พร้อม Null Guard เพื่อความปลอดภัย 100%
            List<RecipeSO> fallbackList = DeliveryManager.Instance.GetWaitingRecipeSOList();
            if (fallbackList != null && fallbackList.Count > 0)
            {
                foreach (RecipeSO recipeSO in fallbackList)
                {
                    Transform recipeTransform = Instantiate(recipeTemplate, container);
                    recipeTransform.gameObject.SetActive(true);
                    if (recipeTransform.TryGetComponent(out DeliveryManagerSingleUI singleUI))
                    {
                        singleUI.SetRecipeSO(recipeSO);
                        activeCardsList.Add(singleUI);
                    }
                }
            }
        }
    }
}
