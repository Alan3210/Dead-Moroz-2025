using UnityEngine;

public class MonthlyMoneyManager : MonoBehaviour
{
    public static MonthlyMoneyManager Instance { get; private set; }

    [Header("Money Containers")]
    [SerializeField] private GameObject[] monthMoneyContainers = new GameObject[12];

    private int currentActiveMonth = -1;

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
        DeactivateAllMoney();
    }

    public void ActivateMonthMoney(int monthNumber)
    {
        if (currentActiveMonth >= 0 && currentActiveMonth < monthMoneyContainers.Length)
        {
            if (monthMoneyContainers[currentActiveMonth] != null)
            {
                monthMoneyContainers[currentActiveMonth].SetActive(false);
            }
        }

        int monthIndex = monthNumber - 1;
        if (monthIndex >= 0 && monthIndex < monthMoneyContainers.Length)
        {
            if (monthMoneyContainers[monthIndex] != null)
            {
                monthMoneyContainers[monthIndex].SetActive(true);
                currentActiveMonth = monthIndex;
                Debug.Log($"💰 Activated money for Month {monthNumber}");
            }
        }
    }

    public void DeactivateAllMoney()
    {
        for (int i = 0; i < monthMoneyContainers.Length; i++)
        {
            if (monthMoneyContainers[i] != null)
            {
                monthMoneyContainers[i].SetActive(false);
            }
        }
        currentActiveMonth = -1;
    }
}
