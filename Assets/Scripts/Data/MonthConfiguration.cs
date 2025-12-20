using UnityEngine;

[CreateAssetMenu(fileName = "Month_01", menuName = "Dead Moroz/Month Configuration")]
public class MonthConfiguration : ScriptableObject
{
    [Header("Month Info")]
    public int monthNumber = 1;
    public string monthName;

    [Header("Obstacles")]
    public ObstacleData[] obstacles;

    [Header("Segment Length")]
    public float segmentLength = 100f;
}
