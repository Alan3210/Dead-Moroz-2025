using UnityEngine;

[CreateAssetMenu(fileName = "New Money Spawn", menuName = "Dead Moroz/Money Spawn Data")]
public class MoneySpawnData : ScriptableObject
{
    [Header("Spawn Settings")]
    public int laneIndex;
    public float spawnDistance;

    [Header("Value Settings")]
    [Tooltip("Ruble value of this money bundle (individual values for balancing)")]
    public float rubleValue = 100f;

    [Header("Prefab Reference")]
    public GameObject moneyPrefab;
}
