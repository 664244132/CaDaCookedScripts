using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ควบคุมระบบไฟไหม้เคาน์เตอร์ (Fire Hazard System)
/// ใช้โมเดลเอฟเฟกต์ไฟจริงจาก Assets/VFXPACK_FIRE_WALLCOEUR (VFX_Fire และ VFX_BlackSmoke)
/// หากผู้เล่นดับไฟไม่ทันภายใน 7 วินาที เคาน์เตอร์จะถูกล็อคไม่ให้ใช้งานเป็นเวลา 5 วินาที
/// พร้อมเปลี่ยนสีตัวเคาน์เตอร์ทั้งหมดให้กลายเป็นสีดำไหม้เกรียม (Charred Black Material)
/// </summary>
public class FireHazard : MonoBehaviour
{
    public static event EventHandler OnAnyFireStarted;
    public static event EventHandler OnAnyFireExtinguished;
    public static event EventHandler OnAnyCounterLocked;
    public static event EventHandler OnAnyCounterUnlocked;

    public event EventHandler OnFireStarted;
    public event EventHandler OnFireExtinguished;
    public event EventHandler OnCounterLocked;
    public event EventHandler OnCounterUnlocked;
    public event EventHandler<OnFireHealthChangedEventArgs> OnFireHealthChanged;

    public class OnFireHealthChangedEventArgs : EventArgs
    {
        public float healthNormalized;
    }

    [Header("Fire & Lockout Settings")]
    [SerializeField] private float maxFireHealth = 100f;
    [SerializeField] private float maxFireBurnoutTime = 7.0f; // เวลาดับไฟ (7 วินาที)
    [SerializeField] private float lockoutDuration = 5.0f;    // โดนล็อค 5 วินาทีถ้าดับไม่ทัน

    [Header("Visual References")]
    [SerializeField] private GameObject fireVisualGameObject;
    [SerializeField] private GameObject lockedVisualGameObject;

    private float currentFireHealth;
    private float fireBurnoutTimer;
    private float lockoutTimer;
    private bool isBurning;
    private bool isLockedOut;

    // ระบบเปลี่ยนสีเคาน์เตอร์ไหม้เกรียมเป็นสีดำ
    private Dictionary<MeshRenderer, Material[]> originalMaterials = new Dictionary<MeshRenderer, Material[]>();
    private Material burntMaterial;

    private void Awake()
    {
        currentFireHealth = maxFireHealth;
        InitializeBurntMaterial();
        CacheOriginalMaterials();
        InitializeVisuals();
    }

    private void InitializeBurntMaterial()
    {
        burntMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        burntMaterial.color = new Color(0.1f, 0.1f, 0.1f, 1f); // สีดำไหม้เกรียมเข้ม
        if (burntMaterial.HasProperty("_Smoothness")) burntMaterial.SetFloat("_Smoothness", 0.05f); // พื้นผิวด้าน
    }

