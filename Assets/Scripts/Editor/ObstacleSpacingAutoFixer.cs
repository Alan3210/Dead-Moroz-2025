using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ObstacleSpacingAutoFixer : EditorWindow
{
    private const float MIN_SPACING_SAME_LANE = 15f;
    private const float MIN_SPACING_DIFFERENT_LANE = 8f;
    private const float SEGMENT_LENGTH = 100f;

    private Vector2 scrollPosition;
    private List<MonthData> monthsData = new List<MonthData>();
    private bool isProcessed = false;

    private class MonthData
    {
        public int monthNumber;
        public string monthName;
        public MonthConfiguration config;
        public List<ObstacleData> obstacles = new List<ObstacleData>();
        public float calculatedSpacing;
        public int issuesFixed;
        public bool canFitAll;
        public int maxRecommended;
    }

    [MenuItem("Tools/Dead Moroz/Auto-Fix All Spacing")]
    public static void ShowWindow()
    {
        var window = GetWindow<ObstacleSpacingAutoFixer>("Auto-Fix Spacing");
        window.minSize = new Vector2(800, 500);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Automatic Obstacle Spacing Fixer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will automatically:\n" +
            "1. Discover ALL obstacle assets in your project\n" +
            "2. Calculate optimal spacing respecting 8-unit minimum\n" +
            "3. Apply balanced lane distribution\n" +
            "4. FORCE redistribute obstacles to eliminate clustering",
            MessageType.Info);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Analyze All Obstacles", GUILayout.Height(40)))
        {
            AnalyzeAllObstacles();
        }

        GUI.enabled = isProcessed && monthsData.Count > 0;
        if (GUILayout.Button("Apply Optimal Spacing", GUILayout.Height(40)))
        {
            ApplyOptimalSpacing();
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (monthsData.Count > 0)
        {
            EditorGUILayout.LabelField(string.Format("Found {0} months", monthsData.Count), EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (var month in monthsData)
            {
                DrawMonthData(month);
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawMonthData(MonthData month)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField(
            string.Format("Month {0:D2} - {1}", month.monthNumber, month.monthName),
            EditorStyles.boldLabel);

        EditorGUILayout.LabelField(string.Format("Obstacles: {0} | Spacing: {1:F1} units",
            month.obstacles.Count, month.calculatedSpacing));

        if (!month.canFitAll)
        {
            EditorGUILayout.HelpBox(
                string.Format("WARNING: Too many obstacles! Recommended max: {0}. Current: {1}\nSpacing will be compressed but obstacles will be distributed evenly.",
                month.maxRecommended, month.obstacles.Count),
                MessageType.Warning);
        }

        if (month.issuesFixed > 0)
        {
            EditorGUILayout.LabelField(string.Format("Fixed: {0} obstacles", month.issuesFixed), EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();
    }

    private void AnalyzeAllObstacles()
    {
        monthsData.Clear();
        isProcessed = false;

        string[] monthGuids = AssetDatabase.FindAssets("t:MonthConfiguration");

        foreach (string guid in monthGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonthConfiguration monthConfig = AssetDatabase.LoadAssetAtPath<MonthConfiguration>(path);

            if (monthConfig != null && monthConfig.obstacles != null)
            {
                var monthData = new MonthData
                {
                    monthNumber = monthConfig.monthNumber,
                    monthName = monthConfig.monthName,
                    config = monthConfig,
                    obstacles = monthConfig.obstacles.Where(o => o != null).ToList()
                };

                int obstacleCount = monthData.obstacles.Count;

                int maxObstacles = CalculateMaxObstacles();
                monthData.maxRecommended = maxObstacles;
                monthData.canFitAll = obstacleCount <= maxObstacles;

                if (obstacleCount > 0)
                {
                    monthData.calculatedSpacing = (SEGMENT_LENGTH - 5f) / obstacleCount;
                }

                monthsData.Add(monthData);
            }
        }

        monthsData = monthsData.OrderBy(m => m.monthNumber).ToList();
        isProcessed = true;

        int totalObstacles = monthsData.Sum(m => m.obstacles.Count);
        int problemMonths = monthsData.Count(m => !m.canFitAll);

        Debug.Log(string.Format("[Auto-Fixer] Analyzed {0} months with {1} total obstacles. {2} months have too many obstacles.",
            monthsData.Count, totalObstacles, problemMonths));
    }

    private int CalculateMaxObstacles()
    {
        return Mathf.FloorToInt(SEGMENT_LENGTH / MIN_SPACING_DIFFERENT_LANE) - 1;
    }

    private void ApplyOptimalSpacing()
    {
        int problemMonths = monthsData.Count(m => !m.canFitAll);

        if (problemMonths > 0)
        {
            if (!EditorUtility.DisplayDialog(
                "Warning: Overcrowded Months",
                string.Format("{0} months have too many obstacles. Obstacles will be compressed but distributed evenly.\n\nContinue?", problemMonths),
                "Yes, Continue",
                "Cancel"))
            {
                return;
            }
        }

        if (!EditorUtility.DisplayDialog(
            "Apply Optimal Spacing",
            string.Format("This will FORCE update spacing for ALL {0} obstacles across {1} months.\n\nThis will eliminate clustering and distribute obstacles evenly.\n\nContinue?",
                monthsData.Sum(m => m.obstacles.Count), monthsData.Count),
            "Yes, Fix All",
            "Cancel"))
        {
            return;
        }

        int totalFixed = 0;

        AssetDatabase.StartAssetEditing();

        try
        {
            foreach (var month in monthsData)
            {
                month.issuesFixed = ApplySpacingToMonth(month);
                totalFixed += month.issuesFixed;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(string.Format("[Auto-Fixer] Successfully redistributed {0} obstacles!", totalFixed));

        string message = string.Format("Redistributed {0} obstacles across {1} months!", totalFixed, monthsData.Count);

        if (problemMonths > 0)
        {
            message += string.Format("\n\nNote: {0} months have compressed spacing due to too many obstacles.\nConsider removing some obstacles if spacing is too tight.", problemMonths);
        }
        else
        {
            message += "\n\nRun the Spacing Validator to verify all warnings are resolved.";
        }

        EditorUtility.DisplayDialog("Success", message, "OK");
    }

    private int ApplySpacingToMonth(MonthData month)
    {
        if (month.obstacles.Count == 0)
            return 0;

        var sortedObstacles = month.obstacles
            .OrderBy(o => o.spawnDistance)
            .ToList();

        int fixedCount = 0;
        int obstacleCount = sortedObstacles.Count;

        int[] lanePattern = GenerateLanePattern(obstacleCount);
        float[] distancePattern = GenerateDistancePattern(obstacleCount);

        Debug.Log(string.Format("[Auto-Fixer] Month {0}: Applying spacing pattern to {1} obstacles:",
            month.monthNumber, obstacleCount));

        for (int i = 0; i < obstacleCount; i++)
        {
            ObstacleData obstacle = sortedObstacles[i];

            float oldDistance = obstacle.spawnDistance;
            float newDistance = distancePattern[i];
            int oldLane = obstacle.laneIndex;
            int newLane = lanePattern[i];

            SerializedObject so = new SerializedObject(obstacle);

            SerializedProperty distanceProp = so.FindProperty("spawnDistance");
            SerializedProperty laneProp = so.FindProperty("laneIndex");

            distanceProp.floatValue = newDistance;
            laneProp.intValue = newLane;

            so.ApplyModifiedProperties();

            fixedCount++;

            if (i < 3)
            {
                Debug.Log(string.Format("  Obstacle {0}: {1:F1} -> {2:F1}, Lane {3} -> {4}",
                    i + 1, oldDistance, newDistance, oldLane, newLane));
            }
        }

        SerializedObject monthSo = new SerializedObject(month.config);
        SerializedProperty obstaclesProp = monthSo.FindProperty("obstacles");

        obstaclesProp.ClearArray();
        for (int i = 0; i < sortedObstacles.Count; i++)
        {
            obstaclesProp.InsertArrayElementAtIndex(i);
            obstaclesProp.GetArrayElementAtIndex(i).objectReferenceValue = sortedObstacles[i];
        }

        monthSo.ApplyModifiedProperties();

        Debug.Log(string.Format("[Auto-Fixer] Month {0}: Updated {1} obstacles", month.monthNumber, fixedCount));

        return fixedCount;
    }

    private int[] GenerateLanePattern(int count)
    {
        int[] lanes = new int[count];

        int[] pattern = { 1, 0, 2 };

        for (int i = 0; i < count; i++)
        {
            lanes[i] = pattern[i % pattern.Length];
        }

        return lanes;
    }

    private float[] GenerateDistancePattern(int count)
    {
        float[] distances = new float[count];

        if (count == 0)
            return distances;

        if (count == 1)
        {
            distances[0] = SEGMENT_LENGTH / 2f;
            return distances;
        }

        float startPosition = 5f;
        float endPosition = SEGMENT_LENGTH - 5f;
        float availableSpace = endPosition - startPosition;

        float spacing = availableSpace / (count - 1);

        for (int i = 0; i < count; i++)
        {
            distances[i] = startPosition + (i * spacing);
        }

        return distances;
    }

}
