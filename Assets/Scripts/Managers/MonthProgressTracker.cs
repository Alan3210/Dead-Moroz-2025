using UnityEngine;

public class MonthProgressTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private LevelBuilder levelBuilder;

    private int currentDisplayedMonth = 1;
    private float[] monthTransitionPoints;
    private float[] monthEndPoints;
    private bool[] monthEntered;
    private bool[] monthCompleted;
    private MonthConfiguration[] monthConfigurations;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject sleigh = GameObject.Find("Sleigh");
            if (sleigh != null)
            {
                playerTransform = sleigh.transform;
            }
        }

        if (levelBuilder == null)
        {
            levelBuilder = Object.FindFirstObjectByType<LevelBuilder>();
            if (levelBuilder == null)
            {
                Debug.LogError("MonthProgressTracker: LevelBuilder not found in scene!");
                return;
            }
        }

        monthConfigurations = levelBuilder.MonthConfigurations;

        if (monthConfigurations == null || monthConfigurations.Length == 0)
        {
            Debug.LogError("MonthProgressTracker: No month configurations found in LevelBuilder!");
            return;
        }

        CalculateTransitionPoints();

        Debug.Log($"[MonthProgressTracker] Initialized with Month 1 displayed");
    }

    void CalculateTransitionPoints()
    {
        float startSafeZone = levelBuilder.StartSafeZoneDistance;
        float bufferZone = levelBuilder.MonthBufferZone;

        monthTransitionPoints = new float[monthConfigurations.Length];
        monthEndPoints = new float[monthConfigurations.Length];
        monthEntered = new bool[monthConfigurations.Length];
        monthCompleted = new bool[monthConfigurations.Length];

        monthEntered[0] = true;
        monthCompleted[0] = false;

        float currentMonthStartZ = startSafeZone;

        for (int i = 0; i < monthConfigurations.Length; i++)
        {
            MonthConfiguration currentMonth = monthConfigurations[i];

            if (currentMonth == null)
            {
                Debug.LogWarning($"Month {i + 1} configuration is null!");
                continue;
            }

            float lastObstacleZ = GetLastObstacleZ(currentMonth, currentMonthStartZ);
            monthEndPoints[i] = lastObstacleZ + 10f;

            if (i < monthConfigurations.Length - 1)
            {
                float nextMonthStartZ = currentMonthStartZ + currentMonth.segmentLength + bufferZone;
                MonthConfiguration nextMonth = monthConfigurations[i + 1];

                float firstNextObstacleZ = GetFirstObstacleZ(nextMonth, nextMonthStartZ);

                monthTransitionPoints[i + 1] = (lastObstacleZ + firstNextObstacleZ) / 2f;
                monthEntered[i + 1] = false;
                monthCompleted[i + 1] = false;

                Debug.Log($"[MonthProgressTracker] Month {i + 1}: End at Z={monthEndPoints[i]:F1}, Transition to {i + 2} at Z={monthTransitionPoints[i + 1]:F1}");

                currentMonthStartZ = nextMonthStartZ;
            }
        }
    }

    float GetLastObstacleZ(MonthConfiguration month, float monthStartZ)
    {
        if (month.obstacles == null || month.obstacles.Length == 0)
        {
            return monthStartZ + month.segmentLength;
        }

        float maxDistance = 0f;
        foreach (ObstacleData obstacle in month.obstacles)
        {
            if (obstacle != null && obstacle.spawnDistance > maxDistance)
            {
                maxDistance = obstacle.spawnDistance;
            }
        }

        return monthStartZ + maxDistance;
    }

    float GetFirstObstacleZ(MonthConfiguration month, float monthStartZ)
    {
        if (month.obstacles == null || month.obstacles.Length == 0)
        {
            return monthStartZ;
        }

        float minDistance = float.MaxValue;
        foreach (ObstacleData obstacle in month.obstacles)
        {
            if (obstacle != null && obstacle.spawnDistance < minDistance)
            {
                minDistance = obstacle.spawnDistance;
            }
        }

        return monthStartZ + minDistance;
    }

    void Update()
    {
        if (playerTransform == null || monthTransitionPoints == null) return;

        float playerZ = playerTransform.position.z;

        for (int i = 0; i < monthConfigurations.Length; i++)
        {
            if (!monthCompleted[i] && playerZ >= monthEndPoints[i])
            {
                CompleteMonth(i + 1);
                monthCompleted[i] = true;
            }
        }

        for (int i = 1; i < monthTransitionPoints.Length; i++)
        {
            if (!monthEntered[i] && playerZ >= monthTransitionPoints[i])
            {
                EnterNewMonth(i + 1);
                monthEntered[i] = true;
            }
        }
    }

    void CompleteMonth(int monthNumber)
    {
        Debug.Log($"📅 Month {monthNumber} completed! Showing banking screen...");

        if (EconomicManager.Instance != null)
        {
            EconomicManager.Instance.ShowMonthEndScreen();
        }
        else
        {
            Debug.LogError("EconomicManager not found!");
        }
    }

    void EnterNewMonth(int newMonthNumber)
    {
        if (newMonthNumber <= currentDisplayedMonth) return;
        if (newMonthNumber > monthConfigurations.Length) return;

        currentDisplayedMonth = newMonthNumber;

        MonthConfiguration month = monthConfigurations[newMonthNumber - 1];
        if (month == null) return;

        Debug.Log($"[MonthProgressTracker] ★ MONTH CHANGE: Now displaying {month.monthName} (Month {newMonthNumber}) at player Z = {playerTransform.position.z:F1}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdvanceMonth();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMonthTransitionSound();
        }
    }

    public int GetCurrentDisplayedMonth()
    {
        return currentDisplayedMonth;
    }
}
