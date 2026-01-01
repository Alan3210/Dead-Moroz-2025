using UnityEngine;

[System.Serializable]
public class Loan
{
    public float principal;
    public int monthTaken;
    public float interestRate = 0.15f;

    public Loan(float amount, int month)
    {
        principal = amount;
        monthTaken = month;
    }

    public float GetMonthlyInterest()
    {
        return principal * interestRate;
    }

    public float GetTotalDebt()
    {
        return principal + GetMonthlyInterest();
    }
}
