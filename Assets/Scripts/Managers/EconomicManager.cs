using UnityEngine;
using System.Collections.Generic;

public class EconomicManager : MonoBehaviour
{
    public static EconomicManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private MonthlyExpenses expenseConfig;

    [Header("Economic Constants")]
    private const float BASE_SALARY = 64637f;
    private const float LOAN_THRESHOLD = 5000f;

    [Header("Current State")]
    [SerializeField] private float currentMonthIncome = 0f;
    [SerializeField] private float totalLifetimeIncome = 0f;
    [SerializeField] private int currentMonth = 1;

    [Header("Loans")]
    private List<Loan> activeLoans = new List<Loan>();

    public float CurrentMonthIncome => currentMonthIncome;
    public int CurrentMonth => currentMonth;
    public List<Loan> ActiveLoans => activeLoans;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (expenseConfig == null)
        {
            Debug.LogError("EconomicManager: MonthlyExpenses configuration not assigned!");
        }
    }

    public void AddMoney(float amount)
    {
        currentMonthIncome += amount;
        totalLifetimeIncome += amount;

        Debug.Log($"💰 Collected {amount}₽ | Month total: {currentMonthIncome:F0}₽");
    }

    public void ShowMonthEndScreen()
    {
        ExpenseItem[] baseExpenses = expenseConfig.GetExpenses(currentMonth);
        List<ExpenseItem> allExpenses = new List<ExpenseItem>(baseExpenses);

        foreach (Loan loan in activeLoans)
        {
            allExpenses.Add(new ExpenseItem
            {
                name = "Проценты по займу",
                amount = loan.GetMonthlyInterest()
            });
        }

        float totalExpenses = 0f;
        foreach (ExpenseItem expense in allExpenses)
        {
            totalExpenses += expense.amount;
        }

        float balance = currentMonthIncome - totalExpenses;
        bool canAfford = balance >= 0;
        bool needsLoan = balance < -LOAN_THRESHOLD;

        Debug.Log("═══════════════════════════════════════════");
        Debug.Log($"<color=cyan>📊 MONTH {currentMonth} FINANCIAL SUMMARY</color>");
        Debug.Log("═══════════════════════════════════════════");
        Debug.Log($"<color=green>💵 Income: +{currentMonthIncome:F0}₽</color>");
        Debug.Log("<color=red>Expenses:</color>");

        foreach (ExpenseItem expense in allExpenses)
        {
            Debug.Log($"  • {expense.name}: -{expense.amount:F0}₽");
        }

        Debug.Log($"Total Expenses: -{totalExpenses:F0}₽");

        if (balance >= 0)
        {
            Debug.Log($"<color=green>✅ Balance: +{balance:F0}₽ - Живём пока</color>");
        }
        else if (needsLoan)
        {
            Debug.Log($"<color=red>❌ Deficit: {balance:F0}₽ - Нужен займ!</color>");
        }
        else
        {
            Debug.Log($"<color=yellow>⚠️ Deficit: {balance:F0}₽</color>");
        }

        Debug.Log("═══════════════════════════════════════════");
    }

    public void TakeLoan(float amount)
    {
        Loan newLoan = new Loan(amount, currentMonth);
        activeLoans.Add(newLoan);

        currentMonthIncome += amount;

        Debug.Log($"💳 Took microloan: {amount}₽ | Interest: {newLoan.GetMonthlyInterest():F0}₽ per month");
    }

    public void AdvanceToNextMonth()
    {
        currentMonth++;
        currentMonthIncome = 0f;

        Debug.Log($"📅 Advanced to Month {currentMonth}");
    }

    public float GetDeficit()
    {
        float totalExpenses = expenseConfig.GetTotalExpenses(currentMonth);

        foreach (Loan loan in activeLoans)
        {
            totalExpenses += loan.GetMonthlyInterest();
        }

        return currentMonthIncome - totalExpenses;
    }

    public bool IsInDebt()
    {
        return activeLoans.Count > 0;
    }

    public float GetTotalDebt()
    {
        float total = 0f;
        foreach (Loan loan in activeLoans)
        {
            total += loan.GetTotalDebt();
        }
        return total;
    }
}
