using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class BankingScreenController : MonoBehaviour
{
    public static BankingScreenController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject bankingPanel;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI incomeValueText;
    [SerializeField] private TextMeshProUGUI balanceValueText;
    [SerializeField] private Transform expensesContainer;
    [SerializeField] private GameObject expenseLineItemPrefab;
    [SerializeField] private Button loanButton;
    [SerializeField] private Button continueButton;

    [Header("Animation Settings")]
    [SerializeField] private float lineDelay = 0.3f;
    [SerializeField] private float slowMotionTimeScale = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color positiveColor = new Color(0f, 1f, 0f);
    [SerializeField] private Color negativeColor = new Color(1f, 0.27f, 0.27f);
    [SerializeField] private Color neutralColor = Color.white;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip moneyCountSFX;
    [SerializeField] private AudioClip expenseLineSFX;
    [SerializeField] private AudioClip balancePositiveSFX;
    [SerializeField] private AudioClip balanceNegativeSFX;
    [SerializeField] private AudioClip loanTakenSFX;
    [SerializeField] private AudioClip buttonClickSFX;

    [Header("Animation")]
    [SerializeField] private Animator panelAnimator;

    [SerializeField] private Animator backgroundOverlayAnimator;


    private bool isShowingScreen = false;
    private float currentDeficit = 0f;
    private float previousTimeScale = 1f;

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
        if (bankingPanel != null)
        {
            bankingPanel.SetActive(false);
        }

        if (backgroundOverlayAnimator != null)
        {
            backgroundOverlayAnimator.gameObject.SetActive(false);
        }

        if (loanButton != null)
        {
            loanButton.onClick.AddListener(OnLoanButtonClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
    }


    public void ShowBankingScreen(float income, ExpenseItem[] expenses, float balance, int monthNumber, bool canAffordExpenses, bool showLoanButton)
    {
        if (isShowingScreen) return;

        currentDeficit = Mathf.Abs(balance);

        // Ensure overlay is visible
        if (backgroundOverlayAnimator != null)
        {
            backgroundOverlayAnimator.gameObject.SetActive(true);
        }

        bankingPanel.SetActive(true);

        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("Show");
        }

        if (backgroundOverlayAnimator != null)
        {
            backgroundOverlayAnimator.SetTrigger("Show");
        }


        isShowingScreen = true;

        previousTimeScale = Time.timeScale;
        Time.timeScale = slowMotionTimeScale;

        float earnedIncome = EconomicManager.Instance != null ? EconomicManager.Instance.EarnedMonthIncome : income;
        StartCoroutine(AnimateBankingScreen(earnedIncome, expenses, balance, monthNumber, showLoanButton));
    }

    IEnumerator AnimateBankingScreen(float income, ExpenseItem[] expenses, float balance, int monthNumber, bool showLoanButton)
    {
        ClearExpenseLines();

        continueButton.gameObject.SetActive(false);
        loanButton.gameObject.SetActive(false);

        headerText.text = $"РАСЧЁТ ЗА МЕСЯЦ {monthNumber}";

        yield return new WaitForSeconds(0.5f);

        incomeValueText.text = $"+{income:F0} руб";
        incomeValueText.color = positiveColor;

        if (AudioManager.Instance != null && moneyCountSFX != null)
        {
            AudioManager.Instance.PlaySFX(moneyCountSFX);
        }

        yield return new WaitForSeconds(lineDelay);

        float totalExpenses = 0f;
        foreach (ExpenseItem expense in expenses)
        {
            GameObject lineItem = Instantiate(expenseLineItemPrefab, expensesContainer);

            TextMeshProUGUI nameText = lineItem.transform.Find("ExpenseNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI amountText = lineItem.transform.Find("ExpenseAmountText").GetComponent<TextMeshProUGUI>();

            nameText.text = expense.name;
            amountText.text = $"-{expense.amount:F0} руб";
            amountText.color = negativeColor;

            totalExpenses += expense.amount;

            if (AudioManager.Instance != null && expenseLineSFX != null)
            {
                AudioManager.Instance.PlaySFX(expenseLineSFX);
            }

            yield return new WaitForSeconds(lineDelay);
        }

        yield return new WaitForSeconds(0.5f);

        if (balance >= 0)
        {
            balanceValueText.text = $"+{balance:F0} руб";
            balanceValueText.color = positiveColor;

            if (AudioManager.Instance != null && balancePositiveSFX != null)
            {
                AudioManager.Instance.PlaySFX(balancePositiveSFX);
            }
        }
        else
        {
            balanceValueText.text = $"{balance:F0} руб";
            balanceValueText.color = negativeColor;

            if (AudioManager.Instance != null && balanceNegativeSFX != null)
            {
                AudioManager.Instance.PlaySFX(balanceNegativeSFX);
            }
        }

        yield return new WaitForSeconds(1f);

        if (showLoanButton)
        {
            loanButton.gameObject.SetActive(true);

            TextMeshProUGUI loanButtonText = loanButton.GetComponentInChildren<TextMeshProUGUI>();
            if (loanButtonText != null)
            {
                float loanAmount = Mathf.Ceil(currentDeficit / 1000f) * 1000f;
                loanButtonText.text = $"ВЗЯТЬ ЗАЙМ {loanAmount:F0} руб\n(15% в месяц)";
            }
        }

        continueButton.gameObject.SetActive(true);
    }

    void OnLoanButtonClicked()
    {
        if (EconomicManager.Instance == null) return;

        float loanAmount = Mathf.Ceil(currentDeficit / 1000f) * 1000f;

        EconomicManager.Instance.TakeLoan(loanAmount);

        if (AudioManager.Instance != null && loanTakenSFX != null)
        {
            AudioManager.Instance.PlaySFX(loanTakenSFX);
        }

        loanButton.gameObject.SetActive(false);

        float newBalance = EconomicManager.Instance.GetDeficit();
        if (newBalance >= 0)
        {
            balanceValueText.text = $"+{newBalance:F0} руб";
            balanceValueText.color = positiveColor;
        }
        else
        {
            balanceValueText.text = $"{newBalance:F0} руб";
            balanceValueText.color = negativeColor;
        }

        Debug.Log($"💳 Microloan taken: {loanAmount:F0} руб");
    }

    void OnContinueButtonClicked()
    {
        if (AudioManager.Instance != null && buttonClickSFX != null)
        {
            AudioManager.Instance.PlaySFX(buttonClickSFX);
        }

        HideBankingScreen();

        if (EconomicManager.Instance != null)
        {
            EconomicManager.Instance.AdvanceToNextMonth();
        }

        if (backgroundOverlayAnimator != null)
        {
            backgroundOverlayAnimator.SetTrigger("Hide");
        }

    }

    public void HideBankingScreen()
    {
        if (panelAnimator != null)
        {
            panelAnimator.SetTrigger("Hide");
        }

        if (backgroundOverlayAnimator != null)
        {
            backgroundOverlayAnimator.SetTrigger("Hide");
        }

        isShowingScreen = false;
        Time.timeScale = previousTimeScale;

        StartCoroutine(DisablePanelAfterAnimation(0.4f));
    }


    void ClearExpenseLines()
    {
        foreach (Transform child in expensesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public bool IsShowing()
    {
        return isShowingScreen;
    }

    private IEnumerator DisablePanelAfterAnimation(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (bankingPanel != null)
        {
            bankingPanel.SetActive(false);
        }

        if (backgroundOverlayAnimator != null)
        {
            backgroundOverlayAnimator.gameObject.SetActive(false);
        }
    }


}
