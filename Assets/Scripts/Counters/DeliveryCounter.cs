using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
            {
                DeliveryManager.Instance.DeliveryRecipe(plateKitchenObject);
                player.GetKitchenObject().DestroySelf();
            }
            else
            {
                // The object is not a plate (e.g. raw ingredient)
                DeliveryManager.Instance.DeliverIncorrectRecipe();
                player.GetKitchenObject().DestroySelf();
            }
        }
    }
}
