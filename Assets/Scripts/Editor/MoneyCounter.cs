using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class MoneyCounter : EditorWindow
{
    private const float TARGET_PER_MONTH = 64637f;

    [MenuItem("Tools/Count Money in Scene")]
    static void CountMoney()
    {
        LevelBuilder levelBuilder = Object.FindFirstObjectByType<LevelBuilder>();

        if (levelBuilder == null)
        {
            Debug.LogError("❌ LevelBuilder not found in scene! Cannot determine month boundaries.");
            return;
        }

        MoneyCollectible[] allMoney = Object.FindObjectsByType<MoneyCollectible>(FindObjectsSortMode.None);

        if (allMoney.Length == 0)
        {
            Debug.LogWarning("⚠️ No MoneyCollectible objects found in scene!");
            return;
        }

        MonthConfiguration[] monthConfigs = levelBuilder.MonthConfigurations;
        if (monthConfigs == null || monthConfigs.Length == 0)
        {
            Debug.LogError("❌ No month configurations found in LevelBuilder!");
            return;
        }

        float startSafeZone = levelBuilder.StartSafeZoneDistance;
        float bufferZone = levelBuilder.MonthBufferZone;

        List<MonthMoneyData> monthData = CalculateMonthBoundaries(monthConfigs, startSafeZone, bufferZone);

        foreach (MoneyCollectible money in allMoney)
        {
            float moneyZ = money.transform.position.z;

            foreach (MonthMoneyData month in monthData)
            {
                if (moneyZ >= month.startZ && moneyZ < month.endZ)
                {
                    month.totalValue += money.GetValue();
                    month.moneyCount++;
                    break;
                }
            }
        }

        Debug.Log("═══════════════════════════════════════════════════");
        Debug.Log("<color=cyan><b>💰 MONEY DISTRIBUTION BY MONTH 💰</b></color>");
        Debug.Log("═══════════════════════════════════════════════════");

        float grandTotal = 0f;
        int totalBundles = 0;
        int perfectMonths = 0;

        foreach (MonthMoneyData month in monthData)
        {
            grandTotal += month.totalValue;
            totalBundles += month.moneyCount;

            string statusIcon;
            string colorCode;

            if (month.totalValue == TARGET_PER_MONTH)
            {
                statusIcon = "✅";
                colorCode = "green";
                perfectMonths++;
            }
            else if (Mathf.Abs(month.totalValue - TARGET_PER_MONTH) < 100)
            {
                statusIcon = "⚠️";
                colorCode = "yellow";
            }
            else
            {
                statusIcon = "❌";
                colorCode = "orange";
            }

            int difference = (int)(month.totalValue - TARGET_PER_MONTH);
            string diffText = difference >= 0 ? $"+{difference:N0}₽" : $"{difference:N0}₽";

            Debug.Log($"{statusIcon} <color={colorCode}><b>Month {month.monthNumber}</b> ({month.monthName})</color>");
            Debug.Log($"   └─ Money: <b>{month.totalValue:N0}₽</b> | Bundles: {month.moneyCount} | Diff: {diffText}");
        }

        Debug.Log("───────────────────────────────────────────────────");
        Debug.Log($"<color=cyan><b>TOTALS:</b></color>");
        Debug.Log($"   💵 Grand Total: <b>{grandTotal:N0}₽</b>");
        Debug.Log($"   📦 Total Bundles: <b>{totalBundles}</b>");
        Debug.Log($"   🎯 Perfect Months: <b>{perfectMonths}/{monthData.Count}</b>");

        float expectedTotal = TARGET_PER_MONTH * monthData.Count;
        float totalDifference = grandTotal - expectedTotal;

        if (perfectMonths == monthData.Count)
        {
            Debug.Log($"<color=green><b>✨ PERFECT! All {monthData.Count} months have exactly {TARGET_PER_MONTH:N0}₽!</b></color>");
        }
        else
        {
            Debug.Log($"<color=yellow>📊 Expected total: {expectedTotal:N0}₽ | Actual: {grandTotal:N0}₽ | Difference: {totalDifference:+0;-0}₽</color>");
        }

        Debug.Log("═══════════════════════════════════════════════════");
    }

    static List<MonthMoneyData> CalculateMonthBoundaries(MonthConfiguration[] configs, float startSafeZone, float bufferZone)
    {
        List<MonthMoneyData> result = new List<MonthMoneyData>();
        float currentZ = startSafeZone;

        for (int i = 0; i < configs.Length; i++)
        {
            MonthConfiguration month = configs[i];
            if (month == null) continue;

            MonthMoneyData data = new MonthMoneyData
            {
                monthNumber = month.monthNumber,
                monthName = month.monthName,
                startZ = currentZ,
                endZ = currentZ + month.segmentLength
            };

            result.Add(data);
            currentZ = data.endZ + bufferZone;
        }

        return result;
    }

    class MonthMoneyData
    {
        public int monthNumber;
        public string monthName;
        public float startZ;
        public float endZ;
        public float totalValue;
        public int moneyCount;
    }
}
