using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// สคริปต์ควบคุมตัวละครทดลอง (Standalone Experimental Player Controller)
/// รองรับการเคลื่อนที่และการยิงปืนพื้นฐานสำหรับฉากทดสอบด้วย New Input System
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject bulletPrefab; // Prefab กระสุน
    [SerializeField] private Transform firePoint;     // จุดปล่อยกระสุน

    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public GameObject BulletPrefab => bulletPrefab;
    public Transform FirePoint => firePoint;

    private void Update()
    {
        // Guard Clause: ตรวจสอบความปลอดภัยของฮาร์ดแวร์คีย์บอร์ด (Null Safety)
        if (Keyboard.current == null) return;

        // --- การเคลื่อนที่ (Movement ด้วย Unity New Input System) ---
        float horizontal = 0f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;

        float vertical = 0f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;

        transform.Translate(Vector3.forward * vertical * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);

        // --- การยิงปืน (Shooting: คลิกซ้ายเมาส์ หรือกด Spacebar หรือ Left Ctrl) ---
        bool isShootPressed = (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                              Keyboard.current.spaceKey.wasPressedThisFrame ||
                              Keyboard.current.leftCtrlKey.wasPressedThisFrame;

        if (isShootPressed)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}