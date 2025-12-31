using UnityEngine;

[CreateAssetMenu(fileName = "New Expense", menuName = "Economy/Expense Data")]
public class ExpenseData : ScriptableObject
{
    public string expenseName = "New Expense";
    public int baseCost = 100;
    public bool isMandatory = true;
    public bool isEnabled = true; // Tracks if the player has chosen to pay this (if optional)
}
