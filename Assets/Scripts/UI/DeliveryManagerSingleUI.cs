using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// DeliveryManagerSingleUI: แสดงการ์ดออเดอร์เดี่ยว พร้อมไฮไลท์สีทองแบบ Pure ASCII และหลอดเวลานับถอยหลังที่สวยงาม คมชัด
/// </summary>
public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipeNameText;
    [SerializeField] private Transform iconContainer;
    [SerializeField] private Transform iconTemplate;

    private Image backgroundImage;
    private Image timerBarImage;
    private GameObject timerBarRoot;
    private DeliveryManager.OrderData currentOrderData;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
        backgroundImage = GetComponent<Image>();
        EnsureTimerBarVisual();
    }

    private void EnsureTimerBarVisual()
    {
        if (timerBarRoot != null) return;

        // ค้นหาหรือสร้างหลอดเวลานับถอยหลังที่ขอบล่างสุดของการ์ด
        Transform existingTimer = transform.Find("VIPTimerBar");
        if (existingTimer != null)
        {
            timerBarRoot = existingTimer.gameObject;
            timerBarImage = existingTimer.Find("BarFill")?.GetComponent<Image>();
        }
        else
        {
            GameObject barObj = new GameObject("VIPTimerBar");
            barObj.transform.SetParent(transform, false);
            RectTransform barRect = barObj.AddComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(1f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.sizeDelta = new Vector2(-6f, 4f);
            barRect.anchoredPosition = new Vector2(0f, 2f);

            Image bgImg = barObj.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            GameObject fillObj = new GameObject("BarFill");
            fillObj.transform.SetParent(barObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;

            timerBarImage = fillObj.AddComponent<Image>();
            timerBarImage.color = new Color(1.0f, 0.75f, 0.0f); // Golden Yellow

            timerBarRoot = barObj;
        }

        timerBarRoot.SetActive(false);
    }

    public void SetOrderData(DeliveryManager.OrderData orderData)
    {
        currentOrderData = orderData;
        SetRecipSO(orderData.recipeSO);

        EnsureTimerBarVisual();

        if (orderData.isVIP)
        {
            // ตกแต่งการ์ด VIP สีเข้มพรีเมียม ขอบทอง
            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.18f, 0.14f, 0.06f, 0.96f); // Dark Gold-Brown
            }

            // บรรทัดเดียว คมชัด 100% ปราศจาก Unicode Emojis เพื่อแก้ปัญหาตัวอักษรกล่องสี่เหลี่ยม
            recipeNameText.text = $"<color=#FFD700><b>[VIP 3X]</b></color> <color=#FFFFFF>{orderData.recipeSO.recipeName}</color>";
            recipeNameText.textWrappingMode = TextWrappingModes.NoWrap;

            if (timerBarRoot != null)
            {
                timerBarRoot.SetActive(true);
            }
        }
        else
        {
            // ตกแต่งการ์ดปกติ
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.white;
            }

            recipeNameText.text = orderData.recipeSO.recipeName;
            recipeNameText.color = new Color(0.12f, 0.12f, 0.15f);
            recipeNameText.textWrappingMode = TextWrappingModes.NoWrap;

            if (timerBarRoot != null)
            {
                timerBarRoot.SetActive(false);
            }
        }
    }

    public void UpdateTimerVisual()
    {
        if (currentOrderData != null && currentOrderData.isVIP && timerBarImage != null)
        {
            float ratio = currentOrderData.GetTimerNormalized();
            timerBarImage.rectTransform.anchorMax = new Vector2(ratio, 1f);

            // เปลี่ยนสีหลอดเวลาเป็นสีแดงกระพริบเมื่อใกล้หมดเวลา (เหลือ < 25%)
            if (ratio < 0.25f)
            {
                timerBarImage.color = Mathf.PingPong(Time.time * 8f, 1f) > 0.5f ? Color.red : new Color(1f, 0.5f, 0f);
            }
            else
            {
                timerBarImage.color = new Color(1.0f, 0.75f, 0.0f);
            }
        }
    }

    public void SetRecipSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;

        foreach (Transform child in iconContainer)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (KitchenObjectSO kitchenObjectSO in recipeSO.kitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplate, iconContainer);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
        }
    }
}
