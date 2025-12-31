using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinanceSummaryController : MonoBehaviour
{
    public static FinanceSummaryController Instance { get; private set; }

    [Header("Timing Settings")]
    [SerializeField] private float delayBeforeStart = 1.0f;
    [SerializeField] private float delayBetweenExpenses = 0.5f;
    [SerializeField] private float delayBeforeFinish = 1.0f;

    // Events for UI/Logic hookups
    public event Action<int> OnIncomeProcessed; // Returns total income
    public event Action<ExpenseData, int> OnExpenseProcessed; // Returns expense data and cost
    public event Action<int> OnBalanceUpdated; // Returns current running balance
    public event Action<int, bool> OnSummaryFinished; // Returns final balance, isBankrupt
    public event Action<int> OnMicroloanAvailable; // Returns the deficit amount

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void StartMonthlySummary(int monthNumber)
    {
        StartCoroutine(ProcessMonthlyFinances(monthNumber));
    }

    private IEnumerator ProcessMonthlyFinances(int monthNumber)
    {
        yield return new WaitForSeconds(delayBeforeStart);

        // 1. Process Income
        int income = EconomyManager.Instance.GetMonthMoney();
        int currentBalance = income;
        
        OnIncomeProcessed?.Invoke(income);
        OnBalanceUpdated?.Invoke(currentBalance);

        yield return new WaitForSeconds(delayBetweenExpenses);

        // 2. Process Expenses Sequentially
        List<ExpenseData> expenses = ExpenseManager.Instance.GetAllExpenses();

        foreach (var expense in expenses)
        {
            if (expense.isMandatory || expense.isEnabled)
            {
                int cost = ExpenseManager.Instance.CalculateExpenseCost(expense, monthNumber);
                currentBalance -= cost;

                OnExpenseProcessed?.Invoke(expense, cost);
                OnBalanceUpdated?.Invoke(currentBalance);

                yield return new WaitForSeconds(delayBetweenExpenses);
            }
        }

        yield return new WaitForSeconds(delayBeforeFinish);

        // 3. Finalize
        bool isBankrupt = currentBalance < 0;

        // Check for deficit > 5000 (meaning balance is less than -5000)
        if (currentBalance < -5000)
        {
            int deficit = Mathf.Abs(currentBalance);
            OnMicroloanAvailable?.Invoke(deficit);
            Debug.Log($"Deficit {deficit} > 5000. Microloan available.");
        }

        OnSummaryFinished?.Invoke(currentBalance, isBankrupt);
        
        Debug.Log($"Summary Finished. Final Balance: {currentBalance}. Bankrupt: {isBankrupt}");
    }
}
