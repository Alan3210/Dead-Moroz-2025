using UnityEngine;

public class SleighAnimator : MonoBehaviour
{
    [Header("Tilt Settings")]
    [SerializeField] private float laneTiltAmount = 15f;
    [SerializeField] private float laneTiltSpeed = 8f;
    [SerializeField] private float speedTiltAmount = 10f;
    [SerializeField] private float maxSpeedForTilt = 30f;

    [Header("Steering Settings")]
    [SerializeField] private float steeringAmount = 10f;
    [SerializeField] private float steeringSpeed = 5f;

    [Header("Bobbing Settings")]
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private float bobbingSpeed = 3f;

    [Header("DedMoroz Settings")]
    [SerializeField] private float dedMorozLeanAmount = 10f;
    [SerializeField] private float dedMorozLeanSpeed = 8f;
    [SerializeField] private float dedMorozTiltXAmount = 5f;
    [SerializeField] private float dedMorozTiltXSpeed = 8f;
    [SerializeField] private float dedMorozTiltZAmount = 5f;
    [SerializeField] private float dedMorozTiltZSpeed = 8f;

    [Header("References")]
    [SerializeField] private Transform sleighVisual;
    [SerializeField] private Transform dedMorozTransform;
    [SerializeField] private PlayerController playerController;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Quaternion dedMorozOriginalRotation;
    private float currentTiltZ;
    private float targetTiltZ;
    private float currentSteeringY;
    private float targetSteeringY;
    private float currentDedMorozLean;
    private float targetDedMorozLean;
    private float currentDedMorozTiltX;
    private float targetDedMorozTiltX;
    private float currentDedMorozTiltZ;
    private float targetDedMorozTiltZ;
    private float bobbingOffset;
    private Vector3 lastPosition;

    void Start()
    {
        if (sleighVisual != null)
        {
            originalPosition = sleighVisual.localPosition;
            originalRotation = sleighVisual.localRotation;
        }

        if (dedMorozTransform != null)
        {
            dedMorozOriginalRotation = dedMorozTransform.localRotation;
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        if (sleighVisual == null || playerController == null) return;

        CalculateVelocityEffects();
        ApplyBobbing();
        ApplyRotations();
        ApplyDedMorozRotation();
    }

    void CalculateVelocityEffects()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        if (Mathf.Abs(velocity.x) > 0.1f)
        {
            targetTiltZ = -velocity.x * laneTiltAmount;
            targetSteeringY = velocity.x * steeringAmount;
            targetDedMorozLean = velocity.x * dedMorozLeanAmount;
            targetDedMorozTiltX = -Mathf.Abs(velocity.x) * dedMorozTiltXAmount;
            targetDedMorozTiltZ = -velocity.x * dedMorozTiltZAmount;
        }
        else
        {
            targetTiltZ = 0f;
            targetSteeringY = 0f;
            targetDedMorozLean = 0f;
            targetDedMorozTiltX = 0f;
            targetDedMorozTiltZ = 0f;
        }

        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, laneTiltSpeed * Time.deltaTime);
        currentSteeringY = Mathf.Lerp(currentSteeringY, targetSteeringY, steeringSpeed * Time.deltaTime);
        currentDedMorozLean = Mathf.Lerp(currentDedMorozLean, targetDedMorozLean, dedMorozLeanSpeed * Time.deltaTime);
        currentDedMorozTiltX = Mathf.Lerp(currentDedMorozTiltX, targetDedMorozTiltX, dedMorozTiltXSpeed * Time.deltaTime);
        currentDedMorozTiltZ = Mathf.Lerp(currentDedMorozTiltZ, targetDedMorozTiltZ, dedMorozTiltZSpeed * Time.deltaTime);
    }

    void ApplyBobbing()
    {
        bobbingOffset += Time.deltaTime * bobbingSpeed;
        float yOffset = Mathf.Sin(bobbingOffset) * bobbingAmount;

        Vector3 newPosition = originalPosition;
        newPosition.y += yOffset;
        sleighVisual.localPosition = newPosition;
    }

    void ApplyRotations()
    {
        float currentSpeed = playerController.GetCurrentSpeed();
        float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeedForTilt);
        float tiltX = -speedRatio * speedTiltAmount;

        Quaternion tiltRotation = Quaternion.Euler(tiltX, currentSteeringY, currentTiltZ);
        sleighVisual.localRotation = originalRotation * tiltRotation;
    }

    void ApplyDedMorozRotation()
    {
        if (dedMorozTransform == null) return;

        Quaternion dedMorozLean = Quaternion.Euler(currentDedMorozTiltX, currentDedMorozLean, currentDedMorozTiltZ);
        dedMorozTransform.localRotation = dedMorozOriginalRotation * dedMorozLean;
    }

    public void TriggerLandeImpact()
    {
        StartCoroutine(LandingImpactCoroutine());
    }

    private System.Collections.IEnumerator LandingImpactCoroutine()
    {
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float bounceAmount = Mathf.Sin(t * Mathf.PI) * 0.1f;

            Vector3 pos = originalPosition;
            pos.y -= bounceAmount;
            sleighVisual.localPosition = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        sleighVisual.localPosition = originalPosition;
    }
}
