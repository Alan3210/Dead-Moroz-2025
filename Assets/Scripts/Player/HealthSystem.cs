using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDeath;

    private int currentHealth;

    public static HealthSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage = 1)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log($"Health: {currentHealth}/{maxHealth}");

        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount = 1)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    void Die()
    {
        Debug.Log("Player died!");
        OnDeath?.Invoke();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}
