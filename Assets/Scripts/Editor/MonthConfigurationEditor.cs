using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(MonthConfiguration))]
public class MonthConfigurationEditor : Editor
{
    private const float MIN_SPACING_SAME_LANE = 15f;
    private const float MIN_SPACING_DIFFERENT_LANE = 8f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Obstacle Analysis", EditorStyles.boldLabel);

        MonthConfiguration month = (MonthConfiguration)target;

        if (month.obstacles == null || month.obstacles.Length == 0)
        {
            EditorGUILayout.HelpBox("No obstacles assigned to this month.", MessageType.Info);
            return;
        }

        var sortedObstacles = month.obstacles
            .Where(o => o != null)
            .OrderBy(o => o.spawnDistance)
            .ToList();

        EditorGUILayout.LabelField($"Total Obstacles: {sortedObstacles.Count}");

        if (sortedObstacles.Count > 0)
        {
            float minDist = sortedObstacles.First().spawnDistance;
            float maxDist = sortedObstacles.Last().spawnDistance;
            float avgSpacing = sortedObstacles.Count > 1
                ? (maxDist - minDist) / (sortedObstacles.Count - 1)
                : 0;

            EditorGUILayout.LabelField($"Distance Range: {minDist:F1} - {maxDist:F1}");
            EditorGUILayout.LabelField($"Average Spacing: {avgSpacing:F1} units");
        }

        EditorGUILayout.Space();

        int criticalIssues = 0;
        int warnings = 0;

        for (int i = 0; i < sortedObstacles.Count - 1; i++)
        {
            ObstacleData current = sortedObstacles[i];
            ObstacleData next = sortedObstacles[i + 1];

            float distance = next.spawnDistance - current.spawnDistance;
            bool sameLane = current.laneIndex == next.laneIndex;

            if (sameLane && distance < MIN_SPACING_SAME_LANE)
            {
                criticalIssues++;
                EditorGUILayout.HelpBox(
                    $"Critical: Obstacles too close in same lane!\n" +
                    $"Distance {current.spawnDistance:F1} -> {next.spawnDistance:F1} (Gap: {distance:F1} units, Lane: {current.laneIndex})",
                    MessageType.Error);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"Select {current.name}", GUILayout.Width(150)))
                {
                    Selection.activeObject = current;
                    EditorGUIUtility.PingObject(current);
                }
                if (GUILayout.Button($"Select {next.name}", GUILayout.Width(150)))
                {
                    Selection.activeObject = next;
                    EditorGUIUtility.PingObject(next);
                }
                EditorGUILayout.EndHorizontal();
            }
            else if (!sameLane && distance < MIN_SPACING_DIFFERENT_LANE)
            {
                warnings++;
                EditorGUILayout.HelpBox(
                    $"Warning: Obstacles close in different lanes.\n" +
                    $"Distance {current.spawnDistance:F1} (Lane {current.laneIndex}) -> {next.spawnDistance:F1} (Lane {next.laneIndex}) (Gap: {distance:F1} units)",
                    MessageType.Warning);
            }
        }

        EditorGUILayout.Space();

        if (criticalIssues == 0 && warnings == 0)
        {
            EditorGUILayout.HelpBox("All obstacles are properly spaced!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"Found {criticalIssues} critical issues and {warnings} warnings.", MessageType.None);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Spacing Validator Window", GUILayout.Height(30)))
        {
            ObstacleSpacingValidator.ShowWindow();
        }
    }
}
