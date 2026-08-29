using System;
using UnityEngine;

/// <summary>
/// ถังดับเพลิง (Fire Extinguisher)
/// เมื่อผู้เล่นถือและกดปุ่ม InteractAlternate (F) จะทำการฉีดพ่นโฟมดับเพลิงใส่เคาน์เตอร์ที่เกิดไฟไหม้
/// </summary>
public class FireExtinguisher : KitchenObject
{
    [Header("Extinguisher Settings")]
    [SerializeField] private float extinguishRate = 45f; // ปริมาณการดับไฟต่อวินาที
    [SerializeField] private float extinguishRange = 2.5f; // ระยะการฉีด
    [SerializeField] private ParticleSystem sprayParticleSystem;
    [SerializeField] private AudioSource sprayAudioSource;

    private bool isSpraying;

    private void Start()
    {
        if (sprayParticleSystem != null)
        {
            sprayParticleSystem.Stop();
        }
        if (sprayAudioSource != null)
        {
            sprayAudioSource.Stop();
        }
    }

    private void Update()
    {
        // หากวางอยู่บนเคาน์เตอร์หรือไม่ถูกถือ ให้หยุดฉีดทันที
        if (isSpraying && GetKitchenObjectParent() is not Player)
        {
            StopSpraying();
        }
    }

    /// <summary>
    /// สั่งเริ่มฉีดโฟมดับเพลิง
    /// </summary>
    /// <param name="forwardDirection">ทิศทางที่ผู้เล่นกำลังหันหน้า</param>
    public void StartSpraying(Vector3 forwardDirection)
    {
        isSpraying = true;

        if (sprayParticleSystem != null && !sprayParticleSystem.isPlaying)
        {
            sprayParticleSystem.Play();
        }

        if (sprayAudioSource != null && !sprayAudioSource.isPlaying)
        {
            sprayAudioSource.Play();
        }

        Debug.DrawRay(transform.position, forwardDirection * extinguishRange, Color.cyan);

        // ตรวจจับ FireHazard บริเวณด้านหน้าในระยะพ่น
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1.2f, forwardDirection, extinguishRange);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent(out FireHazard fireHazard))
            {
                fireHazard.Extinguish(extinguishRate * Time.deltaTime);
            }
            else if (hit.collider.GetComponentInParent<FireHazard>() != null)
            {
                hit.collider.GetComponentInParent<FireHazard>().Extinguish(extinguishRate * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// สั่งหยุดฉีดโฟมดับเพลิง
    /// </summary>
    public void StopSpraying()
    {
        if (!isSpraying) return;

        isSpraying = false;

        if (sprayParticleSystem != null && sprayParticleSystem.isPlaying)
        {
            sprayParticleSystem.Stop();
        }

        if (sprayAudioSource != null && sprayAudioSource.isPlaying)
        {
            sprayAudioSource.Stop();
        }
    }

    public bool IsSpraying()
    {
        return isSpraying;
    }
}
