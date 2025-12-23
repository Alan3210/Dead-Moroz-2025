using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI controlsText;

    [Header("Main Menu Elements")]
    [SerializeField] private Button startButton;
    
    [Header("Pause Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartFromPauseButton;
    [SerializeField] private Button quitFromPauseButton;

    [Header("Game Over Elements")]
    [SerializeField] private TextMeshProUGUI gameOverMonthText;
    [SerializeField] private Button restartFromGameOverButton;
    [SerializeField] private Button quitFromGameOverButton;
    [SerializeField] private PassedObstaclesList passedObstaclesList;

    [Header("Victory Elements")]
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    [SerializeField] private Button restartFromVictoryButton;
    [SerializeField] private Button quitFromVictoryButton;

    [Header("Month Transition Elements")]
    [SerializeField] private MonthTransitionEffect monthTransitionEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSFX;


    private bool isPaused = false;
    private Keyboard keyboard;
    private const float CONTROLS_DISPLAY_DURATION = 5f;
    private float controlsDisplayTimer = 0f;
    private bool showingControls = false;

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
            HandleControlsDisplay();
        }
    }

    private void HandleControlsDisplay()
    {
        if (showingControls && controlsText != null)
        {
            controlsDisplayTimer += Time.deltaTime;

            if (controlsDisplayTimer >= CONTROLS_DISPLAY_DURATION)
            {
                controlsText.gameObject.SetActive(false);
                showingControls = false;
            }
        }
    }


    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

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
            string monthName = GetMonthName(month);
            monthText.text = monthName;
        }
    }

    private string GetMonthName(int month)
    {
        string[] monthNames = {
        "Январь", "Февраль", "Март", "Апрель",
        "Май", "Июнь", "Июль", "Август",
        "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
    };

        return month >= 1 && month <= 12 ? monthNames[month - 1] : "Неизвестный месяц";
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

        if (controlsText != null)
            controlsText.gameObject.SetActive(false);
        
        showingControls = false;
        controlsDisplayTimer = 0f;

        Time.timeScale = 1f;
        isPaused = false;

    }

    public void ShowGameplayHUD()
    {
        HideAllPanels();
        if (gameplayHUDPanel != null)
            gameplayHUDPanel.SetActive(true);

        if (controlsText != null)
        {
            controlsText.gameObject.SetActive(true);
            showingControls = true;
            controlsDisplayTimer = 0f;
        }

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
        }

        Debug.Log("[UIManager] ShowGameOver called");
        Debug.Log($"[UIManager] passedObstaclesList is null: {passedObstaclesList == null}");
        Debug.Log($"[UIManager] PassedObstaclesTracker.Instance is null: {PassedObstaclesTracker.Instance == null}");

        if (passedObstaclesList != null && PassedObstaclesTracker.Instance != null)
        {
            List<string> passedObstacles = PassedObstaclesTracker.Instance.GetPassedObstacles();
            Debug.Log($"[UIManager] Passed obstacles count: {passedObstacles.Count}");
            passedObstaclesList.DisplayPassedObstacles(passedObstacles);
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
                victoryMessageText.text = "Поздравляем!\nВы вывезли 2025ый год! Готовы к следующему?";
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
