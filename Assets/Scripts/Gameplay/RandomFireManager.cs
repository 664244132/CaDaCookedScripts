using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ระบบสุ่มเกิดเหตุการณ์ไฟไหม้ตามเคาน์เตอร์ต่างๆ ในห้องครัว (Random Counter Fire Outbreak)
/// สุ่มจุดไฟไหม้ตามเคาน์เตอร์ต่างๆ (เตา, เคาน์เตอร์เตรียม, เขียง) พร้อมกัน 2 แห่ง ทุกๆ 12-20 วินาที
/// </summary>
public class RandomFireManager : MonoBehaviour
{
    public static RandomFireManager Instance { get; private set; }

    [Header("Random Fire Interval Settings")]
    [SerializeField] private float minInterval = 12f;
    [SerializeField] private float maxInterval = 20f;

    [Header("Simultaneous Fire Settings")]
    [SerializeField] private int firesPerOutbreak = 2;   // จำนวนเคาน์เตอร์ที่จะเกิดไฟไหม้พร้อมกันใน 1 รอบ (ตาม Requirement 2)
    [SerializeField] private int maxConcurrentFires = 2; // ขีดจำกัดสูงสุดของไฟที่ไหม้พร้อมกันในห้องครัว

    private float nextFireTimer = 5.0f; // เกิดไฟไหม้ครั้งแรกหลังเริ่มเล่น 5 วินาที
    private FireHazard[] cachedHazards;
    private readonly List<FireHazard> reusableAvailableHazards = new List<FireHazard>(32);

    private void Awake()
    {
        Instance = this;
        nextFireTimer = 5.0f;
    }

    private void Start()
    {
        RefreshCachedHazards();
    }

    /// <summary>
    /// ทำการค้นหาและ Cache ออบเจกต์ FireHazard ทั้งหมดในฉาก เพื่อป้องกันการ FindObjectsByType ใน Update/Outbreak (Rule 5 & 6)
    /// </summary>
    public void RefreshCachedHazards()
    {
        cachedHazards = FindObjectsByType<FireHazard>(FindObjectsSortMode.None);
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
    /// สุ่มเลือกเคาน์เตอร์ที่ยังไม่ติดไฟและไม่ถูกล็อค เพื่อจุดไฟไหม้พร้อมกัน 2 แห่ง (Dual Outbreak)
    /// </summary>
    public void TriggerRandomCounterFire()
    {
        if (cachedHazards == null || cachedHazards.Length == 0)
        {
            RefreshCachedHazards();
            if (cachedHazards == null || cachedHazards.Length == 0) return;
        }

        reusableAvailableHazards.Clear();
        int activeBurningCount = 0;

        foreach (FireHazard hazard in cachedHazards)
        {
            if (hazard == null) continue;

            if (hazard.IsBurning())
            {
                activeBurningCount++;
            }
            else if (!hazard.IsLockedOut())
            {
                reusableAvailableHazards.Add(hazard);
            }
        }

        // หากมีไฟกำลังไหม้อยู่แล้วถึงเพดาน maxConcurrentFires หรือไม่มีเคาน์เตอร์ว่าง ไม่ต้องจุดเพิ่ม
        if (activeBurningCount >= maxConcurrentFires || reusableAvailableHazards.Count == 0)
        {
            return;
        }

        // คำนวณจำนวนไฟที่จะจุดเพิ่มในรอบนี้ (ไม่เกิน firesPerOutbreak และไม่เกิน maxConcurrentFires)
        int allowedToIgnite = maxConcurrentFires - activeBurningCount;
        int firesToSpawn = Mathf.Min(firesPerOutbreak, Mathf.Min(allowedToIgnite, reusableAvailableHazards.Count));

        for (int i = 0; i < firesToSpawn; i++)
        {
            // สุ่มเลือกเคาน์เตอร์จาก reusableAvailableHazards แบบไม่ซ้ำกัน
            int randomIndex = UnityEngine.Random.Range(0, reusableAvailableHazards.Count);
            FireHazard selectedHazard = reusableAvailableHazards[randomIndex];
            reusableAvailableHazards.RemoveAt(randomIndex); // ดึงออกจากลิสต์เพื่อไม่ให้สุ่มซ้ำเคาน์เตอร์เดิม

            selectedHazard.Ignite();
            Debug.Log($"🚨 RandomFireManager: DUAL FIRE OUTBREAK [{i + 1}/{firesToSpawn}] on [{selectedHazard.gameObject.name}]!");
        }
    }
}