    private void CacheOriginalMaterials()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer r in renderers)
        {
            if (r.transform.name.Contains("Visual") || r.transform.name.Contains("LockIcon")) continue;
            if (!originalMaterials.ContainsKey(r))
            {
                originalMaterials[r] = r.sharedMaterials;
            }
        }
    }

    private void InitializeVisuals()
    {
        if (fireVisualGameObject == null)
        {
            Transform foundVisual = transform.Find("FireVisual");
            if (foundVisual != null) fireVisualGameObject = foundVisual.gameObject;
        }
        if (fireVisualGameObject != null) fireVisualGameObject.SetActive(false);

        if (lockedVisualGameObject == null)
        {
            Transform foundLocked = transform.Find("LockedVisual");
            if (foundLocked != null) lockedVisualGameObject = foundLocked.gameObject;
        }
        if (lockedVisualGameObject != null) lockedVisualGameObject.SetActive(false);
    }

    private void Update()
    {
        // 1. นับเวลาหากกำลังติดไฟ (ถ้าดับไม่ทันจะเข้าสู่สถานะ Lockout)
        if (isBurning)
        {
            fireBurnoutTimer -= Time.deltaTime;
            if (fireBurnoutTimer <= 0f)
            {
                TriggerBurnoutLockout();
            }
        }
        // 2. นับเวลาปลดล็อคเคาน์เตอร์ (5 วินาที)
        else if (isLockedOut)
        {
            lockoutTimer -= Time.deltaTime;
            if (lockoutTimer <= 0f)
            {
                UnlockCounter();
            }
        }
    }

    /// <summary>
    /// สั่งให้เกิดไฟไหม้บนเคาน์เตอร์นี้
    /// </summary>
    public void Ignite()
    {
        if (isBurning || isLockedOut) return;

        isBurning = true;
        currentFireHealth = maxFireHealth;
        fireBurnoutTimer = maxFireBurnoutTime;

        if (fireVisualGameObject == null)
        {
            InitializeVisuals();
        }

        if (fireVisualGameObject != null)
        {
            fireVisualGameObject.SetActive(true);
            ParticleSystem[] pss = fireVisualGameObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss)
            {
                ps.Clear();
                ps.Play();
            }
        }

        Debug.Log($"🔥 FireHazard: Counter [{gameObject.name}] CAUGHT FIRE! Extinguish within {maxFireBurnoutTime}s!");

        OnFireStarted?.Invoke(this, EventArgs.Empty);
        OnAnyFireStarted?.Invoke(this, EventArgs.Empty);
        
        OnFireHealthChanged?.Invoke(this, new OnFireHealthChangedEventArgs
        {
            healthNormalized = 1f
        });
    }

    /// <summary>
    /// รับการฉีดสารดับเพลิงเพื่อลดระดับความรุนแรงของไฟ
    /// </summary>
    public void Extinguish(float extinguishAmount)
    {
        if (!isBurning || isLockedOut) return;

        // ขณะที่ผู้เล่นกำลังฉีดดับไฟ ให้ชะลอการหมดเวลาของเคาน์เตอร์
        fireBurnoutTimer = Mathf.Min(maxFireBurnoutTime, fireBurnoutTimer + Time.deltaTime * 2.0f);

        currentFireHealth -= extinguishAmount;
        currentFireHealth = Mathf.Clamp(currentFireHealth, 0f, maxFireHealth);

        OnFireHealthChanged?.Invoke(this, new OnFireHealthChangedEventArgs
        {
            healthNormalized = currentFireHealth / maxFireHealth
        });

        if (currentFireHealth <= 0f)
        {
            ExtinguishComplete();
        }
    }

    /// <summary>
    /// ดับไฟสำเร็จทันเวลา
    /// </summary>
    private void ExtinguishComplete()
    {
        isBurning = false;

        if (fireVisualGameObject != null)
        {
            ParticleSystem[] pss = fireVisualGameObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss) ps.Stop();
            fireVisualGameObject.SetActive(false);
        }

        Debug.Log($"🧯 FireHazard: Counter [{gameObject.name}] fire extinguished safely!");

        OnFireExtinguished?.Invoke(this, EventArgs.Empty);
        OnAnyFireExtinguished?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ดับไฟไม่ทัน! เคาน์เตอร์ไหม้เกรียม เปลี่ยนสีเป็นสีดำ และถูกล็อค 5 วินาที
    /// </summary>
    private void TriggerBurnoutLockout()
    {
        isBurning = false;
        isLockedOut = true;
        lockoutTimer = lockoutDuration;

        // ปิดไฟ
        if (fireVisualGameObject != null)
        {
            ParticleSystem[] pss = fireVisualGameObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss) ps.Stop();
            fireVisualGameObject.SetActive(false);
        }

        // ทำลายวัตถุดิบบนเคาน์เตอร์
        if (TryGetComponent(out IKitchenObjectParent counterParent) && counterParent.HasKitchenObject())
        {
            counterParent.GetKitchenObject().DestroySelf();
        }

        // 1. เปลี่ยนสีเคาน์เตอร์ทั้งหมดให้เป็นสีดำไหม้เกรียม (Charred Burnt Black)
        ApplyBurntColorToCounter();

        // 2. แสดงควันดำไหม้เกรียม (VFX_BlackSmoke)
        if (lockedVisualGameObject != null)
        {
            lockedVisualGameObject.SetActive(true);
            ParticleSystem[] smokePss = lockedVisualGameObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in smokePss)
            {
                ps.Clear();
                ps.Play();
            }
        }

        Debug.Log($"🔒 FireHazard: Counter [{gameObject.name}] LOCKED for {lockoutDuration}s! (Turned Burnt Black)");

        OnCounterLocked?.Invoke(this, EventArgs.Empty);
        OnAnyCounterLocked?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// ปลดล็อคเคาน์เตอร์หลังครบ 5 วินาที คืนสีเดิม
    /// </summary>
    private void UnlockCounter()
    {
        isLockedOut = false;

        // 1. คืนสีเดิมของเคาน์เตอร์
        RestoreOriginalCounterColor();

        // 2. ปิดควันดำ
        if (lockedVisualGameObject != null)
        {
            ParticleSystem[] smokePss = lockedVisualGameObject.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in smokePss) ps.Stop();
            lockedVisualGameObject.SetActive(false);
        }

        Debug.Log($"🔓 FireHazard: Counter [{gameObject.name}] recovered! Restored original color and unlocked.");

        OnCounterUnlocked?.Invoke(this, EventArgs.Empty);
        OnAnyCounterUnlocked?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyBurntColorToCounter()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer r in renderers)
        {
            if (r.transform.name.Contains("Visual") || r.transform.name.Contains("LockIcon")) continue;

            Material[] burntArray = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < burntArray.Length; i++)
            {
                burntArray[i] = burntMaterial;
            }
            r.materials = burntArray;
        }
    }

    private void RestoreOriginalCounterColor()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null && kvp.Value != null)
            {
                kvp.Key.materials = kvp.Value;
            }
        }
    }

    public bool IsBurning() => isBurning;
    public bool IsLockedOut() => isLockedOut;
    public float GetLockoutRemainingTimer() => Mathf.Max(0f, lockoutTimer);
    public float GetFireRemainingTimer() => Mathf.Max(0f, fireBurnoutTimer);
    public float GetFireHealthNormalized() => currentFireHealth / maxFireHealth;
}
