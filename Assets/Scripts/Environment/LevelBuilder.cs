using UnityEngine;
using TMPro;

public class LevelBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private GameObject snowPrefab;
    [SerializeField] private GameObject pinePrefab;
    [SerializeField] private GameObject panelkaPrefab;

    [Header("Level Settings")]
    [SerializeField] private float totalLevelLength = 2500f;
    [SerializeField] private float startPosition = 0f;
    [SerializeField] private float startSafeZoneDistance = 80f;
    [SerializeField] private float monthBufferZone = 80f;

    [Header("Road Settings")]
    [SerializeField] private float roadSegmentLength = 6.65f;

    [Header("Snow Settings")]
    [SerializeField] private float snowYOffset = -0.5f;

    [Header("Environment Settings")]
    [SerializeField] private float pineRoadSideOffset = 4f;
    [SerializeField] private float panelkaRoadSideOffset = 9.8f;
    [SerializeField] private float minEnvironmentSpacing = 8f;
    [SerializeField] private float maxEnvironmentSpacing = 20f;
    [SerializeField] private float randomPositionVariation = 0.5f;

    [Header("Obstacle Settings")]
    [SerializeField] private MonthConfiguration[] monthConfigurations;
    [SerializeField] private float laneDistance = 1.25f;

    [Header("Parallax Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float pineParallaxSpeed = 1.1f;
    [SerializeField] private float panelkaParallaxSpeed = 1.3f;
    [SerializeField] private float snowParallaxSpeed = 1.1f;
    [SerializeField] private bool enableDynamicParallax = true;

    [Header("Parent Transforms")]
    [SerializeField] private Transform roadParent;
    [SerializeField] private Transform snowParent;
    [SerializeField] private Transform environmentParent;
    [SerializeField] private Transform obstaclesParent;
    [SerializeField] private Transform collectiblesParent;

    [Header("Animation")]
    [SerializeField] private LevelIntroAnimator introAnimator;

    public float StartSafeZoneDistance => startSafeZoneDistance;
    public float MonthBufferZone => monthBufferZone;
    public MonthConfiguration[] MonthConfigurations => monthConfigurations;

    // Public Getters for Intro Animator
    public Transform SnowParent => snowParent;
    public Transform EnvironmentParent => environmentParent;
    public Transform ObstaclesParent => obstaclesParent;
    public Transform CollectiblesParent => collectiblesParent;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject sleigh = GameObject.Find("Sleigh");
            if (sleigh != null)
            {
                playerTransform = sleigh.transform;
            }
        }

        BuildLevel();
    }

    [ContextMenu("Build Level")]
    public void BuildLevel()
    {
        Debug.Log("[LevelBuilder] BuildLevel called.");

        if (roadPrefab == null || snowPrefab == null || pinePrefab == null || panelkaPrefab == null)
        {
            Debug.LogError("LevelBuilder: Missing prefabs! Assign all prefabs before building.");
            return;
        }

        ClearLevel();
        CreateParents();

        BuildRoad();
        BuildSnow();
        BuildEnvironment();
        BuildObstacles();
        BuildMoneyCollectibles();
        BuildFinishLine();  // 🆕 ADD THIS LINE

        if (introAnimator == null)
        {
            introAnimator = GetComponent<LevelIntroAnimator>();
        }

        if (introAnimator != null)
        {
            Debug.Log("[LevelBuilder] Calling IntroAnimator.Prepare()...");
            introAnimator.Prepare(snowParent, environmentParent, obstaclesParent, collectiblesParent);
        }
        else
        {
            Debug.LogError("[LevelBuilder] IntroAnimator reference is missing and component not found!");
        }

        Debug.Log($"Level built! Total length: {totalLevelLength} units");
    }

    [ContextMenu("Clear Level")]
    public void ClearLevel()
    {
        if (roadParent != null)
        {
            DestroyImmediate(roadParent.gameObject);
        }
        if (snowParent != null)
        {
            DestroyImmediate(snowParent.gameObject);
        }
        if (environmentParent != null)
        {
            DestroyImmediate(environmentParent.gameObject);
        }
        if (obstaclesParent != null)
        {
            DestroyImmediate(obstaclesParent.gameObject);
        }
        if (collectiblesParent != null)
        {
            DestroyImmediate(collectiblesParent.gameObject);
        }

        CreateParents();
    }

    void CreateParents()
    {
        if (roadParent == null)
        {
            GameObject roadObj = new GameObject("Road");
            roadObj.transform.SetParent(transform);
            roadParent = roadObj.transform;
        }

        if (snowParent == null)
        {
            GameObject snowObj = new GameObject("Snow");
            snowObj.transform.SetParent(transform);
            snowParent = snowObj.transform;
        }

        if (environmentParent == null)
        {
            GameObject envObj = new GameObject("Environment");
            envObj.transform.SetParent(transform);
            environmentParent = envObj.transform;
        }

        if (obstaclesParent == null)
        {
            GameObject obsObj = new GameObject("Obstacles");
            obsObj.transform.SetParent(transform);
            obstaclesParent = obsObj.transform;
        }

        if (collectiblesParent == null)
        {
            GameObject collObj = new GameObject("Collectibles");
            collObj.transform.SetParent(transform);
            collectiblesParent = collObj.transform;
        }
    }

    void BuildRoad()
    {
        float currentZ = startPosition;
        int segmentCount = 0;

        while (currentZ < totalLevelLength)
        {
            Vector3 position = new Vector3(0f, 0f, currentZ);
            GameObject road = Instantiate(roadPrefab, position, Quaternion.identity, roadParent);
            road.name = $"Road_{segmentCount:D3}";

            currentZ += roadSegmentLength;
            segmentCount++;
        }

        Debug.Log($"Built {segmentCount} road segments");
    }

    void BuildSnow()
    {
        float currentZ = startPosition;
        int segmentCount = 0;

        while (currentZ < totalLevelLength)
        {
            Vector3 position = new Vector3(0f, snowYOffset, currentZ);
            GameObject snow = Instantiate(snowPrefab, position, Quaternion.identity, snowParent);
            snow.name = $"Snow_{segmentCount:D3}";

            ParallaxObject parallax = snow.GetComponent<ParallaxObject>();
            if (parallax == null)
            {
                parallax = snow.AddComponent<ParallaxObject>();
            }

            if (playerTransform != null)
            {
                parallax.Initialize(playerTransform, snowParallaxSpeed, enableDynamicParallax);
            }

            currentZ += roadSegmentLength;
            segmentCount++;
        }

        Debug.Log($"Built {segmentCount} snow segments");
    }

    void BuildEnvironment()
    {
        float leftPineZ = startPosition + Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);
        float rightPineZ = startPosition + Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);
        float leftPanelkaZ = startPosition + Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);
        float rightPanelkaZ = startPosition + Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);

        int pineCount = 0;
        int panelkaCount = 0;

        while (leftPineZ < totalLevelLength || rightPineZ < totalLevelLength ||
               leftPanelkaZ < totalLevelLength || rightPanelkaZ < totalLevelLength)
        {
            if (leftPineZ < totalLevelLength)
            {
                SpawnPine(true, leftPineZ, ref pineCount);
                leftPineZ += Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);
            }

            if (rightPineZ < totalLevelLength)
            {
                SpawnPine(false, rightPineZ, ref pineCount);
                rightPineZ += Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);
            }

            if (leftPanelkaZ < totalLevelLength)
            {
                SpawnPanelka(true, leftPanelkaZ, ref panelkaCount);
                leftPanelkaZ += Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);
            }

            if (rightPanelkaZ < totalLevelLength)
            {
                SpawnPanelka(false, rightPanelkaZ, ref panelkaCount);
                rightPanelkaZ += Random.Range(minEnvironmentSpacing, maxEnvironmentSpacing);
            }
        }

        Debug.Log($"Built {pineCount} pines and {panelkaCount} panelkas");
    }

    void BuildObstacles()
    {
        if (monthConfigurations == null || monthConfigurations.Length == 0)
        {
            Debug.LogWarning("LevelBuilder: No month configurations assigned. Skipping obstacle placement.");
            return;
        }

        float currentMonthStartZ = startPosition + startSafeZoneDistance;
        int totalObstacleCount = 0;

        for (int monthIndex = 0; monthIndex < monthConfigurations.Length; monthIndex++)
        {
            MonthConfiguration month = monthConfigurations[monthIndex];

            if (month == null)
            {
                Debug.LogWarning($"LevelBuilder: Month configuration at index {monthIndex} is null! Skipping.");
                continue;
            }

            GameObject monthParent = new GameObject($"Month_{month.monthNumber:D2}_{month.monthName}");
            monthParent.transform.SetParent(obstaclesParent);
            monthParent.transform.position = Vector3.zero;

            float monthEndZ = currentMonthStartZ + month.segmentLength;

            if (month.obstacles != null && month.obstacles.Length > 0)
            {
                foreach (ObstacleData obstacleData in month.obstacles)
                {
                    if (obstacleData == null || obstacleData.obstaclePrefab == null)
                    {
                        Debug.LogWarning($"LevelBuilder: Null obstacle data in {month.monthName}. Skipping.");
                        continue;
                    }

                    float obstacleZ = currentMonthStartZ + obstacleData.spawnDistance;
                    float obstacleX = (obstacleData.laneIndex - 1) * laneDistance;

                    Vector3 position = new Vector3(obstacleX, 0f, obstacleZ);
                    GameObject obstacle = Instantiate(obstacleData.obstaclePrefab, position, Quaternion.identity, monthParent.transform);
                    obstacle.name = $"Obstacle_{totalObstacleCount:D3}_{obstacleData.obstacleText}";

                    ObstacleTextDisplay textDisplay = obstacle.GetComponent<ObstacleTextDisplay>();
                    if (textDisplay != null && !string.IsNullOrEmpty(obstacleData.obstacleText))
                    {
                        textDisplay.SetText(obstacleData.obstacleText);
                    }

                    totalObstacleCount++;
                }

                Debug.Log($"Built {month.obstacles.Length} obstacles for Month {month.monthNumber} ({month.monthName}): Z={currentMonthStartZ:F1} to Z={monthEndZ:F1}");
            }

            currentMonthStartZ = monthEndZ + monthBufferZone;
        }

        Debug.Log($"Built {totalObstacleCount} total obstacles across {monthConfigurations.Length} months");
        Debug.Log($"Total level distance with buffers: {currentMonthStartZ:F1} units");
    }

    void BuildMoneyCollectibles()
    {
        if (monthConfigurations == null || monthConfigurations.Length == 0)
        {
            Debug.LogWarning("LevelBuilder: No month configurations assigned. Skipping money placement.");
            return;
        }

        float currentMonthStartZ = startPosition + startSafeZoneDistance;
        int totalMoneyCount = 0;
        float totalValuePlaced = 0f;

        for (int monthIndex = 0; monthIndex < monthConfigurations.Length; monthIndex++)
        {
            MonthConfiguration month = monthConfigurations[monthIndex];

            if (month == null)
            {
                Debug.LogWarning($"LevelBuilder: Month configuration at index {monthIndex} is null! Skipping.");
                continue;
            }

            GameObject monthMoneyParent = new GameObject($"Month_{month.monthNumber:D2}_{month.monthName}_Money");
            monthMoneyParent.transform.SetParent(collectiblesParent);
            monthMoneyParent.transform.position = Vector3.zero;

            if (month.moneySpawns != null && month.moneySpawns.Length > 0)
            {
                float monthValue = 0f;

                foreach (MoneySpawnData moneyData in month.moneySpawns)
                {
                    if (moneyData == null || moneyData.moneyPrefab == null)
                    {
                        Debug.LogWarning($"LevelBuilder: Null money data in {month.monthName}. Skipping.");
                        continue;
                    }

                    float moneyZ = currentMonthStartZ + moneyData.spawnDistance;
                    float moneyX = (moneyData.laneIndex - 1) * laneDistance;

                    Vector3 position = new Vector3(moneyX, 0.5f, moneyZ);
                    GameObject money = Instantiate(moneyData.moneyPrefab, position, Quaternion.identity, monthMoneyParent.transform);
                    money.name = $"Money_{totalMoneyCount:D3}_{moneyData.rubleValue}руб";

                    MoneyCollectible collectible = money.GetComponent<MoneyCollectible>();
                    if (collectible != null)
                    {
                        collectible.SetValue((int)moneyData.rubleValue);
                    }

                    monthValue += moneyData.rubleValue;
                    totalMoneyCount++;
                }

                totalValuePlaced += monthValue;
                Debug.Log($"Built {month.moneySpawns.Length} money bundles for Month {month.monthNumber} ({month.monthName}): Total value = {monthValue:F0}₽");
            }

            currentMonthStartZ += month.segmentLength + monthBufferZone;
        }

        Debug.Log($"Built {totalMoneyCount} total money bundles across {monthConfigurations.Length} months");
        Debug.Log($"Total money available in level: {totalValuePlaced:F0}₽");
    }

    void SpawnPine(bool isLeftSide, float zPosition, ref int count)
    {
        float xPosition = isLeftSide ? -pineRoadSideOffset : pineRoadSideOffset;
        float zVariation = Random.Range(-randomPositionVariation, randomPositionVariation);

        Vector3 position = new Vector3(xPosition, 0f, zPosition + zVariation);
        Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        GameObject pine = Instantiate(pinePrefab, position, rotation, environmentParent);
        pine.name = $"Pine_{(isLeftSide ? "L" : "R")}_{count:D3}";

        ParallaxObject parallax = pine.GetComponent<ParallaxObject>();
        if (parallax == null)
        {
            parallax = pine.AddComponent<ParallaxObject>();
        }

        if (playerTransform != null)
        {
            parallax.Initialize(playerTransform, pineParallaxSpeed, enableDynamicParallax);
        }

        count++;
    }

    void SpawnPanelka(bool isLeftSide, float zPosition, ref int count)
    {
        float xPosition = isLeftSide ? -panelkaRoadSideOffset : panelkaRoadSideOffset;
        float zVariation = Random.Range(-randomPositionVariation, randomPositionVariation);
        float yRotation = isLeftSide ? 90f : -90f;

        Vector3 position = new Vector3(xPosition, 0f, zPosition + zVariation);
        Quaternion rotation = Quaternion.Euler(0f, yRotation, 0f);

        GameObject panelka = Instantiate(panelkaPrefab, position, rotation, environmentParent);
        panelka.name = $"Panelka_{(isLeftSide ? "L" : "R")}_{count:D3}";

        ParallaxObject parallax = panelka.GetComponent<ParallaxObject>();
        if (parallax == null)
        {
            parallax = panelka.AddComponent<ParallaxObject>();
        }

        if (playerTransform != null)
        {
            parallax.Initialize(playerTransform, panelkaParallaxSpeed, enableDynamicParallax);
        }

        count++;
    }

    void BuildFinishLine()
    {
        GameObject finishLine = new GameObject("FinishLine");
        finishLine.transform.SetParent(transform);

        float finishLineZ = totalLevelLength + 50f;
        finishLine.transform.position = new Vector3(0f, 1f, finishLineZ);

        BoxCollider trigger = finishLine.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(20f, 10f, 5f);

        FinishLineTrigger finishTrigger = finishLine.AddComponent<FinishLineTrigger>();

        // 🆕 ADD VISUAL MARKER (Optional)
        GameObject visualMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visualMarker.name = "FinishLineVisual";
        visualMarker.transform.SetParent(finishLine.transform);
        visualMarker.transform.localPosition = Vector3.zero;
        visualMarker.transform.localScale = new Vector3(15f, 8f, 2f);

        Renderer renderer = visualMarker.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0f, 0f, 0f, 0.3f);
        }

        Destroy(visualMarker.GetComponent<BoxCollider>());

        Debug.Log($"[LevelBuilder] ✅ Built finish line at Z={finishLineZ:F1}");
    }


}
