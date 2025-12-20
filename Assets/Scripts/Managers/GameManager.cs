using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private bool isGameActive = false;

    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float speedIncreasePerMonth = 2f;
    [SerializeField] private int currentMonth = 1;

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
        StartGame();
    }

    public void StartGame()
    {
        isGameActive = true;
        isGameOver = false;
        currentMonth = 1;
        UpdateSpeed();
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isGameActive = false;
        Time.timeScale = 0f;

        Debug.Log("Game Over!");
    }

    public void AdvanceMonth()
    {
        if (currentMonth >= 12)
        {
            TriggerVictory();
            return;
        }

        currentMonth++;
        UpdateSpeed();
        Debug.Log($"Advanced to Month {currentMonth}");
    }

    private void UpdateSpeed()
    {
        CurrentSpeed = baseSpeed + (speedIncreasePerMonth * (currentMonth - 1));
    }

    private void TriggerVictory()
    {
        isGameActive = false;
        Debug.Log("Victory! Completed all 12 months!");
    }
}
