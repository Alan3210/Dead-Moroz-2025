using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class ObstacleTextExporter : EditorWindow
{
    private const string MONTH_FOLDER_PATH = "Assets/Scripts/Data/Months";

    private Vector2 scrollPosition;
    private string displayText = "";
    private bool hasContent = false;
    private int currentTab = 0;
    private string[] tabNames = { "Export", "Import" };

    [MenuItem("Tools/Dead Moroz/Obstacle Text Manager")]
    public static void ShowWindow()
    {
        ObstacleTextExporter window = GetWindow<ObstacleTextExporter>("Obstacle Text Manager");
        window.minSize = new Vector2(700, 500);
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField("Obstacle Text Manager", EditorStyles.boldLabel);

        currentTab = GUILayout.Toolbar(currentTab, tabNames);

        GUILayout.Space(10);

        if (currentTab == 0)
        {
            DrawExportTab();
        }
        else
        {
            DrawImportTab();
        }
    }

    void DrawExportTab()
    {
        EditorGUILayout.HelpBox("Export obstacle texts to edit them externally, then use the Import tab to update Unity assets.", MessageType.Info);

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Export to File", GUILayout.Height(40)))
        {
            ExportToFile();
        }

        if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(40)))
        {
            ExportToClipboard();
        }

        if (GUILayout.Button("Export Simple Format (for editing)", GUILayout.Height(40)))
        {
            ExportSimpleFormat();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (hasContent)
        {
            EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(displayText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    void DrawImportTab()
    {
        EditorGUILayout.HelpBox("Import edited obstacle texts from a file. Use the 'Simple Format' export for easier editing.\n\nFormat:\nMONTH 1\n1. First obstacle text\n2. Second obstacle text\n\nMONTH 2\n1. Another obstacle text", MessageType.Info);

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Import from File", GUILayout.Height(40)))
        {
            ImportFromFile();
        }

        if (GUILayout.Button("Import from Clipboard", GUILayout.Height(40)))
        {
            ImportFromClipboard();
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(20);

        if (hasContent)
        {
            EditorGUILayout.LabelField("Import Log:", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(displayText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    void ExportToFile()
    {
        string content = GenerateDetailedExportContent();
        displayText = content;
        hasContent = true;

        string filePath = EditorUtility.SaveFilePanel(
            "Save Obstacle Texts (Detailed)",
            Application.dataPath,
            "DeadMoroz_ObstacleTexts_Detailed.txt",
            "txt"
        );

        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, content, Encoding.UTF8);
            EditorUtility.DisplayDialog(
                "Export Successful",
                $"Detailed obstacle texts exported to:\n{filePath}",
                "OK"
            );

            EditorUtility.RevealInFinder(filePath);
        }
    }

    void ExportToClipboard()
    {
        string content = GenerateDetailedExportContent();
        displayText = content;
        hasContent = true;

        EditorGUIUtility.systemCopyBuffer = content;

        EditorUtility.DisplayDialog(
            "Copied to Clipboard",
            "Detailed obstacle texts have been copied to clipboard!",
            "OK"
        );
    }

    void ExportSimpleFormat()
    {
        string content = GenerateSimpleExportContent();
        displayText = content;
        hasContent = true;

        string filePath = EditorUtility.SaveFilePanel(
            "Save Obstacle Texts (Simple - For Editing)",
            Application.dataPath,
            "DeadMoroz_ObstacleTexts_Simple.txt",
            "txt"
        );

        if (!string.IsNullOrEmpty(filePath))
        {
            File.WriteAllText(filePath, content, Encoding.UTF8);
            EditorUtility.DisplayDialog(
                "Export Successful",
                $"Simple format exported to:\n{filePath}\n\nEdit this file and use Import tab to update Unity!",
                "OK"
            );

            EditorUtility.RevealInFinder(filePath);
        }
    }

    void ImportFromFile()
    {
        string filePath = EditorUtility.OpenFilePanel(
            "Select Obstacle Texts File",
            Application.dataPath,
            "txt"
        );

        if (!string.IsNullOrEmpty(filePath))
        {
            string content = File.ReadAllText(filePath, Encoding.UTF8);
            ProcessImport(content);
        }
    }

    void ImportFromClipboard()
    {
        string content = EditorGUIUtility.systemCopyBuffer;

        if (string.IsNullOrEmpty(content))
        {
            EditorUtility.DisplayDialog(
                "Import Failed",
                "Clipboard is empty!",
                "OK"
            );
            return;
        }

        ProcessImport(content);
    }

    void ProcessImport(string content)
    {
        StringBuilder log = new StringBuilder();
        log.AppendLine("═══════════════════════════════════════════════════════");
        log.AppendLine("              IMPORT OBSTACLE TEXTS");
        log.AppendLine("═══════════════════════════════════════════════════════");
        log.AppendLine();

        MonthConfiguration[] monthConfigs = LoadAllMonthConfigurations();

        if (monthConfigs.Length == 0)
        {
            log.AppendLine("ERROR: No month configurations found!");
            displayText = log.ToString();
            hasContent = true;
            return;
        }

        Dictionary<int, List<string>> parsedData = ParseImportContent(content);

        int totalUpdated = 0;
        int totalErrors = 0;

        AssetDatabase.StartAssetEditing();

        try
        {
            foreach (MonthConfiguration month in monthConfigs)
            {
                if (month == null) continue;

                if (!parsedData.ContainsKey(month.monthNumber))
                {
                    log.AppendLine($"Month {month.monthNumber}: No data found in import file (skipped)");
                    continue;
                }

                List<string> importedTexts = parsedData[month.monthNumber];

                if (month.obstacles == null || month.obstacles.Length == 0)
                {
                    log.AppendLine($"Month {month.monthNumber}: No obstacles in Unity (skipped)");
                    continue;
                }

                log.AppendLine($"───────────────────────────────────────────────────────");
                log.AppendLine($"МЕСЯЦ {month.monthNumber}: {month.monthName}");
                log.AppendLine($"───────────────────────────────────────────────────────");

                int updatedInMonth = 0;
                int errorsInMonth = 0;

                for (int i = 0; i < month.obstacles.Length; i++)
                {
                    ObstacleData obstacle = month.obstacles[i];

                    if (obstacle == null)
                    {
                        log.AppendLine($"  Obstacle {i + 1}: NULL (skipped)");
                        errorsInMonth++;
                        continue;
                    }

                    if (i >= importedTexts.Count)
                    {
                        log.AppendLine($"  Obstacle {i + 1}: No import data (skipped)");
                        errorsInMonth++;
                        continue;
                    }

                    string newText = importedTexts[i];
                    string oldText = obstacle.obstacleText;

                    if (newText != oldText)
                    {
                        obstacle.obstacleText = newText;
                        EditorUtility.SetDirty(obstacle);
                        log.AppendLine($"  ✓ Obstacle {i + 1}: Updated");
                        log.AppendLine($"    Old: {(string.IsNullOrEmpty(oldText) ? "[empty]" : oldText)}");
                        log.AppendLine($"    New: {newText}");
                        updatedInMonth++;
                    }
                    else
                    {
                        log.AppendLine($"  - Obstacle {i + 1}: Unchanged");
                    }
                }

                log.AppendLine($"Updated: {updatedInMonth} | Errors: {errorsInMonth}");
                log.AppendLine();

                totalUpdated += updatedInMonth;
                totalErrors += errorsInMonth;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        log.AppendLine("═══════════════════════════════════════════════════════");
        log.AppendLine($"TOTAL UPDATED: {totalUpdated} obstacles");
        log.AppendLine($"TOTAL ERRORS: {totalErrors}");
        log.AppendLine("═══════════════════════════════════════════════════════");

        displayText = log.ToString();
        hasContent = true;

        EditorUtility.DisplayDialog(
            "Import Complete",
            $"Updated {totalUpdated} obstacles\nErrors: {totalErrors}",
            "OK"
        );
    }

    Dictionary<int, List<string>> ParseImportContent(string content)
    {
        Dictionary<int, List<string>> result = new Dictionary<int, List<string>>();

        string[] lines = content.Split('\n');
        int currentMonth = -1;
        List<string> currentTexts = null;

        Regex monthPattern = new Regex(@"MONTH\s+(\d+)", RegexOptions.IgnoreCase);
        Regex obstaclePattern = new Regex(@"^\s*\d+\.\s*(.+)$");

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (string.IsNullOrEmpty(line)) continue;

            Match monthMatch = monthPattern.Match(line);
            if (monthMatch.Success)
            {
                if (currentMonth > 0 && currentTexts != null)
                {
                    result[currentMonth] = currentTexts;
                }

                currentMonth = int.Parse(monthMatch.Groups[1].Value);
                currentTexts = new List<string>();
                continue;
            }

            Match obstacleMatch = obstaclePattern.Match(line);
            if (obstacleMatch.Success && currentTexts != null)
            {
                string text = obstacleMatch.Groups[1].Value.Trim();
                currentTexts.Add(text);
            }
        }

        if (currentMonth > 0 && currentTexts != null)
        {
            result[currentMonth] = currentTexts;
        }

        return result;
    }

    string GenerateSimpleExportContent()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# DEAD MOROZ 2025 - OBSTACLE TEXTS (SIMPLE FORMAT)");
        sb.AppendLine("# Edit the texts after each number, then import back to Unity");
        sb.AppendLine("# DO NOT change the 'MONTH X' lines or numbering!");
        sb.AppendLine();

        MonthConfiguration[] monthConfigs = LoadAllMonthConfigurations();

        foreach (MonthConfiguration month in monthConfigs)
        {
            if (month == null) continue;

            sb.AppendLine($"MONTH {month.monthNumber}");

            if (month.obstacles == null || month.obstacles.Length == 0)
            {
                sb.AppendLine("# No obstacles");
                sb.AppendLine();
                continue;
            }

            for (int i = 0; i < month.obstacles.Length; i++)
            {
                ObstacleData obstacle = month.obstacles[i];

                string text = (obstacle == null || string.IsNullOrEmpty(obstacle.obstacleText))
                    ? "[Нет текста]"
                    : obstacle.obstacleText;

                sb.AppendLine($"{i + 1}. {text}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    string GenerateDetailedExportContent()
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine("           DEAD MOROZ 2025 - OBSTACLE TEXTS");
        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine();

        MonthConfiguration[] monthConfigs = LoadAllMonthConfigurations();

        if (monthConfigs.Length == 0)
        {
            sb.AppendLine("ERROR: No month configurations found!");
            return sb.ToString();
        }

        int totalObstacles = 0;

        foreach (MonthConfiguration month in monthConfigs)
        {
            if (month == null) continue;

            sb.AppendLine($"───────────────────────────────────────────────────────");
            sb.AppendLine($"МЕСЯЦ {month.monthNumber}: {month.monthName.ToUpper()}");
            sb.AppendLine($"───────────────────────────────────────────────────────");
            sb.AppendLine();

            if (month.obstacles == null || month.obstacles.Length == 0)
            {
                sb.AppendLine("  [Нет препятствий]");
                sb.AppendLine();
                continue;
            }

            for (int i = 0; i < month.obstacles.Length; i++)
            {
                ObstacleData obstacle = month.obstacles[i];

                if (obstacle == null)
                {
                    sb.AppendLine($"  {i + 1}. [NULL OBSTACLE]");
                    continue;
                }

                string laneText = GetLaneText(obstacle.laneIndex);
                string text = string.IsNullOrEmpty(obstacle.obstacleText)
                    ? "[Нет текста]"
                    : obstacle.obstacleText;

                sb.AppendLine($"  {i + 1}. {text}");
                sb.AppendLine($"     Позиция: {obstacle.spawnDistance:F1}м | Полоса: {laneText}");
                sb.AppendLine();

                totalObstacles++;
            }
        }

        sb.AppendLine("═══════════════════════════════════════════════════════");
        sb.AppendLine($"ИТОГО: {totalObstacles} препятствий в {monthConfigs.Length} месяцах");
        sb.AppendLine("═══════════════════════════════════════════════════════");

        return sb.ToString();
    }

    MonthConfiguration[] LoadAllMonthConfigurations()
    {
        List<MonthConfiguration> configs = new List<MonthConfiguration>();

        string[] guids = AssetDatabase.FindAssets("t:MonthConfiguration", new[] { MONTH_FOLDER_PATH });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            MonthConfiguration config = AssetDatabase.LoadAssetAtPath<MonthConfiguration>(path);

            if (config != null)
            {
                configs.Add(config);
            }
        }

        configs.Sort((a, b) => a.monthNumber.CompareTo(b.monthNumber));

        return configs.ToArray();
    }

    string GetLaneText(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0: return "Левая";
            case 1: return "Средняя";
            case 2: return "Правая";
            default: return $"Неизвестная ({laneIndex})";
        }
    }
}
