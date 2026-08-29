using UnityEngine;

/// <summary>
/// ระบบจำลองคลื่นและการเอียงของแพ/เรือในครัวธีมชายหาด (Raft Kitchen Tilt)
/// ทำให้แพและวัตถุในครัวเอียงตามจังหวะคลื่นทะเล เพิ่มบรรยากาศและความท้าทาย
/// </summary>
public class RaftKitchenTilt : MonoBehaviour
{
    [Header("Wave & Tilt Settings")]
    [SerializeField] private float tiltAngleMax = 3.5f;   // องศาการเอียงสูงสุด
    [SerializeField] private float tiltSpeed = 1.2f;       // ความถี่ของคลื่น
    [SerializeField] private float bobHeight = 0.15f;      // ความสูงการลอยขึ้น-ลงตามคลื่น

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        // คำนวณการลอยขึ้นลงตามคลื่น
        float newY = initialPosition.y + Mathf.Sin(Time.time * tiltSpeed) * bobHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        // คำนวณการเอียง Roll & Pitch
        float tiltX = Mathf.Sin(Time.time * tiltSpeed * 0.8f) * tiltAngleMax;
        float tiltZ = Mathf.Cos(Time.time * tiltSpeed) * tiltAngleMax;

        transform.rotation = initialRotation * Quaternion.Euler(tiltX, 0f, tiltZ);
    }
}
