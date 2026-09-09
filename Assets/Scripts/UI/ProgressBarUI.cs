using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
    [SerializeField] private GameObject hasProgressGameObject;
    [SerializeField] private Image barImage;

    private IHasProgress hasProgress;

    private void Start()
    {
        if (hasProgressGameObject != null)
        {
            hasProgress = hasProgressGameObject.GetComponent<IHasProgress>();
        }

        if (hasProgress == null)
        {
            Debug.LogError("Game Object " + hasProgressGameObject + " does not implement IHasProgress");
            return;
        }

        hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;

        if (barImage != null)
        {
            barImage.fillAmount = 0f;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (hasProgress != null)
        {
            hasProgress.OnProgressChanged -= HasProgress_OnProgressChanged;
        }
    }

    private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        if (barImage != null)
        {
            barImage.fillAmount = e.progressNormalized;
        }

        if (e.progressNormalized == 0f || e.progressNormalized >= 1f)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}