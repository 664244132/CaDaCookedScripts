using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDeliveryResultUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failedSprite;
    [SerializeField] private Color successColor = new Color(0, 0.7f, 0, 1f); // เขียว
    [SerializeField] private Color failedColor = new Color(0.8f, 0, 0, 1f);   // แดง
    
    [SerializeField] private float showDuration = 1.5f;
    private float showTimer;

    private void Start()
    {
        // รอรับสัญญาณตอนส่งอาหาร
        DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
        DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (showTimer > 0)
        {
            showTimer -= Time.deltaTime;
            if (showTimer <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
        showTimer = showDuration;
        
        if (iconImage != null && successSprite != null)
        {
            iconImage.sprite = successSprite;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = successColor;
        }
    }

    private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
    {
        gameObject.SetActive(true);
        showTimer = showDuration;

        if (iconImage != null && failedSprite != null)
        {
            iconImage.sprite = failedSprite;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = failedColor;
        }
    }

    private void OnDestroy()
    {
        // ป้องกัน memory leak
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.OnRecipeSuccess -= DeliveryManager_OnRecipeSuccess;
            DeliveryManager.Instance.OnRecipeFailed -= DeliveryManager_OnRecipeFailed;
        }
    }
}
