using UnityEngine;

public class MoneyCollectible : MonoBehaviour
{
    [Header("Economy Settings")]
    [Tooltip("Amount to add to the monthly counter when collected")]
    [SerializeField] private int value = 10;

    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip collectSound;

    [Header("VFX Settings")]
    [SerializeField] private GameObject collectEffectPrefab;

    private Vector3 startPosition;
    private float bobOffset;

    void Start()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectMoney();
        }
    }

    void CollectMoney()
    {
        if (EconomicManager.Instance != null)
        {
            EconomicManager.Instance.AddMoney(value);
        }

        if (collectEffectPrefab != null)
        {
            Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
        }

        if (collectSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(collectSound);
        }

        Destroy(gameObject);
    }

    public void SetValue(int newValue)
    {
        value = newValue;
    }

    public int GetValue()
    {
        return value;
    }
}
