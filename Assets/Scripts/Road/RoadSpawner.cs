using UnityEngine;
using System.Collections.Generic;

public class RoadSpawner : MonoBehaviour
{
    [Header("Road Settings")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private int initialRoadCount = 5;
    [SerializeField] private float roadLength = 6.65f;

    [Header("Spawning")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnDistanceAhead = 30f;
    [SerializeField] private float despawnDistanceBehind = 20f;

    private Queue<GameObject> activeRoadSegments = new Queue<GameObject>();
    private float nextSpawnZ = 0f;

    void Start()
    {
        SpawnInitialRoads();
    }

    void Update()
    {
        ManageRoadSegments();
    }

    void SpawnInitialRoads()
    {
        for (int i = 0; i < initialRoadCount; i++)
        {
            SpawnRoadSegment();
        }
    }

    void SpawnRoadSegment()
    {
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        GameObject road = Instantiate(roadPrefab, spawnPosition, Quaternion.identity, transform);
        activeRoadSegments.Enqueue(road);
        nextSpawnZ += roadLength;
    }

    void ManageRoadSegments()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.z + spawnDistanceAhead > nextSpawnZ)
        {
            SpawnRoadSegment();
        }

        if (activeRoadSegments.Count > 0)
        {
            GameObject oldestRoad = activeRoadSegments.Peek();
            if (oldestRoad.transform.position.z < playerTransform.position.z - despawnDistanceBehind)
            {
                activeRoadSegments.Dequeue();
                Destroy(oldestRoad);
            }
        }
    }
}
