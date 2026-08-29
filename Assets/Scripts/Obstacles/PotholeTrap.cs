using System;
using UnityEngine;

/// <summary>
/// ท่อระบายน้ำ/หลุมดักในครัว (Pothole Trap)
/// เมื่อผู้เล่นเดินผ่าน จะทำให้สะดุดความเร็วลดลงชั่วคราว และมีโอกาสทำของหลุดมือ
/// </summary>
public class PotholeTrap : MonoBehaviour
{
    public static event EventHandler OnPlayerTripped;

    [Header("Trap Settings")]
    [SerializeField] private float slowDuration = 1.0f;       // ระยะเวลาที่เดินช้าลง (วินาที)
    [SerializeField] private float slowMultiplier = 0.4f;      // ความเร็วลดเหลือ 40%
    [SerializeField] private bool dropItemOnTrip = false;      // บังคับทำของตกหรือไม่

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.ApplySlowEffect(slowMultiplier, slowDuration);

            if (dropItemOnTrip && player.HasKitchenObject())
            {
                player.GetKitchenObject().DestroySelf();
            }

            OnPlayerTripped?.Invoke(this, EventArgs.Empty);
        }
    }
}
