using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private Transform iconTemplate;

    private void Awake()
    {
        iconTemplate.gameObject.SetActive(false);
    }
    private void Start()
    {
        if (plateKitchenObject != null)
        {
            plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
            plateKitchenObject.OnIngredientRemoved += PlateKitchenObject_OnIngredientRemoved;
            plateKitchenObject.OnIngredientsCleared += PlateKitchenObject_OnIngredientsCleared;
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
        UpdateVisual();
    }

    private void PlateKitchenObject_OnIngredientRemoved(object sender, PlateKitchenObject.OnIngredientRemovedEventArgs e)
    {
        UpdateVisual();
    }

    private void PlateKitchenObject_OnIngredientsCleared(object sender, System.EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach (Transform child in transform)
        {
            if (child == iconTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (KitchenObjectSO kitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
        {
            Transform iconTransform = Instantiate(iconTemplate, transform);
            iconTransform.gameObject.SetActive(true);
            if (iconTransform.TryGetComponent(out PlateIconsSingleUI singleUI))
            {
                singleUI.SetKitchenObjectSO(kitchenObjectSO);
            }
        }
    }
}
