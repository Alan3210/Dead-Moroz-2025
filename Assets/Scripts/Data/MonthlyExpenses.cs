using UnityEngine;

[CreateAssetMenu(fileName = "MonthlyExpenses", menuName = "Dead Moroz/Monthly Expenses")]
public class MonthlyExpenses : ScriptableObject
{
    [Header("Expense Data References")]
    [Tooltip("List of all expense data assets")]
    public ExpenseData[] expenseDataList;

    [Header("Economic Settings")]
    [Tooltip("Inflation rate per month (1% = 0.01)")]
    public float inflationRate = 0.01f;

    public ExpenseItem[] GetExpenses(int monthNumber)
    {
        if (expenseDataList == null || expenseDataList.Length == 0)
        {
            Debug.LogWarning("MonthlyExpenses: No expense data configured!");
            return new ExpenseItem[0];
        }

        float multiplier = Mathf.Pow(1f + inflationRate, monthNumber - 1);

        ExpenseItem[] expenses = new ExpenseItem[expenseDataList.Length];

        for (int i = 0; i < expenseDataList.Length; i++)
        {
            ExpenseData data = expenseDataList[i];

            if (data != null && data.isEnabled)
            {
                expenses[i] = new ExpenseItem
                {
                    name = data.expenseName,
                    amount = data.baseCost * multiplier
                };
            }
            else
            {
                expenses[i] = new ExpenseItem
                {
                    name = "Unknown",
                    amount = 0
                };
            }
        }

        return expenses;
    }

    public float GetTotalExpenses(int monthNumber)
    {
        if (expenseDataList == null || expenseDataList.Length == 0)
        {
            return 0f;
        }

        float baseTotal = 0f;
        foreach (ExpenseData data in expenseDataList)
        {
            if (data != null && data.isEnabled)
            {
                baseTotal += data.baseCost;
            }
        }

        float multiplier = Mathf.Pow(1f + inflationRate, monthNumber - 1);
        return baseTotal * multiplier;
    }
}

[System.Serializable]
public class ExpenseItem
{
    public string name;
    public float amount;
}
