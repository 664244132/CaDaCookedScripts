using System;
using UnityEngine;

/// <summary>
/// ตัวจัดการระบบชั่วโมงเร่งด่วน (Rush Hour Event)
/// ปรับให้เกิดบ่อยขึ้นเป็นระลอก (ทุกๆ 18 วินาที นาน 10 วินาที) สำหรับรอบเกมที่สั้นและเข้มข้น (60 วินาที)
/// </summary>
public class RushHourManager : MonoBehaviour
{
    public static RushHourManager Instance { get; private set; }

    public event EventHandler OnRushHourStarted;
    public event EventHandler OnRushHourEnded;

    [Header("Rush Hour High-Frequency Settings")]
    [SerializeField] private float rushHourDuration = 15f;       // ระยะเวลาในแต่ละรอบ (15 วินาที)
    [SerializeField] private float rushHourInterval = 25f;       // เกิดซ้ำทุกๆ 25 วินาที
    [SerializeField] private int rushHourScoreMultiplier = 2;   // ตัวคูณคะแนน (2x)
    [SerializeField] private float fastSpawnTimerMax = 1.5f;     // ออเดอร์เข้าถี่มาก (1.5 วินาที)

    private bool isRushHourActive;
    private float nextRushHourTimer;
    private float rushHourRemainingTimer;
    private float defaultSpawnTimerMax = 4f;

    private void Awake()
    {
        Instance = this;
        nextRushHourTimer = 15f; // เกิดระลอกแรกหลังเริ่มเกม 15 วินาที
    }

    private void Start()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;
        }

        if (DeliveryManager.Instance != null)
        {
            defaultSpawnTimerMax = DeliveryManager.Instance.GetSpawnRecipeTimerMax();
        }
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged -= KitchenGameManager_OnStateChanged;
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsCountdownToStartActive())
        {
            // รีเซ็ตสถานะเมื่อเริ่มเกมใหม่
            nextRushHourTimer = 10f;
            isRushHourActive = false;
        }
    }

    private void Update()
    {
        if (KitchenGameManager.Instance == null || !KitchenGameManager.Instance.IsGamePlaying()) return;

        if (!isRushHourActive)
        {
            nextRushHourTimer -= Time.deltaTime;
            if (nextRushHourTimer <= 0f)
            {
                StartRushHour();
            }
        }
        else
        {
            rushHourRemainingTimer -= Time.deltaTime;
            if (rushHourRemainingTimer <= 0f)
            {
                EndRushHour();
            }
        }
    }

    private void StartRushHour()
    {
        isRushHourActive = true;
        rushHourRemainingTimer = rushHourDuration;

        Debug.Log("⚡ RushHourManager: RUSH HOUR STARTED! (2X POINTS & FAST ORDERS)");

        if (DeliveryManager.Instance != null)
        {
            defaultSpawnTimerMax = DeliveryManager.Instance.GetSpawnRecipeTimerMax();
            DeliveryManager.Instance.SetScoreMultiplier(rushHourScoreMultiplier);
            DeliveryManager.Instance.SetSpawnRecipeTimerMax(fastSpawnTimerMax);
        }

        OnRushHourStarted?.Invoke(this, EventArgs.Empty);
    }

    private void EndRushHour()
    {
        isRushHourActive = false;
        nextRushHourTimer = rushHourInterval; // ตั้งเวลารอรอบถัดไป

        Debug.Log("🏁 RushHourManager: Rush Hour Ended.");

        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.SetScoreMultiplier(1);
            DeliveryManager.Instance.SetSpawnRecipeTimerMax(defaultSpawnTimerMax);
        }

        OnRushHourEnded?.Invoke(this, EventArgs.Empty);
    }

    public bool IsRushHourActive()
    {
        return isRushHourActive;
    }

    public float GetRushHourRemainingTimer()
    {
        return Mathf.Max(0f, rushHourRemainingTimer);
    }
}
