using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 100f;

    [Header("Shooting Settings")]
    public GameObject bulletPrefab; // ช่องใส่ Prefab กระสุน
    public Transform firePoint;     // ช่องใส่จุดปล่อยกระสุน

    void Update()
    {
        // --- ส่วนการเคลื่อนที่ (Movement) ---
        float horizontal = Input.GetAxis("Horizontal"); // ปุ่ม A/D หรือ ลูกศรซ้าย/ขวา
        float vertical = Input.GetAxis("Vertical");     // ปุ่ม W/S หรือ ลูกศรขึ้น/ลง

        // เคลื่อนที่ไปข้างหน้า/หลัง
        transform.Translate(Vector3.forward * vertical * moveSpeed * Time.deltaTime);

        // หมุนตัวละคร (เพื่อให้กล้องหมุนตามไปด้วยแบบง่ายๆ)
        transform.Rotate(Vector3.up * horizontal * rotationSpeed * Time.deltaTime);

        // --- ส่วนการยิงปืน (Shooting) ---
        if (Input.GetButtonDown("Fire1")) // คลิกซ้าย หรือ Ctrl ซ้าย
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // สร้างกระสุนขึ้นมา ณ ตำแหน่ง FirePoint และหันหน้าตาม FirePoint
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}