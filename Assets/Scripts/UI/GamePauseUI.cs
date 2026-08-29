using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// เมนูหยุดเกมชั่วคราว (Game Pause UI)
/// แสดงหน้าต่าง Pause เมื่อผู้เล่นกดปุ่ม ESC พร้อมปุ่ม Resume (เล่นต่อ) และ Main Menu (กลับหน้าแรก)
/// </summary>
public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;

    private void Awake()
    {
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(Resume);
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(MainMenu);
        }
    }

    private void Start()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnGamePaused += KitchenGameManager_OnGamePaused;
            KitchenGameManager.Instance.OnGameUnpaused += KitchenGameManager_OnGameUnpaused;
        }
        Hide();
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnGamePaused -= KitchenGameManager_OnGamePaused;
            KitchenGameManager.Instance.OnGameUnpaused -= KitchenGameManager_OnGameUnpaused;
        }
    }

    public void Resume()
    {
        Debug.Log("GamePauseUI: Resume Clicked");
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.TogglePauseGame();
        }
        else
        {
            Time.timeScale = 1f;
            Hide();
        }
    }

    public void MainMenu()
    {
        Debug.Log("GamePauseUI: MainMenu Clicked");
        Time.timeScale = 1f;
        Loader.Load(Loader.Scene.MainMenuScene);
    }

    private void KitchenGameManager_OnGameUnpaused(object sender, System.EventArgs e)
    {
        Hide();
    }

    private void KitchenGameManager_OnGamePaused(object sender, System.EventArgs e)
    {
        Show();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (resumeButton != null)
        {
            resumeButton.Select();
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
