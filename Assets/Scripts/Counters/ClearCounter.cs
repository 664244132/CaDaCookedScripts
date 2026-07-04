using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ClearCounter : BaseCounter
{

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player) // drop items on counters
    {
        if (!HasKitchenObject()) // ถ้าไม่มี obj
        {
            if (player.HasKitchenObject())
            {
                // player ถืออะไรมาด้วย
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            else
            {
                // ถ้าไม่ได้ถืออะไร
            }
        }
        else // ถ้ามี obj อยู่่บน counter อยู่เเล้ว
        {
            if(player.HasKitchenObject())
            {
                // player ถืออะไรมาด้วย
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // ผู้เล่นกำลังถือจายอยู่
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        GetKitchenObject().DestroySelf();
                    }
                }
                else
                {
                    if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
                    {
                        if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                        {
                            player.GetKitchenObject().DestroySelf();
                        }
                    }
                }
            }
            else
            {
                // player ไม่ได้ถืออะไร
                GetKitchenObject().SetKitchenObjectParent(player);
            }
        }

    }
}