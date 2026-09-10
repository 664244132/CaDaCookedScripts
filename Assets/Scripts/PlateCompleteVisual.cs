using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
    [Serializable]

    public struct KitchenObjectSO_GameObject
    {
        public KitchenObjectSO kitchenObjectSO;
        public GameObject gameObject;
    }

    [UnityEngine.Serialization.FormerlySerializedAs("PlateKitchenObject")]
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> kitchenObjectSOGameObjectList;

    private void Start()
    {
        if (plateKitchenObject != null)
        {
            plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
            plateKitchenObject.OnIngredientRemoved += PlateKitchenObject_OnIngredientRemoved;
            plateKitchenObject.OnIngredientsCleared += PlateKitchenObject_OnIngredientsCleared;
        }

        foreach (KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectList)
        { 
            kitchenObjectSOGameObject.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (plateKitchenObject != null)
        {
            plateKitchenObject.OnIngredientAdded -= PlateKitchenObject_OnIngredientAdded;
            plateKitchenObject.OnIngredientRemoved -= PlateKitchenObject_OnIngredientRemoved;
            plateKitchenObject.OnIngredientsCleared -= PlateKitchenObject_OnIngredientsCleared;
        }
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectList)
        {
            if (kitchenObjectSOGameObject.kitchenObjectSO == e.kitchenObjectSO)
            {
                kitchenObjectSOGameObject.gameObject.SetActive(true);
            }
        }
    }

    private void PlateKitchenObject_OnIngredientRemoved(object sender, PlateKitchenObject.OnIngredientRemovedEventArgs e)
    {
        foreach (KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectList)
        {
            if (kitchenObjectSOGameObject.kitchenObjectSO == e.kitchenObjectSO)
            {
                kitchenObjectSOGameObject.gameObject.SetActive(false);
            }
        }
    }

    private void PlateKitchenObject_OnIngredientsCleared(object sender, EventArgs e)
    {
        foreach (KitchenObjectSO_GameObject kitchenObjectSOGameObject in kitchenObjectSOGameObjectList)
        {
            kitchenObjectSOGameObject.gameObject.SetActive(false);
        }
    }
}
