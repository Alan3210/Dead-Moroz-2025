using UnityEngine;

[CreateAssetMenu(fileName = "Month_Configuration", menuName = "Game/Month Configuration")]
public class MonthConfiguration : ScriptableObject
{
    [Header("Month Info")]
    public int monthNumber = 1;
    public string monthName = "январь";

    [Header("Obstacles")]
    public ObstacleData[] obstacles;

    [Header("Money Collectibles")]
    [Tooltip("Money bundles scattered throughout this month")]
    public MoneySpawnData[] moneySpawns;

    [Header("Economic Info (Read-Only)")]
    [SerializeField] private float totalMoneyAvailable;

    [Header("Auto-Calculated Length (Read-Only)")]
    [SerializeField] private float calculatedLength;

    public float segmentLength
    {
        get { return CalculateSegmentLength(); }
    }

    public float TotalMoneyInMonth
    {
        get { return CalculateTotalMoney(); }
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

    private float CalculateTotalMoney()
    {
        if (moneySpawns == null || moneySpawns.Length == 0)
        {
            return 0f;
        }

        float total = 0f;
        foreach (MoneySpawnData money in moneySpawns)
        {
            if (money != null)
            {
                total += money.rubleValue;
            }
        }

        return total;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        calculatedLength = CalculateSegmentLength();
        totalMoneyAvailable = CalculateTotalMoney();
    }
#endif
}
