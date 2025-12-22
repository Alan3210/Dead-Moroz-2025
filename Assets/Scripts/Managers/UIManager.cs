using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameplayHUDPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI monthText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Button pauseButton;

    [Header("Main Menu Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private TextMeshProUGUI controlsText;

    [Header("Pause Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartFromPauseButton;
    [SerializeField] private Button quitFromPauseButton;

    [Header("Game Over Elements")]
    [SerializeField] private TextMeshProUGUI gameOverMonthText;
    [SerializeField] private Button restartFromGameOverButton;
    [SerializeField] private Button quitFromGameOverButton;

    [Header("Victory Elements")]
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    [SerializeField] private Button restartFromVictoryButton;
    [SerializeField] private Button quitFromVictoryButton;

    [Header("Month Transition Elements")]
    [SerializeField] private MonthTransitionEffect monthTransitionEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSFX;

    [SerializeField] private TextMeshProUGUI mainMenuHighScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;


    private bool isPaused = false;
    private Keyboard keyboard;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        keyboard = Keyboard.current;
    }

    private void Start()
    {
        SetupButtonListeners();
        ShowMainMenu();
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameActive)
        {
            UpdateHUD();
            HandlePauseInput();
        }
    }

    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseGame);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeGame);

        if (restartFromPauseButton != null)
            restartFromPauseButton.onClick.AddListener(OnRestartGame);

        if (quitFromPauseButton != null)
            quitFromPauseButton.onClick.AddListener(OnQuitToMainMenu);

        if (restartFromGameOverButton != null)
            restartFromGameOverButton.onClick.AddListener(OnRestartGame);

        if (quitFromGameOverButton != null)
            quitFromGameOverButton.onClick.AddListener(OnQuitToMainMenu);

        if (restartFromVictoryButton != null)
            restartFromVictoryButton.onClick.AddListener(OnRestartGame);

        if (quitFromVictoryButton != null)
            quitFromVictoryButton.onClick.AddListener(OnQuitToMainMenu);
    }

    private void UpdateHUD()
    {
        if (GameManager.Instance == null) return;

        if (monthText != null)
        {
            int month = GameManager.Instance.CurrentMonth;
            monthText.text = $"ћес€ц: {month}/12";
        }

        if (speedText != null)
        {
            float speed = GameManager.Instance.CurrentSpeed;
            speedText.text = $"—корость: {speed:F0}";
        }
    }

    private void HandlePauseInput()
    {
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && !isPaused)
        {
            OnPauseGame();
        }
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        if (mainMenuHighScoreText != null && GameManager.Instance != null)
        {
            int highScore = GameManager.Instance.HighScore;
            if (highScore > 0)
            {
                mainMenuHighScoreText.text = $"Ћучший результат: ћес€ц {highScore}";
            }
            else
            {
                mainMenuHighScoreText.text = "—ыграйте первую игру!";
            }
        }

    }

    public void ShowGameplayHUD()
    {
        HideAllPanels();
        if (gameplayHUDPanel != null)
            gameplayHUDPanel.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ShowPauseMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ShowGameOver(int monthReached)
    {
        HideAllPanels();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            //if (gameOverMonthText != null)
            //{
            //    gameOverMonthText.text = $"¬ы дошли до мес€ца: {monthReached}";
            //}
        }

        if (gameOverHighScoreText != null && GameManager.Instance != null)
        {
            int highScore = GameManager.Instance.HighScore;
            if (highScore > monthReached)
            {
                gameOverHighScoreText.text = $"–екорд: ћес€ц {highScore}";
            }
            else
            {
                gameOverHighScoreText.text = "Ќјƒќ Ќ≈ћЌќ√ќ ѕќ“≈–ѕ≈“№";
            }
        }


        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        HideAllPanels();
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryMessageText != null)
            {
                victoryMessageText.text = "ѕоздравл€ем!\n¬ы прошли все 12 мес€цев 2025 года!";
            }
        }

        Time.timeScale = 0f;
    }

    public void ShowMonthTransition(int month, string monthName)
    {
        if (monthTransitionEffect != null)
        {
            if (!monthTransitionEffect.gameObject.activeInHierarchy)
            {
                monthTransitionEffect.gameObject.SetActive(true);
            }
            monthTransitionEffect.ShowTransition(month, monthName);
        }
    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX);
        }
    }


    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameplayHUDPanel != null) gameplayHUDPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }


    private void OnStartGame()
    {
        PlayButtonClickSound();
        ShowGameplayHUD();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    private void OnPauseGame()
    {
        ShowPauseMenu();
    }

    private void OnResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    private void OnRestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnQuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
