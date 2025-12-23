using UnityEngine;

public class ObstaclePhysicsReaction : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float fallDuration = 1.5f;
    [SerializeField] private float pushForce = 3f;
    [SerializeField] private float torqueForce = 5f;

    [Header("Performance")]
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private float autoDestroyDelay = 2f;

    private Rigidbody rb;
    private Collider col;
    private bool hasBeenHit = false;
    private float hitTime;

    void Awake()
    {
        SetupComponents();
    }

    void SetupComponents()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;

        col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    public void ActivatePhysics(Vector3 hitDirection)
    {
        if (hasBeenHit || !usePhysics) return;

        hasBeenHit = true;
        hitTime = Time.time;

        if (col != null)
        {
            col.isTrigger = false;
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Vector3 forceDirection = (hitDirection + Vector3.up * 0.3f).normalized;
        rb.AddForce(forceDirection * pushForce, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
            Random.Range(-torqueForce, torqueForce),
            Random.Range(-torqueForce, torqueForce),
            Random.Range(-torqueForce, torqueForce)
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        Invoke(nameof(DeactivatePhysics), fallDuration);
        Invoke(nameof(DestroyObstacle), autoDestroyDelay);
    }

    void DeactivatePhysics()
    {
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void DestroyObstacle()
    {
        Destroy(gameObject);
    }

    void OnDisable()
    {
        CancelInvoke();
    }
}
