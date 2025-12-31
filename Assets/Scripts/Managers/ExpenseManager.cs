using System.Collections.Generic;
using UnityEngine;

public class ExpenseManager : MonoBehaviour
{
    public static ExpenseManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private List<ExpenseData> allExpenses = new List<ExpenseData>();
    [SerializeField] private float inflationRatePerMonth = 0.01f; // 1%

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Calculates the cost of a specific expense for a given month number (1-based index).
    /// Formula: BaseCost * ((1 + Rate) ^ (Month - 1))
    /// </summary>
    public int CalculateExpenseCost(ExpenseData expense, int monthNumber)
    {
        if (expense == null) return 0;

        float inflationMultiplier = Mathf.Pow(1f + inflationRatePerMonth, monthNumber - 1);
        return Mathf.RoundToInt(expense.baseCost * inflationMultiplier);
    }

    /// <summary>
    /// Returns the total cost of all enabled expenses for the current month.
    /// </summary>
    public int CalculateTotalExpenses(int monthNumber)
    {
        int total = 0;
        foreach (var expense in allExpenses)
        {
            if (expense.isMandatory || expense.isEnabled)
            {
                total += CalculateExpenseCost(expense, monthNumber);
            }
        }
        return total;
    }

    public void ToggleExpense(ExpenseData expense, bool isEnabled)
    {
        if (expense.isMandatory)
        {
            Debug.LogWarning($"Cannot toggle mandatory expense: {expense.expenseName}");
            return;
        }
        expense.isEnabled = isEnabled;
    }

    public void AddExpense(ExpenseData newExpense)
    {
        if (newExpense != null)
        {
            allExpenses.Add(newExpense);
        }
    }

    public List<ExpenseData> GetAllExpenses()
    {
        return allExpenses;
    }
}
