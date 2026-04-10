using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        None,
        Playing,
        Paused,
        Won,
        Lost
    }

    public static GameManager Instance { get; private set; }

    [Header("Round")]
    [SerializeField] private float roundDurationSeconds = 120f;
    [SerializeField] private bool startRoundOnStart = true;
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private bool pauseTimeScale = true;

    [Header("Score")]
    [SerializeField] private int valuableItems;
    [SerializeField] private int lowValueItems;

    public event Action<GameState> StateChanged;
    public event Action<float> TimeRemainingChanged;
    public event Action<int> ScoreChanged;

    public GameState State { get; private set; } = GameState.None;
    public float TimeRemaining { get; private set; }
    public int ValuableItems => valuableItems;
    public int LowValueItems => lowValueItems;
    public int Score => valuableItems - lowValueItems;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (startRoundOnStart)
        {
            StartRound();
        }
    }

    private void Update()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        TimeRemaining = Mathf.Max(0f, TimeRemaining - dt);
        TimeRemainingChanged?.Invoke(TimeRemaining);

        if (TimeRemaining <= 0f)
        {
            EndRoundFromTimeout();
        }
    }

    public void StartRound()
    {
        valuableItems = 0;
        lowValueItems = 0;
        TimeRemaining = Mathf.Max(0f, roundDurationSeconds);

        ScoreChanged?.Invoke(Score);
        TimeRemainingChanged?.Invoke(TimeRemaining);

        SetState(GameState.Playing);
        if (pauseTimeScale)
        {
            Time.timeScale = 1f;
        }
    }

    public void PauseGame()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        SetState(GameState.Paused);
        if (pauseTimeScale)
        {
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        if (State != GameState.Paused)
        {
            return;
        }

        SetState(GameState.Playing);
        if (pauseTimeScale)
        {
            Time.timeScale = 1f;
        }
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
        {
            PauseGame();
        }
        else if (State == GameState.Paused)
        {
            ResumeGame();
        }
    }

    public void AddValuableItem(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        valuableItems += amount;
        ScoreChanged?.Invoke(Score);
    }

    public void AddLowValueItem(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        lowValueItems += amount;
        ScoreChanged?.Invoke(Score);
    }

    public void CheckoutPlayer()
    {
        if (State != GameState.Playing && State != GameState.Paused)
        {
            return;
        }

        if (pauseTimeScale)
        {
            Time.timeScale = 1f;
        }

        SetState(GameState.Won);
    }

    public void EndRoundFromTimeout()
    {
        if (State != GameState.Playing)
        {
            return;
        }

        if (pauseTimeScale)
        {
            Time.timeScale = 1f;
        }

        SetState(GameState.Lost);
    }

    public void ReturnToMenu()
    {
        if (pauseTimeScale)
        {
            Time.timeScale = 1f;
        }

        TimeRemaining = Mathf.Max(0f, roundDurationSeconds);
        valuableItems = 0;
        lowValueItems = 0;

        TimeRemainingChanged?.Invoke(TimeRemaining);
        ScoreChanged?.Invoke(Score);

        SetState(GameState.None);
    }

    private void SetState(GameState newState)
    {
        if (State == newState)
        {
            return;
        }

        State = newState;
        StateChanged?.Invoke(State);
    }
}
