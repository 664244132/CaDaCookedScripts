using System;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO plateKitchenObjectSO;

    private float spawnPlateTimer;
    private float spawnPlateTimerMAX = 3f;
    private int platesSpwanedAmount;
    private int platesSpwanedAmountMax = 5;

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
        if (player.HasKitchenObject() || platesSpwanedAmount <= 0) return;

        platesSpwanedAmount--;
        KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}
