using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ระบบสุ่มเกิดเหตุการณ์ไฟไหม้ตามเคาน์เตอร์ต่างๆ ในห้องครัว (Random Counter Fire Outbreak)
/// สุ่มจุดไฟไหม้ตามเคาน์เตอร์ต่างๆ (เตา, เคาน์เตอร์เตรียม, เขียง) ทุกๆ 18-28 วินาที
/// </summary>
public class RandomFireManager : MonoBehaviour
{
    public static RandomFireManager Instance { get; private set; }

    [Header("Random Fire Interval Settings")]
    [SerializeField] private float minInterval = 12f;
    [SerializeField] private float maxInterval = 20f;

    private float nextFireTimer = 5.0f; // เกิดไฟไหม้ครั้งแรกหลังเริ่มเล่น 5 วินาที

    private void Awake()
    {
        Instance = this;
        nextFireTimer = 5.0f;
    }

    private void Update()
    {
        if (KitchenGameManager.Instance == null || !KitchenGameManager.Instance.IsGamePlaying()) return;

        nextFireTimer -= Time.deltaTime;
        if (nextFireTimer <= 0f)
        {
            TriggerRandomCounterFire();
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        nextFireTimer = UnityEngine.Random.Range(minInterval, maxInterval);
    }

    /// <summary>
    /// สุ่มเลือกเคาน์เตอร์ที่ยังไม่ติดไฟและไม่ถูกล็อค เพื่อจุดไฟไหม้
    /// </summary>
    public void TriggerRandomCounterFire()
    {
        FireHazard[] allHazards = FindObjectsByType<FireHazard>(FindObjectsSortMode.None);
        List<FireHazard> availableHazards = new List<FireHazard>();

        foreach (FireHazard hazard in allHazards)
        {
            if (!hazard.IsBurning() && !hazard.IsLockedOut())
            {
                availableHazards.Add(hazard);
            }
        }

        if (availableHazards.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableHazards.Count);
            FireHazard selectedHazard = availableHazards[randomIndex];
            selectedHazard.Ignite();

            Debug.Log($"🚨 RandomFireManager: SPONTANEOUS FIRE OUTBREAK on [{selectedHazard.gameObject.name}]!");
        }
    }
}
