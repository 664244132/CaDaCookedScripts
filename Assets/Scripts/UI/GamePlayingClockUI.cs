using UnityEngine;
using UnityEngine.UI;

public class GamePlayingClockUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;

    private void Update()
    {
        if (KitchenGameManager.Instance != null && timerImage != null)
        {
            timerImage.fillAmount = KitchenGameManager.Instance.GetGamePlayingTimerNormalized();
        }
    }
}
