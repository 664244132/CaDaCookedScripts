using UnityEngine;

/// <summary>
/// เคาน์เตอร์เลื่อนตำแหน่ง (Moving Workstation / Counter)
/// เคลื่อนที่ไปกลับระหว่าง 2 จุด บังคับให้ผู้เล่นต้องกะจังหวะการหยิบ/วางของ
/// รองรับการตั้งค่าระยะทาง ความเร็ว และจังหวะหน่วงเวลาแบบแยกรายตัว
/// </summary>
public class MovingCounter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(2.4f, 0f, 0f); // ระยะและทิศทางที่เลื่อนไป
    [SerializeField] private float moveSpeed = 1.6f;                      // ความเร็วในการเลื่อนไป-กลับ
    [SerializeField] private float timeOffset = 0f;                       // ออฟเซ็ตเวลาเพื่อให้แต่ละตัวเลื่อนไม่พร้อมกัน

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    private void InitializeIfNeeded()
    {
        if (isInitialized) return;
        startPosition = transform.position;
        targetPosition = startPosition + moveOffset;
        isInitialized = true;
    }

    /// <summary>
    /// กำหนดค่าระยะทาง ความเร็ว และจังหวะออฟเซ็ตสำหรับเคาน์เตอร์ตัวนี้
    /// </summary>
    public void Setup(Vector3 offset, float speed, float phaseOffset = 0f)
    {
        moveOffset = offset;
        moveSpeed = speed;
        timeOffset = phaseOffset;
        startPosition = transform.position;
        targetPosition = startPosition + moveOffset;
        isInitialized = true;
    }

    private void Update()
    {
        InitializeIfNeeded();
        float pingPongTime = Mathf.PingPong((Time.time + timeOffset) * moveSpeed, 1f);
        transform.position = Vector3.Lerp(startPosition, targetPosition, pingPongTime);
    }
}
