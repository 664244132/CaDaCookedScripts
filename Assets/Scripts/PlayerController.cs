using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab; // Prefab กระสุน
    public Transform firePoint;     // จุดปล่อยกระสุน

    void Update()
    {
        // --- การเคลื่อนที่ (Movement) ---
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        transform.Translate(Vector3.forward * vertical * moveSpeed * Time.deltaTime);
        transform.Rotate(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);

        // --- การยิงปืน (Shooting) ---
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        }
    }
}