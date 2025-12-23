using System.Collections.Generic;
using UnityEngine;

public class PassedObstaclesTracker : MonoBehaviour
{
    public static PassedObstaclesTracker Instance { get; private set; }

    private List<string> passedObstacles = new List<string>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[PassedObstaclesTracker] Instance created and ready");
    }

    public void RecordPassedObstacle(string obstacleText)
    {
        if (!string.IsNullOrEmpty(obstacleText))
        {
            passedObstacles.Add(obstacleText);
            Debug.Log($"[PassedObstaclesTracker] ✓ RECORDED: '{obstacleText}' (Total: {passedObstacles.Count})");
        }
        else
        {
            Debug.LogWarning("[PassedObstaclesTracker] ✗ REJECTED: Text was null or empty");
        }
    }

    public List<string> GetPassedObstacles()
    {
        Debug.Log($"[PassedObstaclesTracker] GetPassedObstacles called - Returning {passedObstacles.Count} obstacles");
        return new List<string>(passedObstacles);
    }

    public int GetPassedObstacleCount()
    {
        return passedObstacles.Count;
    }

    public void ClearPassedObstacles()
    {
        int count = passedObstacles.Count;
        passedObstacles.Clear();
        Debug.Log($"[PassedObstaclesTracker] CLEARED: Removed {count} obstacles");
    }
}
