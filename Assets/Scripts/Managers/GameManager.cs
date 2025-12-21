using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isGameActive = false;

    [Header("Audio")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float speedIncreasePerMonth = 2f;
    [SerializeField] private int currentMonth = 1;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MonthVisualManager monthVisualManager;

    public float CurrentSpeed { get; private set; }
    public int CurrentMonth => currentMonth;
    public bool IsGameOver => isGameOver;
    public bool IsGameActive => isGameActive;

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
        InitializeGame();
    }

    private void InitializeGame()
    {
        isGameActive = false;
        isGameOver = false;
        currentMonth = 1;
        UpdateSpeed();

        if (playerController != null)
        {
            playerController.StopPlayer();
        }

        if (monthVisualManager != null)
        {
            monthVisualManager.ApplyMonthVisuals(0, false);
        }
    }



    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        currentMonth = 1;
        UpdateSpeed();
        ApplySpeedToPlayer();

        if (playerController != null)
        {
            playerController.ResumePlayer();
        }

        if (monthVisualManager != null)
        {
            monthVisualManager.ApplyMonthVisuals(0, false);
        }

        // Add this line:
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusic);
        }
    }



    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isGameActive = false;

        if (playerController != null)
        {
            playerController.StopPlayer();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver(currentMonth);
        }
    }


    public void AdvanceMonth()
    {
        if (currentMonth >= 12)
        {
            return;
        }

        currentMonth++;
        UpdateSpeed();
        ApplySpeedToPlayer();

        if (monthVisualManager != null)
        {
            monthVisualManager.ApplyMonthVisuals(currentMonth - 1, true);
        }

        if (UIManager.Instance != null)
        {
            string monthName = GetMonthName(currentMonth);
            UIManager.Instance.ShowMonthTransition(currentMonth, monthName);
        }

        // Add this line:
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMonthTransitionSound();
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


    private void UpdateSpeed()
    {
        CurrentSpeed = baseSpeed + (speedIncreasePerMonth * (currentMonth - 1));
    }

    private void ApplySpeedToPlayer()
    {
        if (playerController != null)
        {
            playerController.SetSpeed(CurrentSpeed);
        }
    }

    public void TriggerVictory()
    {
        isGameActive = false;

        if (playerController != null)
        {
            playerController.StopPlayer();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory();
        }
    }

}
