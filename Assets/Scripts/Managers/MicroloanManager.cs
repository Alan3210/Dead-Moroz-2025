using System;
using UnityEngine;

public class MicroloanManager : MonoBehaviour
{
    public static MicroloanManager Instance { get; private set; }

    [Header("Loan Settings")]
    [SerializeField] private float interestRate = 0.15f; // 15%

    public bool IsInDebt { get; private set; }
    public event Action<bool> OnDebtStateChanged;

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
    /// Takes a loan to cover the specific deficit.
    /// Creates a new mandatory expense for the interest.
    /// </summary>
    /// <param name="deficitAmount">The amount of money missing (must be positive)</param>
    public void TakeLoan(int deficitAmount)
    {
        if (deficitAmount <= 0) return;

        // 1. Cover the deficit (Add money to economy)
        EconomyManager.Instance.AddMoney(deficitAmount);

        // 2. Calculate Interest
        int interestAmount = Mathf.RoundToInt(deficitAmount * interestRate);

        // 3. Create new Expense Data (Runtime instance)
        ExpenseData loanInterestExpense = ScriptableObject.CreateInstance<ExpenseData>();
        loanInterestExpense.expenseName = $"Loan Interest ({deficitAmount})";
        loanInterestExpense.baseCost = interestAmount;
        loanInterestExpense.isMandatory = true;
        loanInterestExpense.isEnabled = true;

        // 4. Add to Expense Manager for future months
        ExpenseManager.Instance.AddExpense(loanInterestExpense);

        // 5. Update Debt State
        if (!IsInDebt)
        {
            IsInDebt = true;
            OnDebtStateChanged?.Invoke(true);
        }

        Debug.Log($"Loan taken: {deficitAmount}. New Monthly Interest: {interestAmount}");
    }
}
