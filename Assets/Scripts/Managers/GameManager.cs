using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private LevelIntroAnimator levelIntroAnimator;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private TVController tvController;

    private const string HIGH_SCORE_KEY = "HighScore";
    private int highScore = 0;

    public float CurrentSpeed { get; private set; }
    public int CurrentMonth => currentMonth;
    public int HighScore => highScore;

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

    void Update()
    {
        // DEBUG: Press V to trigger victory
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            TriggerVictory();
        }
    }
    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        if (levelIntroAnimator == null)
        {
            levelIntroAnimator = FindFirstObjectByType<LevelIntroAnimator>();
        }

        isGameActive = false;
        isGameOver = false;
        currentMonth = 1;
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateSpeed();

        if (playerController != null)
        {
            playerController.StopPlayer();
        }

        if (tvController != null)
        {
            tvController.DeactivateTV();
        }

        if (monthVisualManager != null)
        {
            //monthVisualManager.ApplyMonthVisuals(0, false);
        }
    }



    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        currentMonth = 1;
        UpdateSpeed();
        ApplySpeedToPlayer();

        // ADD THIS:
        if (EconomicManager.Instance != null)
        {
            EconomicManager.Instance.ResetEconomy(); // We'll create this method
        }

        if (MonthlyMoneyManager.Instance != null)
        {
            MonthlyMoneyManager.Instance.ActivateMonthMoney(currentMonth);
        }

        if (PassedObstaclesTracker.Instance != null)
        {
            PassedObstaclesTracker.Instance.ClearPassedObstacles();
        }

        if (playerController != null)
        {
            playerController.ResumePlayer();
        }

        if (cameraFollow != null)
        {
            cameraFollow.StartFollowing();
        }

        if (tvController != null)
        {
            tvController.ActivateTV();
        }

        if (monthVisualManager != null)
        {
            monthVisualManager.ApplyMonthVisuals(0, false);
        }

        if (levelIntroAnimator != null)
        {
            levelIntroAnimator.Play();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(backgroundMusic);
        }
    }



    public void TriggerGameOver()
    {
        if (currentMonth > highScore)
        {
            highScore = currentMonth;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
        }

        if (isGameOver) return;

        isGameOver = true;
        isGameActive = false;

        RecordAllPassedObstacles();

        if (playerController != null)
        {
            playerController.StopPlayer();
        }


        if (playerController != null)
        {
            playerController.StopPlayer();
        }

        if (cameraFollow != null)
        {
            cameraFollow.StopFollowing();
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

        if (MonthlyMoneyManager.Instance != null)
        {
            MonthlyMoneyManager.Instance.ActivateMonthMoney(currentMonth);
        }

        if (monthVisualManager != null)
        {
            //monthVisualManager.ApplyMonthVisuals(currentMonth - 1, true);
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
    private void RecordAllPassedObstacles()
    {
        Debug.Log("[GameManager] === RecordAllPassedObstacles CALLED ===");

        if (PassedObstaclesTracker.Instance == null)
        {
            Debug.LogWarning("[GameManager] ❌ PassedObstaclesTracker.Instance is NULL!");
            return;
        }

        if (playerController == null)
        {
            Debug.LogWarning("[GameManager] ❌ playerController is NULL!");
            return;
        }

        Debug.Log($"[GameManager] Player position: {playerController.transform.position}");

        GameObject[] allObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        Debug.Log($"[GameManager] Found {allObstacles.Length} obstacles with 'Obstacle' tag");

        if (allObstacles.Length == 0)
        {
            Debug.LogWarning("[GameManager] ❌ No obstacles found! Check if obstacles have 'Obstacle' tag");
            return;
        }

        int recordedCount = 0;
        int skippedCount = 0;

        foreach (GameObject obstacleObj in allObstacles)
        {
            if (obstacleObj == null) continue;

            float distanceBehindPlayer = playerController.transform.position.z - obstacleObj.transform.position.z;

            Debug.Log($"[GameManager] Checking obstacle: {obstacleObj.name} at Z={obstacleObj.transform.position.z:F1}, distance behind player: {distanceBehindPlayer:F1}");

            if (distanceBehindPlayer > 0f)
            {
                ObstacleTextDisplay textDisplay = obstacleObj.GetComponent<ObstacleTextDisplay>();

                if (textDisplay == null)
                {
                    Debug.LogWarning($"[GameManager] ⚠️ Obstacle {obstacleObj.name} has no ObstacleTextDisplay component!");
                    skippedCount++;
                    continue;
                }

                if (textDisplay.hasBeenRecorded)
                {
                    Debug.Log($"[GameManager] ⏭️ Obstacle {obstacleObj.name} already recorded, skipping");
                    skippedCount++;
                    continue;
                }

                TextMeshProUGUI tmpText = textDisplay.GetComponentInChildren<TextMeshProUGUI>();

                if (tmpText == null)
                {
                    Debug.LogWarning($"[GameManager] ⚠️ Obstacle {obstacleObj.name} has no TextMeshProUGUI!");
                    skippedCount++;
                    continue;
                }

                if (string.IsNullOrEmpty(tmpText.text) || tmpText.text == "Loading...")
                {
                    Debug.LogWarning($"[GameManager] ⚠️ Obstacle {obstacleObj.name} has invalid text: '{tmpText.text}'");
                    skippedCount++;
                    continue;
                }

                Debug.Log($"[GameManager] ✅ Recording passed obstacle: '{tmpText.text}' (distance behind: {distanceBehindPlayer:F1})");
                PassedObstaclesTracker.Instance.RecordPassedObstacle(tmpText.text);
                textDisplay.hasBeenRecorded = true;
                recordedCount++;
            }
            else
            {
                Debug.Log($"[GameManager] ⏩ Obstacle {obstacleObj.name} is ahead of player, skipping");
                skippedCount++;
            }
        }

        Debug.Log($"[GameManager] === FINISHED: Recorded {recordedCount} new obstacles, Skipped {skippedCount} obstacles ===");
    }



}
