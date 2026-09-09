using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }


    [SerializeField] private List<KitchenObjectSO> validKitchenObjectSOList;

    private List<KitchenObjectSO> kitchenObjectSOList;

    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }

        if (kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }
        else
        {
            kitchenObjectSOList.Add(kitchenObjectSO);

            OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
                {
                kitchenObjectSO = kitchenObjectSO
                });

            return true;
        }
    }

    public event EventHandler OnIngredientsCleared;

    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }

    /// <summary>
    /// เทวัตถุดิบและอาหารทั้งหมดออกจากจาน (เช่น เมื่อนำไปเทลงถังขยะ TrashCounter)
    /// </summary>
    public void ClearIngredients()
    {
        kitchenObjectSOList.Clear();
        OnIngredientsCleared?.Invoke(this, EventArgs.Empty);
    }
}
