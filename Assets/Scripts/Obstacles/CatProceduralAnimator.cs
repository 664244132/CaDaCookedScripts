using UnityEngine;

/// <summary>
/// แอนิเมชันการเดิน-วิ่งแบบ Procedural สไตล์น่ารักดุ๊กดิ๊กสำหรับโมเดล Neko Cat
/// ขยับขา, ส่ายหาง, โยกตัว (Bobbing & Waddling) อัตโนมัติตามความเร็วการเคลื่อนที่
/// </summary>
public class CatProceduralAnimator : MonoBehaviour
{
    [Header("Bouncy Locomotion Settings")]
    [SerializeField] private float walkAnimFrequency = 12f;
    [SerializeField] private float runAnimFrequency = 22f;
    [SerializeField] private float bounceHeight = 0.08f;
    [SerializeField] private float waddleAngle = 8f;
    [SerializeField] private float pawSwingAngle = 30f;
    [SerializeField] private float tailWagAngle = 25f;

    private Transform rightHand;
    private Transform leftHand;
    private Transform head;
    private Transform tail;
    private Transform visualRoot;

    private Vector3 initialVisualPos;
    private Quaternion initialRightHandRot;
    private Quaternion initialLeftHandRot;
    private Quaternion initialTailRot;
    private Vector3 lastPosition;
    private float currentSpeed;

    private void Awake()
    {
        // ค้นหาชิ้นส่วนกระดูก/โมเดลในตัวแมว
        rightHand = FindDeepChild(transform, "+ R Hand");
        leftHand = FindDeepChild(transform, "+ L Hand");
        head = FindDeepChild(transform, "+ Head");
        tail = FindDeepChild(transform, "Neko Cat Tail") ?? FindDeepChild(transform, "Tail");

        // กำหนด Visual Root สำหรับทำ Bounce Y
        visualRoot = transform;
        initialVisualPos = visualRoot.localPosition;

        if (rightHand != null) initialRightHandRot = rightHand.localRotation;
        if (leftHand != null) initialLeftHandRot = leftHand.localRotation;
        if (tail != null) initialTailRot = tail.localRotation;

        lastPosition = transform.position;
    }

    private void Update()
    {
        // คำนวณความเร็วการเคลื่อนที่จริง
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        currentSpeed = distanceMoved / Mathf.Max(Time.deltaTime, 0.001f);
        lastPosition = transform.position;

        bool isMoving = currentSpeed > 0.1f;
        float freq = currentSpeed > 4.5f ? runAnimFrequency : walkAnimFrequency;
        float animSpeed = isMoving ? freq : 3f; // ส่ายหางเบาๆ ตอนอยู่นิ่ง

        float timeVal = Time.time * animSpeed;

        // 1. โยกตัวขึ้นลง (Vertical Bounce) & เอียงซ้ายขวา (Waddle)
        if (isMoving)
        {
            float bounceY = Mathf.Abs(Mathf.Sin(timeVal)) * bounceHeight * Mathf.Clamp01(currentSpeed / 3f);
            float waddleZ = Mathf.Sin(timeVal * 0.5f) * waddleAngle;

            transform.localRotation = Quaternion.Euler(0, transform.localEulerAngles.y, waddleZ);

            // 2. ขยับขาหน้า ซ้าย-ขวา สลับกัน (Paw Steps)
            float pawOffset = Mathf.Sin(timeVal) * pawSwingAngle;

            if (rightHand != null)
            {
                rightHand.localRotation = initialRightHandRot * Quaternion.Euler(pawOffset, 0, 0);
            }
            if (leftHand != null)
            {
                leftHand.localRotation = initialLeftHandRot * Quaternion.Euler(-pawOffset, 0, 0);
            }
        }
        else
        {
            // คืนท่าทางปกติเมื่อหยุดนิ่ง
            transform.localRotation = Quaternion.Euler(0, transform.localEulerAngles.y, 0);
            if (rightHand != null) rightHand.localRotation = Quaternion.Slerp(rightHand.localRotation, initialRightHandRot, Time.deltaTime * 10f);
            if (leftHand != null) leftHand.localRotation = Quaternion.Slerp(leftHand.localRotation, initialLeftHandRot, Time.deltaTime * 10f);
        }

        // 3. ส่ายหางดุ๊กดิ๊กตลอดเวลา (Tail Wagging)
        if (tail != null)
        {
            float tailWag = Mathf.Sin(Time.time * (isMoving ? freq * 1.2f : 4f)) * tailWagAngle;
            tail.localRotation = initialTailRot * Quaternion.Euler(0, tailWag, 0);
        }

        // 4. ดุ๊กดิ๊กหัวเล็กน้อยตามจังหวะก้าว
        if (head != null && isMoving)
        {
            float headBob = Mathf.Sin(timeVal * 0.5f) * 4f;
            head.localRotation *= Quaternion.Euler(headBob * Time.deltaTime, 0, 0);
        }
    }

    private Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.Contains(childName))
            {
                return child;
            }
        }
        return null;
    }
}
