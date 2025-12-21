using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnDistanceAhead = 50f;
    [SerializeField] private float despawnDistanceBehind = 30f;

    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2f;

    [Header("Month Configurations")]
    [SerializeField] private MonthConfiguration[] monthConfigurations;

    [Header("Month Transition")]
    [SerializeField] private float monthTransitionOverlap = 20f;

    private Queue<GameObject> activeObstacles = new Queue<GameObject>();
    private int currentMonthIndex = 0;
    private float currentMonthStartZ = 0f;
    private List<ObstacleData> pendingObstacles = new List<ObstacleData>();
    private bool monthTransitionQueued = false;

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("ObstacleSpawner: Player Transform is not assigned!");
            return;
        }

        if (monthConfigurations == null || monthConfigurations.Length == 0)
        {
            Debug.LogError("ObstacleSpawner: No month configurations assigned!");
            return;
        }

        currentMonthStartZ = playerTransform.position.z;
        LoadCurrentMonth();
    }

    void Update()
    {
        if (playerTransform == null) return;

        SpawnPendingObstacles();
        DespawnPassedObstacles();
        CheckMonthTransition();
    }

    void LoadCurrentMonth()
    {
        if (currentMonthIndex >= monthConfigurations.Length)
        {
            Debug.Log("ObstacleSpawner: All months completed!");
            return;
        }

        MonthConfiguration currentMonth = monthConfigurations[currentMonthIndex];

        if (currentMonth == null)
        {
            Debug.LogError($"ObstacleSpawner: Month configuration at index {currentMonthIndex} is null! Skipping this month.");
            currentMonthIndex++;
            if (currentMonthIndex < monthConfigurations.Length)
            {
                LoadCurrentMonth();
            }
            return;
        }

        pendingObstacles.Clear();

        if (currentMonth.obstacles != null)
        {
            foreach (ObstacleData obstacleData in currentMonth.obstacles)
            {
                if (obstacleData != null)
                {
                    pendingObstacles.Add(obstacleData);
                }
                else
                {
                    Debug.LogWarning($"ObstacleSpawner: Null obstacle data found in {currentMonth.monthName}");
                }
            }
        }

        monthTransitionQueued = false;
        Debug.Log($"Loaded Month {currentMonth.monthNumber}: {currentMonth.monthName} with {pendingObstacles.Count} obstacles at Z = {currentMonthStartZ}");
    }

    void SpawnPendingObstacles()
    {
        for (int i = pendingObstacles.Count - 1; i >= 0; i--)
        {
            ObstacleData data = pendingObstacles[i];

            if (data == null || data.obstaclePrefab == null)
            {
                Debug.LogWarning("ObstacleSpawner: Obstacle data or prefab is null. Skipping.");
                pendingObstacles.RemoveAt(i);
                continue;
            }

            float spawnZ = currentMonthStartZ + data.spawnDistance;

            if (playerTransform.position.z + spawnDistanceAhead >= spawnZ)
            {
                SpawnObstacle(data, spawnZ);
                pendingObstacles.RemoveAt(i);
            }
        }
    }

    void CheckMonthTransition()
    {
        if (monthTransitionQueued) return;
        if (pendingObstacles.Count > 0) return;

        if (ShouldAdvanceToNextMonth())
        {
            monthTransitionQueued = true;
            AdvanceToNextMonth();
        }
    }

    void SpawnObstacle(ObstacleData data, float zPosition)
    {
        float xPosition = (data.laneIndex - 1) * laneDistance;
        Vector3 spawnPosition = new Vector3(xPosition, 0f, zPosition);

        GameObject obstacle = Instantiate(data.obstaclePrefab, spawnPosition, Quaternion.identity, transform);

        ObstacleTextDisplay textDisplay = obstacle.GetComponent<ObstacleTextDisplay>();
        if (textDisplay != null && !string.IsNullOrEmpty(data.obstacleText))
        {
            textDisplay.SetText(data.obstacleText);
        }

        activeObstacles.Enqueue(obstacle);
    }


    void DespawnPassedObstacles()
    {
        while (activeObstacles.Count > 0)
        {
            GameObject obstacle = activeObstacles.Peek();

            if (obstacle == null || obstacle.transform.position.z < playerTransform.position.z - despawnDistanceBehind)
            {
                activeObstacles.Dequeue();
                if (obstacle != null)
                {
                    Destroy(obstacle);
                }
            }
            else
            {
                break;
            }
        }
    }

    bool ShouldAdvanceToNextMonth()
    {
        if (currentMonthIndex >= monthConfigurations.Length) return false;

        MonthConfiguration currentMonth = monthConfigurations[currentMonthIndex];

        if (currentMonth == null) return true;

        float monthEndZ = currentMonthStartZ + currentMonth.segmentLength;
        float transitionPoint = monthEndZ - monthTransitionOverlap;

        return playerTransform.position.z >= transitionPoint;
    }

    void AdvanceToNextMonth()
    {
        if (currentMonthIndex >= monthConfigurations.Length) return;

        MonthConfiguration completedMonth = monthConfigurations[currentMonthIndex];

        if (completedMonth != null)
        {
            currentMonthStartZ += completedMonth.segmentLength;
        }

        currentMonthIndex++;

        if (currentMonthIndex < monthConfigurations.Length)
        {
            LoadCurrentMonth();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AdvanceMonth();
            }
        }
        else
        {
            Debug.Log("ObstacleSpawner: All months completed! Game finished.");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }
        }
    }


    public int GetCurrentMonth()
    {
        return currentMonthIndex + 1;
    }
}
