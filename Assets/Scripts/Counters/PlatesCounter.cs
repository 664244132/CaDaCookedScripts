using System;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    public KitchenObjectSO GetPlateKitchenObjectSO() => plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMAX = 20f; // ปรับเวลาสร้างจานอัตโนมัติเป็น 20 วินาที เพื่อให้การล้างจานเป็นระบบหลัก
    private int platesSpwanedAmount;
    private int platesSpwanedAmountMax = 5;

    private void Start()
    {
        // เริ่มต้นเกมให้มีจานสะอาดพร้อมใช้งาน 4 ใบ
        platesSpwanedAmount = 4;
        for (int i = 0; i < platesSpwanedAmount; i++)
        {
            OnPlateSpawned?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Update()
    {
        spawnPlateTimer += Time.deltaTime;
        if (spawnPlateTimer > spawnPlateTimerMAX )
        {
            spawnPlateTimer = 0f;

            if (platesSpwanedAmount < platesSpwanedAmountMax)
            {
                platesSpwanedAmount++;

                OnPlateSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public override void Interact(Player player)
    {
        // กรณีที่ 1: ผู้เล่นถือจานสะอาดเปล่า (ยังไม่ได้ใส่อาหาร) นำกลับมาเก็บเข้าแท่นวางจาน
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject heldPlate))
            {
                // ตรวจสอบว่าเป็นจานเปล่าที่ยังไม่มีอาหารข้างใน
                if (heldPlate.GetKitchenObjectSOList().Count == 0 && platesSpwanedAmount < platesSpwanedAmountMax)
                {
                    heldPlate.DestroySelf();
                    platesSpwanedAmount++;
                    OnPlateSpawned?.Invoke(this, EventArgs.Empty);
                    Debug.Log($"🍽️ [PlatesCounter] Stored clean plate back. Total: {platesSpwanedAmount}/{platesSpwanedAmountMax}");
                }
            }
            return;
        }

        // กรณีที่ 2: ผู้เล่นมือเปล่า หยิบจานสะอาดไปใช้งาน
        if (platesSpwanedAmount > 0)
        {
            platesSpwanedAmount--;
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            OnPlateRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}
