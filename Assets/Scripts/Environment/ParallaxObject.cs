using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField] private float baseParallaxMultiplier = 1.2f;
    [SerializeField] private bool scaleWithGameSpeed = true;

    private Transform playerTransform;
    private Vector3 startPosition;
    private float startPlayerZ;
    private bool isInitialized = false;
    private float currentMultiplier;

    public void Initialize(Transform player, float speedMultiplier, bool scaleSpeed = true)
    {
        playerTransform = player;
        baseParallaxMultiplier = speedMultiplier;
        scaleWithGameSpeed = scaleSpeed;
        currentMultiplier = baseParallaxMultiplier;
        startPosition = transform.position;
        startPlayerZ = player.position.z;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || playerTransform == null) return;

        if (scaleWithGameSpeed && GameManager.Instance != null)
        {
            float speedRatio = GameManager.Instance.CurrentSpeed / 10f;
            currentMultiplier = baseParallaxMultiplier * speedRatio;
        }
        else
        {
            currentMultiplier = baseParallaxMultiplier;
        }

        float playerDelta = playerTransform.position.z - startPlayerZ;
        float parallaxOffset = playerDelta * (currentMultiplier - 1f);

        Vector3 newPosition = startPosition;
        newPosition.z -= parallaxOffset;
        transform.position = newPosition;
    }

    public void ResetParallax()
    {
        if (playerTransform != null)
        {
            startPosition = transform.position;
            startPlayerZ = playerTransform.position.z;
        }
    }

    void OnDisable()
    {
        isInitialized = false;
    }
}
