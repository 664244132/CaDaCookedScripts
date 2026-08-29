using System;
using UnityEngine;

/// <summary>
/// ควบคุมระบบไฟไหม้ (Fire Hazard) บนเคาน์เตอร์ครัว
/// เมื่อเกิดไฟไหม้ ผู้เล่นจะไม่สามารถหยิบจับของบนเคาน์เตอร์ได้จนกว่าจะใช้ถังดับเพลิงฉีดดับไฟ
/// </summary>
public class FireHazard : MonoBehaviour
{
    public static event EventHandler OnAnyFireStarted;
    public static event EventHandler OnAnyFireExtinguished;

    public event EventHandler OnFireStarted;
    public event EventHandler OnFireExtinguished;
    public event EventHandler<OnFireHealthChangedEventArgs> OnFireHealthChanged;

    public class OnFireHealthChangedEventArgs : EventArgs
    {
        public float healthNormalized;
    }

    [Header("Fire Settings")]
    [SerializeField] private float maxFireHealth = 100f;
    [SerializeField] private GameObject fireVisualGameObject;
    [SerializeField] private ParticleSystem fireParticleSystem;

    private float currentFireHealth;
    private bool isBurning;

    private void Awake()
    {
        currentFireHealth = maxFireHealth;
        if (fireVisualGameObject == null)
        {
            Transform foundVisual = transform.Find("FireVisual");
            if (foundVisual != null)
            {
                fireVisualGameObject = foundVisual.gameObject;
            }
        }
        if (fireVisualGameObject != null)
        {
            fireVisualGameObject.SetActive(false);
        }
    }

    /// <summary>
    /// สั่งให้เกิดไฟไหม้
    /// </summary>
    public void Ignite()
    {
        if (isBurning) return;

        isBurning = true;
        currentFireHealth = maxFireHealth;

        if (fireVisualGameObject == null)
        {
            Transform foundVisual = transform.Find("FireVisual");
            if (foundVisual != null)
            {
                fireVisualGameObject = foundVisual.gameObject;
            }
        }

        if (fireVisualGameObject != null)
        {
            fireVisualGameObject.SetActive(true);
        }

        if (fireParticleSystem != null && !fireParticleSystem.isPlaying)
        {
            fireParticleSystem.Play();
        }

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
    /// <param name="extinguishAmount">ปริมาณการดับไฟต่อเฟรม</param>
    public void Extinguish(float extinguishAmount)
    {
        if (!isBurning) return;

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
    /// ดับไฟสำเร็จ
    /// </summary>
    private void ExtinguishComplete()
    {
        isBurning = false;

        if (fireVisualGameObject != null)
        {
            fireVisualGameObject.SetActive(false);
        }

        if (fireParticleSystem != null)
        {
            fireParticleSystem.Stop();
        }

        OnFireExtinguished?.Invoke(this, EventArgs.Empty);
        OnAnyFireExtinguished?.Invoke(this, EventArgs.Empty);
    }

    public bool IsBurning()
    {
        return isBurning;
    }

    public float GetFireHealthNormalized()
    {
        return currentFireHealth / maxFireHealth;
    }
}
