using UnityEngine;

/// <summary>
/// พื้นคราบน้ำมันลื่น (Slippery Floor)
/// เมื่อตัวละครเชฟเดินผ่าน จะเกิดการลื่นไถลพุ่งไปข้างหน้าสั้นๆ และหมุนตัว 360 องศา (Quick Slip Impulse 0.45s)
/// แก้ไขปัญหาลื่นไม่หยุด โดยมีระบบ Cooldown และจำกัดระยะเวลาลื่นสั้นกระชับ ไม่รบกวนการเล่น
/// </summary>
public class SlipperyFloor : MonoBehaviour
{
    [Header("Quick Slip Settings")]
    [SerializeField] private float slipSpeedMultiplier = 1.45f; // เพิ่มความเร็วไถลเล็กน้อย
    [SerializeField] private float slipDuration = 0.45f;        // ลื่นแค่ 0.45 วินาที (แป๊บเดียว)
    [SerializeField] private float spinSpeed = 360f;            // หมุนตัว 360 องศา
    [SerializeField] private float triggerRadius = 1.4f;        // รัศมีตรวจจับ

    private void Update()
    {
        if (Player.Instance != null)
        {
            Vector3 playerPos = Player.Instance.transform.position;
            Vector3 puddlePos = transform.position;
            playerPos.y = 0f;
            puddlePos.y = 0f;

            if (Vector3.Distance(puddlePos, playerPos) <= triggerRadius)
            {
                Player.Instance.TriggerSlipImpulse(slipSpeedMultiplier, slipDuration, spinSpeed);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player) || (player = other.GetComponentInParent<Player>()) != null)
        {
            player.TriggerSlipImpulse(slipSpeedMultiplier, slipDuration, spinSpeed);
        }
    }
}
