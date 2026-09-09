using UnityEngine;

public class SelectedCounterVisual : MonoBehaviour
{
    [SerializeField] private BaseCounter baseCounter;
    [SerializeField] private GameObject[] visualGameObjectArray;
    public void SetBaseCounter(BaseCounter baseCounter)
    {
        this.baseCounter = baseCounter;
        if (Player.Instance != null && Player.Instance.GetSelectedCounter() == baseCounter && baseCounter != null)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Start()
    {
        if (baseCounter == null)
        {
            baseCounter = GetComponentInParent<BaseCounter>();
        }

        if (Player.Instance != null)
        {
            Player.Instance.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
        }
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (baseCounter != null && e.selectedCounter == baseCounter)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(true);
        }

    }
    private void Hide()
    {
        foreach (GameObject visualGameObject in visualGameObjectArray)
        {
            visualGameObject.SetActive(false);
        }
    }
}
