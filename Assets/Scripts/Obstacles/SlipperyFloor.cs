using UnityEngine;

/// <summary>
/// พื้นคราบน้ำมันลื่น (Slippery Floor)
/// เมื่อตัวละครเชฟเดินผ่าน จะเกิดการลื่นไถลและหมุนตัว ทำให้ควบคุมทิศทางได้ยากขึ้นชั่วคราว
/// </summary>
public class SlipperyFloor : MonoBehaviour
{
    [Header("Slippery Settings")]
    [SerializeField] private float slipSpeedMultiplier = 1.5f; // เพิ่มความเร็วไถล
    [SerializeField] private float slipDuration = 1.2f;        // ระยะเวลาลื่นไถลหลังออกจากพื้นที่
    [SerializeField] private float spinSpeed = 360f;            // ความเร็วในการหมุนตัวตอนลื่น

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.SetSlipping(true, slipSpeedMultiplier, slipDuration, spinSpeed);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.SetSlipping(true, slipSpeedMultiplier, slipDuration, spinSpeed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.TriggerSlipDecay(slipDuration);
        }
    }
}
