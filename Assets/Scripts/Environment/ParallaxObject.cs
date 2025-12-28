using UnityEngine;

public class ParallaxObject : MonoBehaviour
{
    [Header("Parallax Settings")]
    [SerializeField] private float parallaxSpeedMultiplier = 1.2f;

    private Transform playerTransform;
    private Vector3 startPosition;
    private float startPlayerZ;
    private bool isInitialized = false;

    public void Initialize(Transform player, float speedMultiplier)
    {
        playerTransform = player;
        parallaxSpeedMultiplier = speedMultiplier;
        startPosition = transform.position;
        startPlayerZ = player.position.z;
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized || playerTransform == null) return;

        float playerDelta = playerTransform.position.z - startPlayerZ;
        float parallaxOffset = playerDelta * (parallaxSpeedMultiplier - 1f);

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
