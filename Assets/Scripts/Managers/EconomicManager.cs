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
    [SerializeField] private float earnedMonthIncome = 0f;
    [SerializeField] private float currentMonthIncome = 0f;
    [SerializeField] private float totalLifetimeIncome = 0f;
    [SerializeField] private int currentMonth = 1;

    [Header("Loans")]
    private List<Loan> activeLoans = new List<Loan>();

    public float EarnedMonthIncome => earnedMonthIncome;
    public float CurrentMonthIncome => currentMonthIncome;
    public int CurrentMonth => currentMonth;
    public List<Loan> ActiveLoans => activeLoans;

    [ContextMenu("Test Banking Screen")]
    void TestBankingScreen()
    {
        earnedMonthIncome = 50000f;
        currentMonthIncome = 50000f;
        ShowMonthEndScreen();
    }

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

        InitializeEconomy();
    }

    private void InitializeEconomy()
    {
        earnedMonthIncome = 0f;
        currentMonthIncome = 0f;
        totalLifetimeIncome = 0f;
        currentMonth = 1;
        activeLoans.Clear();

        Debug.Log("💰 Economic system initialized");
    }


    public void AddMoney(float amount)
    {
        earnedMonthIncome += amount;
        currentMonthIncome += amount;
        totalLifetimeIncome += amount;

        Debug.Log($"💰 Collected {amount}₽ | Earned: {earnedMonthIncome:F0}₽ | Available: {currentMonthIncome:F0}₽");
    }

    public void ResetEconomy()
    {
        earnedMonthIncome = 0f;
        currentMonthIncome = 0f;
        totalLifetimeIncome = 0f;
        currentMonth = 1;
        activeLoans.Clear();

        Debug.Log("💰 Economy reset for new game");
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

        float balance = earnedMonthIncome - totalExpenses;
        bool canAfford = balance >= 0;
        bool needsLoan = balance < -LOAN_THRESHOLD;

        if (BankingScreenController.Instance != null)
        {
            BankingScreenController.Instance.ShowBankingScreen(
                income: earnedMonthIncome,
                expenses: allExpenses.ToArray(),
                balance: balance,
                monthNumber: currentMonth,
                canAffordExpenses: canAfford,
                showLoanButton: needsLoan
            );
        }
        else
        {
            Debug.LogError("❌ BankingScreenController.Instance is NULL! Checks:" +
                "\n1. Is BankingScreenCanvas GameObject ENABLED in hierarchy?" +
                "\n2. Does BankingScreenCanvas have BankingScreenController component?" +
                "\n3. Is BankingScreenController component ENABLED (checked)?" +
                "\n4. Check the Awake() method didn't fail (check for other errors above)");

            // Fallback: Continue game without banking screen
            if (Instance != null)
            {
                Instance.AdvanceToNextMonth();
            }
        }

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
        earnedMonthIncome = 0f;
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
