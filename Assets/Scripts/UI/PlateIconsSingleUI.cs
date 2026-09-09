using UnityEngine;
using UnityEngine.UI;

public class PlateIconsSingleUI : MonoBehaviour
{
    [SerializeField] private Image image;

    public void SetKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        if (image != null && kitchenObjectSO != null)
        {
            image.sprite = kitchenObjectSO.sprite;
        }
    }

    [System.Obsolete("Typo in original API method name. Use SetKitchenObjectSO instead.")]
    public void SetKitchenOnjectSO(KitchenObjectSO kitchenObjectSO) => SetKitchenObjectSO(kitchenObjectSO);
}
