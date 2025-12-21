using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class ObstacleSpacingImporter : EditorWindow
{
    private Vector2 scrollPosition;
    private bool showPreview = false;
    private List<ObstacleUpdate> updates = new List<ObstacleUpdate>();
    private int updatedCount = 0;

    private class ObstacleUpdate
    {
        public string assetPath;
        public ObstacleData asset;
        public float newDistance;
        public int newLane;
        public float oldDistance;
        public int oldLane;
        public bool willUpdate;
    }

    [MenuItem("Tools/Dead Moroz/Import Optimal Spacing")]
    public static void ShowWindow()
    {
        var window = GetWindow<ObstacleSpacingImporter>("Obstacle Spacing Importer");
        window.minSize = new Vector2(700, 500);
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Obstacle Spacing CSV Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "This tool will automatically update all obstacle spawn distances and lane indices " +
            "based on the optimal spacing configuration.",
            MessageType.Info);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Load Optimal Configuration", GUILayout.Height(40)))
        {
            LoadOptimalConfiguration();
        }

        if (GUILayout.Button("Apply All Changes", GUILayout.Height(40)))
        {
            ApplyAllChanges();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (updates.Count > 0)
        {
            EditorGUILayout.LabelField($"Total Updates: {updates.Count}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Applied: {updatedCount}/{updates.Count}");

            EditorGUILayout.Space();
            showPreview = EditorGUILayout.Toggle("Show Preview", showPreview);
            EditorGUILayout.Space();

            if (showPreview)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                foreach (var update in updates)
                {
                    DrawUpdatePreview(update);
                }

                EditorGUILayout.EndScrollView();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Click 'Load Optimal Configuration' to preview changes.", MessageType.Info);
        }
    }

    private void DrawUpdatePreview(ObstacleUpdate update)
    {
        if (update.willUpdate)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(update.asset.name, EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Distance: {update.oldDistance:F1} -> {update.newDistance:F1}");
            EditorGUILayout.LabelField($"Lane: {update.oldLane} -> {update.newLane}");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }

    private void LoadOptimalConfiguration()
    {
        updates.Clear();
        updatedCount = 0;

        var config = GetOptimalSpacingData();

        foreach (var entry in config)
        {
            ObstacleData obstacle = AssetDatabase.LoadAssetAtPath<ObstacleData>(entry.assetPath);

            if (obstacle != null)
            {
                bool needsUpdate = Mathf.Abs(obstacle.spawnDistance - entry.distance) > 0.1f ||
                                   obstacle.laneIndex != entry.lane;

                updates.Add(new ObstacleUpdate
                {
                    assetPath = entry.assetPath,
                    asset = obstacle,
                    newDistance = entry.distance,
                    newLane = entry.lane,
                    oldDistance = obstacle.spawnDistance,
                    oldLane = obstacle.laneIndex,
                    willUpdate = needsUpdate
                });
            }
            else
            {
                Debug.LogWarning($"[Obstacle Importer] Could not find asset: {entry.assetPath}");
            }
        }

        int changeCount = updates.Count(u => u.willUpdate);
        Debug.Log($"[Obstacle Importer] Loaded {updates.Count} obstacles. {changeCount} will be updated.");

        showPreview = true;
    }

    private void ApplyAllChanges()
    {
        if (updates.Count == 0)
        {
            EditorUtility.DisplayDialog("No Data", "Please load the configuration first.", "OK");
            return;
        }

        if (!EditorUtility.DisplayDialog(
            "Apply Changes",
            $"This will update {updates.Count(u => u.willUpdate)} obstacles.\n\nDo you want to continue?",
            "Yes, Apply",
            "Cancel"))
        {
            return;
        }

        updatedCount = 0;

        foreach (var update in updates)
        {
            if (update.willUpdate)
            {
                update.asset.spawnDistance = update.newDistance;
                update.asset.laneIndex = update.newLane;

                EditorUtility.SetDirty(update.asset);
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Obstacle Importer] Successfully updated {updatedCount} obstacles!");

        EditorUtility.DisplayDialog(
            "Success",
            $"Updated {updatedCount} obstacles successfully!\n\nRun the Spacing Validator to verify.",
            "OK");
    }

    private List<(string assetPath, float distance, int lane)> GetOptimalSpacingData()
    {
        return new List<(string, float, int)>
        {
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_01.asset", 8f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_02.asset", 14f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_03.asset", 20f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_04.asset", 26f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_05.asset", 32f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_06.asset", 38f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_07.asset", 44f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_08.asset", 50f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_09.asset", 56f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_010.asset", 62f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_011.asset", 68f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_012.asset", 74f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_013.asset", 80f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_014.asset", 85f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_015.asset", 90f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_01-Jan/Obstacle_Month01_016.asset", 96f, 1),

            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_01.asset", 10f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_02.asset", 20f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_03.asset", 30f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_04.asset", 40f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_05.asset", 50f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_06.asset", 60f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_07.asset", 70f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_08.asset", 78f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_09.asset", 87f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_02-Feb/Obstacle_Month02_010.asset", 95f, 1),

            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_01.asset", 8f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_02.asset", 16f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_03.asset", 24f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_04.asset", 32f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_05.asset", 40f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_06.asset", 48f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_07.asset", 56f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_08.asset", 64f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_09.asset", 72f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_010.asset", 79f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_011.asset", 86f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_012.asset", 92f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_03-Mar/Obstacle_Month03_013.asset", 98f, 1),

            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 1.asset", 6f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 2.asset", 11f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 3.asset", 16f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 4.asset", 21f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 5.asset", 26f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 6.asset", 31f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 7.asset", 36f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 8.asset", 41f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 9.asset", 46f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 10.asset", 51f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 11.asset", 56f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 12.asset", 61f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 13.asset", 66f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 14.asset", 71f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 15.asset", 76f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 16.asset", 81f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 17.asset", 86f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 18.asset", 92f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_04-Apr/Obstacle_Month04_01 19.asset", 97f, 1),

            ("Assets/Scripts/Data/Months/Obstacles_month_05-May/Obstacle_Month05_01 1.asset", 20f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_05-May/Obstacle_Month05_01 2.asset", 40f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_05-May/Obstacle_Month05_01 3.asset", 60f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_05-May/Obstacle_Month05_01 4.asset", 80f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_05-May/Obstacle_Month05_01 5.asset", 95f, 0),

            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 1.asset", 10f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 2.asset", 19f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 3.asset", 28f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 4.asset", 37f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 5.asset", 46f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 6.asset", 55f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 7.asset", 64f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 8.asset", 73f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 9.asset", 82f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 10.asset", 90f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_06-Jun/Obstacle_Month06_01 11.asset", 97f, 1),

            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 1.asset", 5f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 2.asset", 10f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 3.asset", 15f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 4.asset", 20f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 5.asset", 25f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 6.asset", 30f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 7.asset", 35f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 8.asset", 40f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 9.asset", 45f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 10.asset", 50f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 11.asset", 55f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 12.asset", 60f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 13.asset", 65f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 14.asset", 70f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 15.asset", 75f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 16.asset", 80f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 17.asset", 84f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 18.asset", 88f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 19.asset", 92f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 20.asset", 96f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_07-Jul/Obstacle_Month07_01 21.asset", 99f, 2),

            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 1.asset", 9f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 2.asset", 17f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 3.asset", 26f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 4.asset", 35f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 5.asset", 44f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 6.asset", 52f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 7.asset", 61f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 8.asset", 69f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 9.asset", 77f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 10.asset", 85f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 11.asset", 92f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_08-Aug/Obstacle_Month08_01 12.asset", 98f, 2),

            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 1.asset", 8f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 2.asset", 16f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 3.asset", 24f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 4.asset", 32f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 5.asset", 40f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 6.asset", 48f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 7.asset", 56f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 8.asset", 64f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 9.asset", 72f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 10.asset", 79f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 11.asset", 86f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 12.asset", 92f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_09-Sep/Obstacle_Month09_01 13.asset", 98f, 2),

            ("Assets/Scripts/Data/Months/Obstacles_month_10-Oct/Obstacle_Month10_01 1.asset", 25f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_10-Oct/Obstacle_Month10_01 2.asset", 50f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_10-Oct/Obstacle_Month10_01 3.asset", 75f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_10-Oct/Obstacle_Month10_01 4.asset", 95f, 1),

            ("Assets/Scripts/Data/Months/Obstacles_month_11-Nov/Obstacle_Month11_01 1.asset", 17f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_11-Nov/Obstacle_Month11_01 2.asset", 33f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_11-Nov/Obstacle_Month11_01 3.asset", 50f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_11-Nov/Obstacle_Month11_01 4.asset", 67f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_11-Nov/Obstacle_Month11_01 5.asset", 83f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_11-Nov/Obstacle_Month11_01 6.asset", 96f, 2),

            ("Assets/Scripts/Data/Months/Obstacles_month_12-Dec/Obstacle_Month12_01 1.asset", 17f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_12-Dec/Obstacle_Month12_01 2.asset", 33f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_12-Dec/Obstacle_Month12_01 3.asset", 50f, 0),
            ("Assets/Scripts/Data/Months/Obstacles_month_12-Dec/Obstacle_Month12_01 4.asset", 67f, 2),
            ("Assets/Scripts/Data/Months/Obstacles_month_12-Dec/Obstacle_Month12_01 5.asset", 83f, 1),
            ("Assets/Scripts/Data/Months/Obstacles_month_12-Dec/Obstacle_Month12_01 6.asset", 96f, 0)
        };
    }
}
