using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Mobile Controls")]
    [SerializeField] private MobileInputButtons mobileInputButtons;

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject gameplayHUDPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;

    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI monthText;
    [Header("Controls Text Settings")]
    [Tooltip("The GameObject containing the controls instructions (e.g., 'ControlsText').")]
    [SerializeField] private GameObject controlsTextObject;
    [Tooltip("How long to wait after game start before showing the controls text.")]
    [SerializeField] private float controlsShowDelay = 2f;
    [Tooltip("How long the controls text remains visible on screen.")]
    [SerializeField] private float controlsDisplayDuration = 5f;
    [Tooltip("How long the fade in/out animation takes.")]
    [SerializeField] private float controlsFadeDuration = 0.5f;
    [Tooltip("If true, controls text will show on all platforms (including mobile).")]
    [SerializeField] private bool showControlsInAllPlatforms = false;

    [Header("Main Menu Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button creditsButton;

    [Header("Credits Elements")]
    [SerializeField] private Button backFromCreditsButton;

    [Header("Pause Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartFromPauseButton;
    [SerializeField] private Button quitFromPauseButton;

    [Header("Game Over Elements")]
    [SerializeField] private TextMeshProUGUI gameOverMonthText;
    [SerializeField] private TextMeshProUGUI monthReachedText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button restartFromGameOverButton;
    [SerializeField] private Button quitFromGameOverButton;
    [SerializeField] private PassedObstaclesList passedObstaclesList;

    [Header("Victory Elements")]
    [SerializeField] private TextMeshProUGUI victoryMessageText;
    [SerializeField] private TextMeshProUGUI finalStatsText;
    [SerializeField] private Button restartFromVictoryButton;
    [SerializeField] private Button quitFromVictoryButton;

    [Header("Month Transition Elements")]
    [SerializeField] private MonthTransitionEffect monthTransitionEffect;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSFX;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private AudioClip gameOverSFX;
    [SerializeField] private AudioClip victorySFX;

    private bool isPaused = false;
    private Keyboard keyboard;
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
        if (showingControls && controlsTextObject != null && controlsTextObject.activeSelf)
        {
            controlsDisplayTimer += Time.deltaTime;

            if (controlsDisplayTimer >= controlsDisplayDuration)
            {
                StartCoroutine(FadeOutControls());
                showingControls = false;
            }
        }
    }

    [ContextMenu("Test Passed Obstacles List")]
    private void TestPassedObstaclesList()
    {
        List<string> testObstacles = new List<string>
    {
        "Тест препятствие 1",
        "Тест препятствие 2",
        "Тест препятствие 3",
        "Тест препятствие 4",
        "Тест препятствие 5",
        "Тест препятствие 6",
        "Тест препятствие 7",
        "Тест препятствие 8",
        "Тест препятствие 9",
        "Тест препятствие 10"
    };

        if (passedObstaclesList != null)
        {
            passedObstaclesList.DisplayPassedObstacles(testObstacles);
        }
    }


    private void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(OnShowCredits);

        if (backFromCreditsButton != null)
            backFromCreditsButton.onClick.AddListener(OnBackToMainMenu);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeGame);

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

        if (mobileInputButtons != null)
        {
            mobileInputButtons.HideControls();
        }


        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (controlsTextObject != null)
            controlsTextObject.SetActive(false);

        showingControls = false;
        controlsDisplayTimer = 0f;

        Time.timeScale = 1f;
        isPaused = false;

        // Play main menu music
        if (AudioManager.Instance != null && mainMenuMusic != null)
        {
            AudioManager.Instance.PlayMusic(mainMenuMusic);
        }
    }


    public void ShowGameplayHUD()
    {
        HideAllPanels();
        if (gameplayHUDPanel != null)
            gameplayHUDPanel.SetActive(true);

        if (controlsTextObject != null)
        {
            controlsTextObject.SetActive(false); // Ensure it's off initially
            
            // Reset Alpha if CanvasGroup exists
            CanvasGroup cg = controlsTextObject.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 0f;

            // Show controls text based on platform or override
            bool shouldShow = showControlsInAllPlatforms;
            #if UNITY_WEBGL || UNITY_STANDALONE
            shouldShow = true;
            #endif

            if (shouldShow)
            {
                StartCoroutine(ShowControlsDelayed());
            }
        }

        Time.timeScale = 1f;
        isPaused = false;

        if (mobileInputButtons != null)
        {
            mobileInputButtons.ShowControls();
        }

        // Music change is handled by GameManager.StartGame()
    }

    private System.Collections.IEnumerator ShowControlsDelayed()
    {
        yield return new WaitForSeconds(controlsShowDelay);
        
        if (controlsTextObject != null)
        {
            controlsTextObject.SetActive(true);
            CanvasGroup cg = controlsTextObject.GetComponent<CanvasGroup>();
            
            // Fade In
            if (cg != null)
            {
                float timer = 0f;
                while (timer < controlsFadeDuration)
                {
                    timer += Time.deltaTime;
                    cg.alpha = Mathf.Lerp(0f, 1f, timer / controlsFadeDuration);
                    yield return null;
                }
                cg.alpha = 1f;
            }

            showingControls = true;
            controlsDisplayTimer = 0f;
        }
    }

    private System.Collections.IEnumerator FadeOutControls()
    {
        if (controlsTextObject == null) yield break;

        CanvasGroup cg = controlsTextObject.GetComponent<CanvasGroup>();
        
        if (cg != null)
        {
            float timer = 0f;
            float startAlpha = cg.alpha;

            while (timer < controlsFadeDuration)
            {
                timer += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, 0f, timer / controlsFadeDuration);
                yield return null;
            }
            cg.alpha = 0f;
        }

        if (controlsTextObject != null)
        {
            controlsTextObject.SetActive(false);
        }
    }

    public void ShowPauseMenu()
    {
        if (mobileInputButtons != null)
        {
            mobileInputButtons.HideControls();
        }


        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ShowGameOver(int monthReached)
    {
        if (mobileInputButtons != null)
        {
            mobileInputButtons.HideControls();
        }


        HideAllPanels();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Update month reached text
        if (monthReachedText != null)
        {
            string monthName = GetMonthName(monthReached);
            monthReachedText.text = $"Вы дожили до: {monthName.ToUpper()}";
        }

        // Update high score text
        if (highScoreText != null)
        {
            int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;
            bool isNewRecord = monthReached > highScore;

            if (isNewRecord)
            {
                highScoreText.text = $"🏆 НОВЫЙ РЕКОРД: {monthReached} месяцев!";
                highScoreText.color = Color.yellow;
            }
            else
            {
                highScoreText.text = $"Рекорд: {highScore} месяцев";
                highScoreText.color = Color.white;
            }
        }

        // Display passed obstacles
        if (passedObstaclesList != null && PassedObstaclesTracker.Instance != null)
        {
            List<string> passedObstacles = PassedObstaclesTracker.Instance.GetPassedObstacles();
            passedObstaclesList.DisplayPassedObstacles(passedObstacles);
        }

        // Play game over music and SFX
        if (AudioManager.Instance != null)
        {
            if (gameOverSFX != null)
            {
                AudioManager.Instance.PlaySFX(gameOverSFX);
            }

            if (gameOverMusic != null)
            {
                AudioManager.Instance.PlayMusic(gameOverMusic);
            }
        }

        Time.timeScale = 0f;
    }

    public void ShowVictory()
    {
        if (mobileInputButtons != null)
        {
            mobileInputButtons.HideControls();
        }


        HideAllPanels();
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            // Update victory message
            if (victoryMessageText != null)
            {
                victoryMessageText.text = "Ну, вывез,\nполучается";
                // Reset alpha to 0 for fade in
                Color color = victoryMessageText.color;
                color.a = 0f;
                victoryMessageText.color = color;
            }

            // Calculate and display final statistics
            if (finalStatsText != null)
            {
                int totalObstacles = 0;
                float totalMoney = 0f;

                if (PassedObstaclesTracker.Instance != null)
                {
                    totalObstacles = PassedObstaclesTracker.Instance.GetPassedObstacles().Count;
                }

                if (EconomicManager.Instance != null)
                {
                    totalMoney = EconomicManager.Instance.EarnedMonthIncome;
                }

                finalStatsText.text = "До встречи в 2026м.\n" +
                                      "Утиль сбор на сани сам себя не оплатит";
                
                // Reset alpha to 0 for fade in
                Color color = finalStatsText.color;
                color.a = 0f;
                finalStatsText.color = color;
            }
            
            StartCoroutine(FadeInVictoryTexts(2f));
        }

        // Play victory music and SFX
        if (AudioManager.Instance != null)
        {
            if (victorySFX != null)
            {
                AudioManager.Instance.PlaySFX(victorySFX);
            }

            if (victoryMusic != null)
            {
                AudioManager.Instance.PlayMusic(victoryMusic);
            }
        }

        Time.timeScale = 0f;
    }

    private System.Collections.IEnumerator FadeInVictoryTexts(float duration)
    {
        float timer = 0f;
        Color victoryStartColor = victoryMessageText != null ? victoryMessageText.color : Color.white;
        Color statsStartColor = finalStatsText != null ? finalStatsText.color : Color.white;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(timer / duration);

            if (victoryMessageText != null)
            {
                Color c = victoryMessageText.color;
                c.a = alpha;
                victoryMessageText.color = c;
            }

            if (finalStatsText != null)
            {
                Color c = finalStatsText.color;
                c.a = alpha;
                finalStatsText.color = c;
            }

            yield return null;
        }

        // Ensure fully visible at the end
        if (victoryMessageText != null)
        {
            Color c = victoryMessageText.color;
            c.a = 1f;
            victoryMessageText.color = c;
        }
        if (finalStatsText != null)
        {
            Color c = finalStatsText.color;
            c.a = 1f;
            finalStatsText.color = c;
        }
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
        if (creditsPanel != null) creditsPanel.SetActive(false);
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

        if (mobileInputButtons != null)
        {
            mobileInputButtons.ShowControls();
        }

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

    private void OnShowCredits()
    {
        PlayButtonClickSound();
        ShowCredits();
    }

    private void OnBackToMainMenu()
    {
        PlayButtonClickSound();
        ShowMainMenu();
    }

    public void ShowCredits()
    {
        HideAllPanels();
        if (creditsPanel != null)
            creditsPanel.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;

        // Keep main menu music playing (don't stop it)
        // Music is already playing from ShowMainMenu()
    }
}