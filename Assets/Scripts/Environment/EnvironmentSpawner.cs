using UnityEngine;
using System.Collections.Generic;

public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Environment Prefabs")]
    [SerializeField] private GameObject pinePrefab;
    [SerializeField] private GameObject panelkaPrefab;

    [Header("Spawning Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnDistanceAhead = 50f;
    [SerializeField] private float despawnDistanceBehind = 30f;

    [Header("Placement Settings")]
    [SerializeField] private float pineRoadSideOffset = 5f;
    [SerializeField] private float panelkaRoadSideOffset = 8f;
    [SerializeField] private float minSpawnInterval = 8f;
    [SerializeField] private float maxSpawnInterval = 15f;
    [SerializeField] private float randomPositionVariation = 1.5f;

    [Header("Parallax Settings")]
    [SerializeField][Range(1f, 3f)] private float pineParallaxSpeed = 1.3f;
    [SerializeField][Range(1f, 3f)] private float panelkaParallaxSpeed = 1.5f;

    [Header("Pooling Settings")]
    [SerializeField] private int initialPoolSize = 20;

    private Queue<EnvironmentObject> activeLeftObjects = new Queue<EnvironmentObject>();
    private Queue<EnvironmentObject> activeRightObjects = new Queue<EnvironmentObject>();

    private List<GameObject> pinePool = new List<GameObject>();
    private List<GameObject> panelkaPool = new List<GameObject>();

    private float nextLeftPineSpawnZ = 0f;
    private float nextRightPineSpawnZ = 0f;
    private float nextLeftPanelkaSpawnZ = 0f;
    private float nextRightPanelkaSpawnZ = 0f;

    private class EnvironmentObject
    {
        public GameObject gameObject;
        public bool isLeftSide;

        public EnvironmentObject(GameObject obj, bool left)
        {
            gameObject = obj;
            isLeftSide = left;
        }
    }

    void Start()
    {
        if (playerTransform == null)
        {
            Debug.LogError("EnvironmentSpawner: Player Transform is not assigned!");
            return;
        }

        InitializePools();

        nextLeftPineSpawnZ = playerTransform.position.z + Random.Range(minSpawnInterval, maxSpawnInterval);
        nextRightPineSpawnZ = playerTransform.position.z + Random.Range(minSpawnInterval, maxSpawnInterval);
        nextLeftPanelkaSpawnZ = playerTransform.position.z + Random.Range(minSpawnInterval, maxSpawnInterval);
        nextRightPanelkaSpawnZ = playerTransform.position.z + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void Update()
    {
        if (playerTransform == null) return;

        CheckAndSpawnEnvironment();
        DespawnPassedObjects();
    }

    void InitializePools()
    {
        if (pinePrefab == null || panelkaPrefab == null)
        {
            Debug.LogError("EnvironmentSpawner: Pine or Panelka prefab is not assigned!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject pine = Instantiate(pinePrefab, Vector3.zero, Quaternion.identity, transform);
            EnsureParallaxComponent(pine, true);
            pine.SetActive(false);
            pinePool.Add(pine);

            GameObject panelka = Instantiate(panelkaPrefab, Vector3.zero, Quaternion.identity, transform);
            EnsureParallaxComponent(panelka, false);
            panelka.SetActive(false);
            panelkaPool.Add(panelka);
        }
    }

    void EnsureParallaxComponent(GameObject obj, bool isPine)
    {
        ParallaxObject parallax = obj.GetComponent<ParallaxObject>();
        if (parallax == null)
        {
            parallax = obj.AddComponent<ParallaxObject>();
        }
    }

    void CheckAndSpawnEnvironment()
    {
        float playerZ = playerTransform.position.z;

        if (playerZ + spawnDistanceAhead > nextLeftPineSpawnZ)
        {
            SpawnSpecificObject(true, true);
            nextLeftPineSpawnZ += Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        if (playerZ + spawnDistanceAhead > nextRightPineSpawnZ)
        {
            SpawnSpecificObject(false, true);
            nextRightPineSpawnZ += Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        if (playerZ + spawnDistanceAhead > nextLeftPanelkaSpawnZ)
        {
            SpawnSpecificObject(true, false);
            nextLeftPanelkaSpawnZ += Random.Range(minSpawnInterval, maxSpawnInterval);
        }

        if (playerZ + spawnDistanceAhead > nextRightPanelkaSpawnZ)
        {
            SpawnSpecificObject(false, false);
            nextRightPanelkaSpawnZ += Random.Range(minSpawnInterval, maxSpawnInterval);
        }
    }

    void SpawnSpecificObject(bool isLeftSide, bool isPine)
    {
        GameObject obj = GetPooledObject(isPine);

        if (obj == null)
        {
            obj = Instantiate(isPine ? pinePrefab : panelkaPrefab, transform);
            EnsureParallaxComponent(obj, isPine);
        }

        float roadOffset = isPine ? pineRoadSideOffset : panelkaRoadSideOffset;
        float xPosition = isLeftSide ? -roadOffset : roadOffset;

        float zPosition;
        if (isPine)
        {
            zPosition = isLeftSide ? nextLeftPineSpawnZ : nextRightPineSpawnZ;
        }
        else
        {
            zPosition = isLeftSide ? nextLeftPanelkaSpawnZ : nextRightPanelkaSpawnZ;
        }

        float zVariation = Random.Range(-randomPositionVariation, randomPositionVariation);
        zPosition += zVariation;

        obj.transform.position = new Vector3(xPosition, 0f, zPosition);

        if (isPine)
        {
            float yRotation = Random.Range(0f, 360f);
            obj.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
        else
        {
            float yRotation = isLeftSide ? 90f : -90f;
            obj.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        ParallaxObject parallax = obj.GetComponent<ParallaxObject>();
        if (parallax != null)
        {
            float speedMultiplier = isPine ? pineParallaxSpeed : panelkaParallaxSpeed;
            parallax.Initialize(playerTransform, speedMultiplier);
        }

        obj.SetActive(true);

        EnvironmentObject envObj = new EnvironmentObject(obj, isLeftSide);

        if (isLeftSide)
        {
            activeLeftObjects.Enqueue(envObj);
        }
        else
        {
            activeRightObjects.Enqueue(envObj);
        }
    }

    GameObject GetPooledObject(bool isPine)
    {
        List<GameObject> pool = isPine ? pinePool : panelkaPool;

        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }

        return null;
    }

    void DespawnPassedObjects()
    {
        float playerZ = playerTransform.position.z;

        DespawnSideObjects(activeLeftObjects, playerZ);
        DespawnSideObjects(activeRightObjects, playerZ);
    }

    void DespawnSideObjects(Queue<EnvironmentObject> objectQueue, float playerZ)
    {
        while (objectQueue.Count > 0)
        {
            EnvironmentObject envObj = objectQueue.Peek();

            if (envObj.gameObject == null || envObj.gameObject.transform.position.z < playerZ - despawnDistanceBehind)
            {
                objectQueue.Dequeue();

                if (envObj.gameObject != null)
                {
                    envObj.gameObject.SetActive(false);
                }
            }
            else
            {
                break;
            }
        }
    }
}
