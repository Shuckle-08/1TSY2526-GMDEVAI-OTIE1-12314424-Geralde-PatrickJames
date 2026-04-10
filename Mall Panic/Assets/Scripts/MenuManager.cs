using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject endRoundPanel;

    [Header("Flow")]
    [SerializeField] private bool singleSceneMode = true;

    [Header("Scene Names")]
    [SerializeField] private string gameplaySceneName = "SampleScene";
    [SerializeField] private string mainMenuSceneName = "";

    [Header("Cursor")]
    [SerializeField] private bool controlCursor = true;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged += HandleStateChanged;
            HandleStateChanged(gameManager.State);
        }
        else
        {
            SetPanelState(mainMenuPanel, true);
            SetPanelState(hudPanel, false);
            SetPanelState(pausePanel, false);
            SetPanelState(endRoundPanel, false);
        }
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.StateChanged -= HandleStateChanged;
        }
    }

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.TogglePause();
        }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (singleSceneMode || string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            if (gameManager != null)
            {
                gameManager.StartRound();
            }
            return;
        }

        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ResumeGame()
    {
        if (gameManager != null)
        {
            gameManager.ResumeGame();
        }
    }

    public void PauseGame()
    {
        if (gameManager != null)
        {
            gameManager.PauseGame();
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (singleSceneMode)
        {
            if (gameManager != null)
            {
                gameManager.StartRound();
            }
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;

        if (singleSceneMode)
        {
            if (gameManager != null)
            {
                gameManager.ReturnToMenu();
            }
            return;
        }

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void HandleStateChanged(GameManager.GameState state)
    {
        bool isMainMenu = state == GameManager.GameState.None;
        bool isPaused = state == GameManager.GameState.Paused;
        bool isEnded = state == GameManager.GameState.Won || state == GameManager.GameState.Lost;
        bool isPlayingLike = state == GameManager.GameState.Playing || isPaused;

        SetPanelState(mainMenuPanel, isMainMenu);
        SetPanelState(hudPanel, isPlayingLike);
        SetPanelState(pausePanel, isPaused);
        SetPanelState(endRoundPanel, isEnded);

        if (controlCursor)
        {
            bool showCursor = isMainMenu || isPaused || isEnded;
            Cursor.visible = showCursor;
            Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    private void SetPanelState(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
