using TMPro;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("HUD Text")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;

    [Header("End Round Text")]
    [SerializeField] private TMP_Text endScoreText;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void OnEnable()
    {
        if (gameManager == null)
        {
            return;
        }

        gameManager.TimeRemainingChanged += UpdateTimeText;
        gameManager.ScoreChanged += UpdateScoreText;
        gameManager.StateChanged += HandleStateChanged;

        UpdateTimeText(gameManager.TimeRemaining);
        UpdateScoreText(gameManager.Score);
        HandleStateChanged(gameManager.State);
    }

    private void OnDisable()
    {
        if (gameManager == null)
        {
            return;
        }

        gameManager.TimeRemainingChanged -= UpdateTimeText;
        gameManager.ScoreChanged -= UpdateScoreText;
        gameManager.StateChanged -= HandleStateChanged;
    }

    private void UpdateTimeText(float timeRemaining)
    {
        if (timeText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, timeRemaining));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timeText.text = $"Time: {minutes:00}:{seconds:00}";
    }

    private void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }

        if (endScoreText != null)
        {
            endScoreText.text = $"Final Score: {score}";
        }
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        if (endScoreText == null || gameManager == null)
        {
            return;
        }

        if (state == GameManager.GameState.Won)
        {
            endScoreText.text = $"You Win\nFinal Score: {gameManager.Score}";
        }
        else if (state == GameManager.GameState.Lost)
        {
            endScoreText.text = $"Time Up\nFinal Score: {gameManager.Score}";
        }
    }
}
