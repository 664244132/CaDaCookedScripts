using UnityEngine;

/// <summary>
/// สายพานลำเลียง (Conveyor Belt)
/// เลื่อนส่งวัตถุดิบหรือผลักตัวละครที่ยืนอยู่บนสายพานไปยังทิศทางที่กำหนด
/// </summary>
public class ConveyorBelt : MonoBehaviour
{
    [Header("Conveyor Settings")]
    [SerializeField] private Vector3 conveyorDirection = Vector3.forward;
    [SerializeField] private float conveyorSpeed = 2.0f;

    private void OnTriggerStay(Collider other)
    {
        // หากเป็นผู้เล่น ให้ผลักผู้เล่นไปตามทิศทางสายพาน
        if (other.TryGetComponent(out Player player))
        {
            player.transform.position += conveyorDirection.normalized * conveyorSpeed * Time.deltaTime;
        }
        // หากเป็นวัตถุดิบอาหารที่วางอยู่
        else if (other.TryGetComponent(out KitchenObject kitchenObject))
        {
            if (kitchenObject.GetKitchenObjectParent() == null)
            {
                kitchenObject.transform.position += conveyorDirection.normalized * conveyorSpeed * Time.deltaTime;
            }
        }
    }
}
