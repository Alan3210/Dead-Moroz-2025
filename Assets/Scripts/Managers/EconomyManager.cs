using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Economy Status")]
    [SerializeField] private int currentMonthMoney = 0;
    [SerializeField] private int totalRunMoney = 0;

    private int lastRecordedMonth = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddMoney(int amount)
    {
        CheckMonthTransition();

        int potentialTotal = currentMonthMoney + amount;
        
        // Clamp to Salary Cap
        if (potentialTotal > SalaryPolicy.MAX_MONTHLY_SALARY)
        {
            int allowedAmount = SalaryPolicy.MAX_MONTHLY_SALARY - currentMonthMoney;
            currentMonthMoney += allowedAmount;
            totalRunMoney += allowedAmount;
            // Optionally log that cap was reached
        }
        else
        {
            currentMonthMoney += amount;
            totalRunMoney += amount;
        }

        // Debug log for verification since UI is not implemented yet
        // Debug.Log($"Collected. Month: {currentMonthMoney}/{SalaryPolicy.MAX_MONTHLY_SALARY} | Total Run: {totalRunMoney}");
    }

    private void CheckMonthTransition()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.CurrentMonth != lastRecordedMonth)
            {
                ResetMonthMoney();
                lastRecordedMonth = GameManager.Instance.CurrentMonth;
            }
        }
    }

    public int GetMonthMoney()
    {
        return currentMonthMoney;
    }

    public int GetTotalMoney()
    {
        return totalRunMoney;
    }

    public void ResetMonthMoney()
    {
        currentMonthMoney = 0;
    }

    public void ResetAllMoney()
    {
        currentMonthMoney = 0;
        totalRunMoney = 0;
    }
}
