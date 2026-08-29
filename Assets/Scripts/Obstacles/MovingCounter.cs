using UnityEngine;

/// <summary>
/// เคาน์เตอร์เลื่อนตำแหน่ง (Moving Workstation / Counter)
/// เคลื่อนที่ไปกลับระหว่าง 2 จุด บังคับให้ผู้เล่นต้องกะจังหวะการหยิบ/วางของ
/// </summary>
public class MovingCounter : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveOffset = new Vector3(3.0f, 0f, 0f); // ระยะที่เลื่อนไป
    [SerializeField] private float moveSpeed = 2.2f;                      // ความเร็วในการเลื่อนไป-กลับ

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        startPosition = transform.position;
        targetPosition = startPosition + moveOffset;
    }

    private void Update()
    {
        float pingPongTime = Mathf.PingPong(Time.time * moveSpeed, 1f);
        transform.position = Vector3.Lerp(startPosition, targetPosition, pingPongTime);
    }
}
