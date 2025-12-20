using UnityEngine;

[CreateAssetMenu(fileName = "New Obstacle", menuName = "Dead Moroz/Obstacle Data")]
public class ObstacleData : ScriptableObject
{
    [Header("Obstacle Settings")]
    public string obstacleName;
    public GameObject obstaclePrefab;
    public int laneIndex;
    public float spawnDistance;

    [Header("Visual")]
    [TextArea(2, 4)]
    public string obstacleText;
}
