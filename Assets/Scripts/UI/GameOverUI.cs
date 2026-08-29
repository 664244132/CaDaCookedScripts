using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// GameOverUI: แสดงผลคะแนนเมื่อจบเกม และพากลับไปยังหน้า Main Menu อัตโนมัติใน 5 วินาที
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipesDeliveryText;

    private Coroutine autoReturnCoroutine;

    private void Start()
    {
        KitchenGameManager.Instance.OnStateChanged += KitchenGameManager_OnStateChanged;

        Hide();
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged -= KitchenGameManager_OnStateChanged;
        }
    }

    private void KitchenGameManager_OnStateChanged(object sender, System.EventArgs e)
    {
        if (KitchenGameManager.Instance.IsGameOver())
        {
            Show();

            recipesDeliveryText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();

            if (autoReturnCoroutine != null)
            {
                StopCoroutine(autoReturnCoroutine);
            }
            autoReturnCoroutine = StartCoroutine(AutoReturnToMainMenuRoutine());
        }
        else
        {
            Hide();
        }
    }

    /// <summary>
    /// หน่วงเวลา 5 วินาที แล้วพากลับสู่หน้า Main Menu อัตโนมัติ
    /// </summary>
    private IEnumerator AutoReturnToMainMenuRoutine()
    {
        yield return new WaitForSecondsRealtime(5.0f);
        Time.timeScale = 1f; // คืนค่า TimeScale เผื่อกรณีเกมถูก Pause ไว้
        Loader.Load(Loader.Scene.MainMenuScene);
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
