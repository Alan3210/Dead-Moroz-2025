using UnityEngine;

[CreateAssetMenu(fileName = "Month_Configuration", menuName = "Game/Month Configuration")]
public class MonthConfiguration : ScriptableObject
{
    [Header("Month Info")]
    public int monthNumber = 1;
    public string monthName = "январь";

    [Header("Obstacles")]
    public ObstacleData[] obstacles;

    [Header("Auto-Calculated Length (Read-Only)")]
    [SerializeField] private float calculatedLength;

    public float segmentLength
    {
        get { return CalculateSegmentLength(); }
    }

    private float CalculateSegmentLength()
    {
        if (obstacles == null || obstacles.Length == 0)
        {
            return 50f;
        }

        float maxDistance = 0f;
        foreach (ObstacleData obstacle in obstacles)
        {
            if (obstacle != null && obstacle.spawnDistance > maxDistance)
            {
                maxDistance = obstacle.spawnDistance;
            }
        }

        return maxDistance + 10f;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        calculatedLength = CalculateSegmentLength();
    }
#endif
}
