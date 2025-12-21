using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ObstacleSpacingValidator : EditorWindow
{
    private const float MIN_SPACING_SAME_LANE = 15f;
    private const float MIN_SPACING_DIFFERENT_LANE = 8f;
    private const float IDEAL_SPACING = 10f;

    private Vector2 scrollPosition;
    private List<SpacingIssue> issues = new List<SpacingIssue>();
    private bool autoRefresh = true;

    private class SpacingIssue
    {
        public string monthName;
        public ObstacleData obstacle1;
        public ObstacleData obstacle2;
        public float distance;
        public bool sameLane;
        public string severity;
    }

    [MenuItem("Tools/Dead Moroz/Validate Obstacle Spacing")]
    public static void ShowWindow()
    {
        var window = GetWindow<ObstacleSpacingValidator>("Obstacle Spacing Validator");
        window.minSize = new Vector2(600, 400);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Obstacle Spacing Validation Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Validate All Months", GUILayout.Height(30)))
        {
            ValidateAllMonths();
        }
        autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Issues Found: {issues.Count}", EditorStyles.helpBox);
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        if (issues.Count == 0)
        {
            EditorGUILayout.HelpBox("No spacing issues found! All obstacles are properly spaced.", MessageType.Info);
        }
        else
        {
            foreach (var issue in issues)
            {
                DrawIssue(issue);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawIssue(SpacingIssue issue)
    {
        MessageType messageType = issue.severity == "Critical" ? MessageType.Error : MessageType.Warning;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField($"[{issue.severity}] {issue.monthName}", EditorStyles.boldLabel);

        string laneInfo = issue.sameLane ? "(SAME LANE)" : "(Different Lanes)";
        EditorGUILayout.LabelField($"Distance: {issue.distance:F1} units {laneInfo}");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Obstacle 1", GUILayout.Width(150)))
        {
            Selection.activeObject = issue.obstacle1;
            EditorGUIUtility.PingObject(issue.obstacle1);
        }
        EditorGUILayout.LabelField($"Distance: {issue.obstacle1.spawnDistance:F1}, Lane: {issue.obstacle1.laneIndex}");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Obstacle 2", GUILayout.Width(150)))
        {
            Selection.activeObject = issue.obstacle2;
            EditorGUIUtility.PingObject(issue.obstacle2);
        }
        EditorGUILayout.LabelField($"Distance: {issue.obstacle2.spawnDistance:F1}, Lane: {issue.obstacle2.laneIndex}");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void ValidateAllMonths()
    {
        issues.Clear();

        string[] monthGuids = AssetDatabase.FindAssets("t:MonthConfiguration");

        foreach (string guid in monthGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonthConfiguration month = AssetDatabase.LoadAssetAtPath<MonthConfiguration>(path);

            if (month != null)
            {
                ValidateMonth(month);
            }
        }

        issues = issues.OrderBy(i => i.severity == "Critical" ? 0 : 1)
                       .ThenBy(i => i.monthName)
                       .ToList();

        Debug.Log($"[Obstacle Spacing Validator] Found {issues.Count} spacing issues across all months.");
    }

    private void ValidateMonth(MonthConfiguration month)
    {
        if (month.obstacles == null || month.obstacles.Length < 2)
            return;

        var sortedObstacles = month.obstacles
            .Where(o => o != null)
            .OrderBy(o => o.spawnDistance)
            .ToList();

        for (int i = 0; i < sortedObstacles.Count - 1; i++)
        {
            ObstacleData current = sortedObstacles[i];
            ObstacleData next = sortedObstacles[i + 1];

            float distance = next.spawnDistance - current.spawnDistance;
            bool sameLane = current.laneIndex == next.laneIndex;

            string severity = null;

            if (sameLane && distance < MIN_SPACING_SAME_LANE)
            {
                severity = "Critical";
            }
            else if (!sameLane && distance < MIN_SPACING_DIFFERENT_LANE)
            {
                severity = "Warning";
            }

            if (severity != null)
            {
                issues.Add(new SpacingIssue
                {
                    monthName = $"Month {month.monthNumber:D2} - {month.monthName}",
                    obstacle1 = current,
                    obstacle2 = next,
                    distance = distance,
                    sameLane = sameLane,
                    severity = severity
                });
            }
        }
    }

    private void OnInspectorUpdate()
    {
        if (autoRefresh)
        {
            Repaint();
        }
    }
}
