using System;
using TMPro;
using UnityEngine;

/// <summary>
/// แสดงผลป้ายแจ้งเตือนชั่วโมงเร่งด่วน (Rush Hour UI)
/// แสดงข้อความภาษาไทยและอังกฤษที่อ่านง่าย ชัดเจน ไม่กลับด้าน และไม่มีตัวอักษรสี่เหลี่ยมหลุด
/// </summary>
public class RushHourUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bannerGameObject;
    [SerializeField] private TextMeshProUGUI rushHourText;

    private void Awake()
    {
        if (rushHourText == null)
        {
            rushHourText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void Start()
    {
        if (RushHourManager.Instance != null)
        {
            RushHourManager.Instance.OnRushHourStarted += RushHourManager_OnRushHourStarted;
            RushHourManager.Instance.OnRushHourEnded += RushHourManager_OnRushHourEnded;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (RushHourManager.Instance != null)
        {
            RushHourManager.Instance.OnRushHourStarted -= RushHourManager_OnRushHourStarted;
            RushHourManager.Instance.OnRushHourEnded -= RushHourManager_OnRushHourEnded;
        }
    }

    private void Update()
    {
        if (RushHourManager.Instance != null && RushHourManager.Instance.IsRushHourActive())
        {
            if (rushHourText != null)
            {
                rushHourText.text = $">> RUSH HOUR 2X POINTS! << ({RushHourManager.Instance.GetRushHourRemainingTimer():0}s)";
            }
        }
    }

    private void RushHourManager_OnRushHourStarted(object sender, EventArgs e)
    {
        Show();
    }

    private void RushHourManager_OnRushHourEnded(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        if (bannerGameObject != null)
        {
            bannerGameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        if (bannerGameObject != null)
        {
            bannerGameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
