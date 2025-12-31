using UnityEngine;

public class MoneyCollectible : MonoBehaviour
{
    [Header("Economy Settings")]
    [Tooltip("Amount to add to the monthly counter when collected")]
    [SerializeField] private int value = 10;

    [Header("Visual Settings")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float floatAmplitude = 0.5f;
    [SerializeField] private float floatFrequency = 2f;

    private Vector3 initialPosition;
    private float timeOffset;

    private void Start()
    {
        initialPosition = transform.position;
        // Randomize start time so multiple collectibles don't move in perfect sync
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        // Rotate around Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Simple Sine wave floating
        float newY = initialPosition.y + Mathf.Sin((Time.time + timeOffset) * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.AddMoney(value);
        }
        else
        {
            Debug.LogWarning("EconomyManager instance is missing in the scene!");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMoneyPickupSound();
        }

        // Potential for visual effects here
        
        Destroy(gameObject);
    }
}