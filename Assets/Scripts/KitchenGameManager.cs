using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state;
    private float WaitingToStartTimer = 1f;
    private float countdownToStartTimer = 15f; // เพิ่มเวลานับถอยหลังเริ่มเกมและสอนเล่นเป็น 15 วินาที
    private float gamePlayingTimer;
    private float gamePlayingTimerMax = 150f; // 2 นาที 30 วินาที
    private bool isGamePaused = false;

    private void Awake()
    {
        Instance = this;
        state = State.WaitingToStart;
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;

        // รับประกันการติดตั้งและเริ่มทำงานของระบบ 2.1 - 2.5 เสมอ
        if (FindFirstObjectByType<GameplayEventsBootstrap>() == null)
        {
            GameObject bootstrapObj = new GameObject("--- GAMEPLAY SYSTEMS BOOTSTRAP ---");
            bootstrapObj.AddComponent<GameplayEventsBootstrap>();
        }
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnPauseAction -= GameInput_OnPauseAction;
        }
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                WaitingToStartTimer -= Time.deltaTime;
                if (WaitingToStartTimer < 0f)
                {
                    state = State.CountdownToStart;
                    countdownToStartTimer = 15f;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                if (countdownToStartTimer < 0f)
                {
                    state = State.GamePlaying;
                    gamePlayingTimer = gamePlayingTimerMax;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer < 0f)
                {
                    state = State.GameOver;
                    OnStateChanged?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.GameOver:
                break;
        }
        Debug.Log(state);
    }

    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return countdownToStartTimer;
    }

    public bool IsGameOver()
    {
        return state == State.GameOver;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (gamePlayingTimer / gamePlayingTimerMax);
    }

    private float lastTogglePauseTime;

    public void TogglePauseGame()
    {
        if (Time.unscaledTime - lastTogglePauseTime < 0.15f) return;
        lastTogglePauseTime = Time.unscaledTime;

        isGamePaused = !isGamePaused;
        Debug.Log("KitchenGameManager: TogglePauseGame - isGamePaused = " + isGamePaused);

        if (isGamePaused)
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
