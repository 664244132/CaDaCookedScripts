using System;
using UnityEngine;

/// <summary>
/// เคาน์เตอร์ถังขยะ (Trash Counter)
/// ตามมาตรฐาน Overcooked (Q3 ตัวเลือก A):
/// 1. หากผู้เล่นถือจานที่มีอาหาร/วัตถุดิบ ให้เทเฉพาะเศษอาหารทิ้ง แล้วคงจานเปล่าไว้ในมือเสมอ
/// 2. หากถือจานเปล่าสะอาด หรือถือจานเปื้อน (DirtyPlateKitchenObject) จะไม่อนุญาตให้ทิ้งจานลงถังขยะเด็ดขาด
/// 3. วัตถุดิบเดี่ยวๆ หรืออาหารไหม้บนเตาที่ไม่ได้ใส่จาน สามารถทิ้งทำลายได้ตามปกติ
/// </summary>
public class TrashCounter : BaseCounter
{
    public static event EventHandler OnAnyObjectTrashed;

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject()) return;

        KitchenObject heldObject = player.GetKitchenObject();

        // 1. กรณีถือจานอาหาร -> เทเฉพาะวัตถุดิบทิ้ง แล้วคงจานเปล่าไว้ในมือผู้เล่น
        if (heldObject.TryGetPlate(out PlateKitchenObject plate))
        {
            if (plate.GetKitchenObjectSOList().Count > 0)
            {
                plate.ClearIngredients();
                OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
                Debug.Log("🗑️ [TrashCounter] Dumped ingredients from plate! Clean empty plate retained in hand.");
            }
            // หากเป็นจานเปล่า ไม่เกิดอะไรขึ้น (ป้องกันการทิ้งจานเปล่าลงถังขยะ)
            return;
        }

        // 2. กรณีถือจานเปื้อน (DirtyPlateKitchenObject) -> ไม่อนุญาตให้ทิ้งจานเปื้อนลงถังขยะ
        if (heldObject is DirtyPlateKitchenObject)
        {
            Debug.Log("🚫 [TrashCounter] Cannot trash dirty plates! Wash them in the sink.");
            return;
        }

        // 3. กรณีถือถังดับเพลิง -> ไม่อนุญาตให้ทิ้งถาวร ให้สั่ง Respawn กลับไปจุดเริ่มต้น
        if (heldObject is FireExtinguisher fireExtinguisher)
        {
            fireExtinguisher.ScheduleRespawn(1.0f);
            OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
            Debug.Log("🧯 [TrashCounter] FireExtinguisher placed in trash! Respawing at initial position in 1.0s...");
            return;
        }

        // 4. กรณีถือวัตถุดิบอื่นๆ (เช่น ผัก, เนื้อไหม้) -> ทำลายทิ้งตามปกติ
        heldObject.DestroySelf();
        OnAnyObjectTrashed?.Invoke(this, EventArgs.Empty);
    }
}