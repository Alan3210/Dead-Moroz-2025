using UnityEngine;
using System.Collections.Generic;

public class RoadSpawner : MonoBehaviour
{
    [Header("Road Settings")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject snowPrefab;
    [SerializeField] private float snowSideOffset = 6f;
    [SerializeField] private float snowXOffset = 0f;
    [SerializeField] private float snowYOffset = -0.1f;
    [SerializeField] private float snowZOffset = 0f;
    [SerializeField] private float snowLengthOffset = 0f;
    [SerializeField] private int initialRoadCount = 5;
    [SerializeField] private float roadLength = 6.65f;

    [Header("Spawning")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnDistanceAhead = 30f;
    [SerializeField] private float despawnDistanceBehind = 20f;

    private Queue<GameObject> activeRoadSegments = new Queue<GameObject>();
    private Queue<GameObject> activeSnowSegments = new Queue<GameObject>();
    private float nextSpawnZ = 0f;
    private float nextSnowSpawnZ = 0f;

    void Start()
    {
        nextSnowSpawnZ = 0f;
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
            SpawnSnowSegment();
        }
    }


    void SpawnRoadSegment()
    {
        Vector3 spawnPosition = new Vector3(0, 0, nextSpawnZ);
        GameObject road = Instantiate(roadPrefab, spawnPosition, Quaternion.identity, transform);
        activeRoadSegments.Enqueue(road);
        nextSpawnZ += roadLength;
    }

    void SpawnSnowSegment()
    {
        Quaternion snowRotation = Quaternion.Euler(0f, -90f, 0f);

        Vector3 leftSpawnPosition = new Vector3(-snowSideOffset + snowXOffset, snowYOffset, nextSnowSpawnZ);
        GameObject leftSnow = Instantiate(snowPrefab, leftSpawnPosition, snowRotation, transform);
        leftSnow.transform.position += leftSnow.transform.forward * snowZOffset;
        activeSnowSegments.Enqueue(leftSnow);

        Vector3 rightSpawnPosition = new Vector3(snowSideOffset + snowXOffset, snowYOffset, nextSnowSpawnZ);
        GameObject rightSnow = Instantiate(snowPrefab, rightSpawnPosition, snowRotation, transform);
        rightSnow.transform.position += rightSnow.transform.forward * snowZOffset;
        activeSnowSegments.Enqueue(rightSnow);

        nextSnowSpawnZ += roadLength + snowLengthOffset;
    }

    void ManageRoadSegments()
    {
        if (playerTransform == null) return;

        if (playerTransform.position.z + spawnDistanceAhead > nextSpawnZ)
        {
            SpawnRoadSegment();
            SpawnSnowSegment();
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

        if (activeSnowSegments.Count > 0)
        {
            GameObject oldestSnow = activeSnowSegments.Peek();
            if (oldestSnow.transform.position.z < playerTransform.position.z - despawnDistanceBehind)
            {
                activeSnowSegments.Dequeue();
                Destroy(oldestSnow);
            }
        }
    }

}
