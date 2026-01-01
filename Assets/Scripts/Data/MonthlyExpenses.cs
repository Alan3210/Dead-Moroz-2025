using UnityEngine;

[CreateAssetMenu(fileName = "MonthlyExpenses", menuName = "Dead Moroz/Monthly Expenses")]
public class MonthlyExpenses : ScriptableObject
{
    [Header("Base Monthly Expenses (Month 1)")]
    [Tooltip("Аренда студии в Бирюлёво")]
    public float rent = 30000f;

    [Tooltip("Еда (макарошки)")]
    public float food = 20000f;

    [Tooltip("Тройка/Метро")]
    public float transport = 4000f;

    [Tooltip("Связь (с VPN)")]
    public float internet = 1000f;

    [Tooltip("Мыльно-рыльное")]
    public float hygiene = 5000f;

    [Tooltip("Лекарства маме")]
    public float medicine = 4637f;

    [Header("Economic Settings")]
    [Tooltip("Inflation rate per month (1% = 0.01)")]
    public float inflationRate = 0.01f;

    public ExpenseItem[] GetExpenses(int monthNumber)
    {
        float multiplier = Mathf.Pow(1f + inflationRate, monthNumber - 1);

        return new ExpenseItem[]
        {
            new ExpenseItem { name = "Аренда студии в Бирюлёво", amount = rent * multiplier },
            new ExpenseItem { name = "Еда (макарошки)", amount = food * multiplier },
            new ExpenseItem { name = "Тройка/Метро", amount = transport * multiplier },
            new ExpenseItem { name = "Связь (с VPN)", amount = internet * multiplier },
            new ExpenseItem { name = "Мыльно-рыльное", amount = hygiene * multiplier },
            new ExpenseItem { name = "Лекарства маме", amount = medicine * multiplier }
        };
    }

    public float GetTotalExpenses(int monthNumber)
    {
        float baseTotal = rent + food + transport + internet + hygiene + medicine;
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
