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

    [Header("Root Rotation Settings")]
    [SerializeField] private bool enableRootRotation = true;
    [SerializeField] private float rootRotationAmount = 8f;
    [SerializeField] private float rootRotationSpeed = 6f;

    [Header("Hover Settings")]
    [SerializeField] private float hoverHeight = 0.4f;
    [SerializeField] private float hoverSpeed = 8f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Bobbing Settings")]
    [SerializeField] private float bobbingAmount = 0.03f;
    [SerializeField] private float bobbingSpeed = 2f;

    [Header("DedMoroz Settings")]
    [SerializeField] private float dedMorozLeanAmount = 10f;
    [SerializeField] private float dedMorozLeanSpeed = 8f;
    [SerializeField] private float dedMorozTiltXAmount = 5f;
    [SerializeField] private float dedMorozTiltXSpeed = 8f;
    [SerializeField] private float dedMorozTiltZAmount = 5f;
    [SerializeField] private float dedMorozTiltZSpeed = 8f;

    [Header("Hat Tip Settings")]
    [SerializeField] private Transform[] hatBones;
    [SerializeField] private float hatSwayAmount = 20f;
    [SerializeField] private float hatSwaySpeed = 3f;
    [SerializeField] private float hatSwayMultiplier = 1.5f;

    [Header("References")]
    [SerializeField] private Transform sleighVisual;
    [SerializeField] private Transform dedMorozTransform;
    [SerializeField] private PlayerController playerController;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Quaternion rootOriginalRotation;
    private Quaternion dedMorozOriginalRotation;
    private Quaternion[] hatBonesOriginalRotations;
    private float currentTiltZ;
    private float targetTiltZ;
    private float currentSteeringY;
    private float targetSteeringY;
    private float currentRootRotationZ;
    private float targetRootRotationZ;
    private float currentDedMorozLean;
    private float targetDedMorozLean;
    private float currentDedMorozTiltX;
    private float targetDedMorozTiltX;
    private float currentDedMorozTiltZ;
    private float targetDedMorozTiltZ;
    private float currentHatSway;
    private float targetHatSway;
    private float bobbingOffset;
    private Vector3 lastPosition;
    private float currentHoverHeight;

    void Start()
    {
        rootOriginalRotation = transform.localRotation;

        if (sleighVisual != null)
        {
            originalPosition = sleighVisual.localPosition;
            originalRotation = sleighVisual.localRotation;
        }

        if (dedMorozTransform != null)
        {
            dedMorozOriginalRotation = dedMorozTransform.localRotation;
        }

        if (hatBones != null && hatBones.Length > 0)
        {
            hatBonesOriginalRotations = new Quaternion[hatBones.Length];
            for (int i = 0; i < hatBones.Length; i++)
            {
                if (hatBones[i] != null)
                {
                    hatBonesOriginalRotations[i] = hatBones[i].localRotation;
                }
            }
        }

        lastPosition = transform.position;
        currentHoverHeight = hoverHeight;
    }

    void Update()
    {
        if (sleighVisual == null || playerController == null) return;

        CalculateVelocityEffects();
        ApplyRootRotation();
        ApplyHoverAndBobbing();
        ApplyRotations();
        ApplyDedMorozRotation();
        ApplyHatAnimation();
    }

    void CalculateVelocityEffects()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        if (Mathf.Abs(velocity.x) > 0.1f)
        {
            targetTiltZ = -velocity.x * laneTiltAmount;
            targetSteeringY = velocity.x * steeringAmount;
            targetRootRotationZ = -velocity.x * rootRotationAmount;
            targetDedMorozLean = velocity.x * dedMorozLeanAmount;
            targetDedMorozTiltX = -Mathf.Abs(velocity.x) * dedMorozTiltXAmount;
            targetDedMorozTiltZ = -velocity.x * dedMorozTiltZAmount;
            targetHatSway = velocity.x * hatSwayAmount;
        }
        else
        {
            targetTiltZ = 0f;
            targetSteeringY = 0f;
            targetRootRotationZ = 0f;
            targetDedMorozLean = 0f;
            targetDedMorozTiltX = 0f;
            targetDedMorozTiltZ = 0f;
            targetHatSway = 0f;
        }

        currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, laneTiltSpeed * Time.deltaTime);
        currentSteeringY = Mathf.Lerp(currentSteeringY, targetSteeringY, steeringSpeed * Time.deltaTime);
        currentRootRotationZ = Mathf.Lerp(currentRootRotationZ, targetRootRotationZ, rootRotationSpeed * Time.deltaTime);
        currentDedMorozLean = Mathf.Lerp(currentDedMorozLean, targetDedMorozLean, dedMorozLeanSpeed * Time.deltaTime);
        currentDedMorozTiltX = Mathf.Lerp(currentDedMorozTiltX, targetDedMorozTiltX, dedMorozTiltXSpeed * Time.deltaTime);
        currentDedMorozTiltZ = Mathf.Lerp(currentDedMorozTiltZ, targetDedMorozTiltZ, dedMorozTiltZSpeed * Time.deltaTime);
        currentHatSway = Mathf.Lerp(currentHatSway, targetHatSway, hatSwaySpeed * Time.deltaTime);
    }

    void ApplyRootRotation()
    {
        if (!enableRootRotation) return;

        Quaternion rootRotation = Quaternion.Euler(0f, 0f, currentRootRotationZ);
        transform.localRotation = rootOriginalRotation * rootRotation;
    }

    void ApplyHoverAndBobbing()
    {
        float targetHover = GetGroundHoverHeight();
        currentHoverHeight = Mathf.Lerp(currentHoverHeight, targetHover, hoverSpeed * Time.deltaTime);

        bobbingOffset += Time.deltaTime * bobbingSpeed;
        float yBobbing = Mathf.Sin(bobbingOffset) * bobbingAmount;

        Vector3 newPosition = originalPosition;
        newPosition.y = currentHoverHeight + yBobbing;
        sleighVisual.localPosition = newPosition;
    }

    float GetGroundHoverHeight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, hoverHeight * 3f, groundLayer))
        {
            return hoverHeight;
        }
        return hoverHeight;
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

    void ApplyHatAnimation()
    {
        if (hatBones == null || hatBones.Length == 0 || hatBonesOriginalRotations == null) return;

        for (int i = 0; i < hatBones.Length; i++)
        {
            if (hatBones[i] == null) continue;

            float progressiveMultiplier = Mathf.Pow(hatSwayMultiplier, i);
            float swayAmount = currentHatSway * progressiveMultiplier;
            float bobbingSway = Mathf.Sin(bobbingOffset * 2f + i * 0.5f) * (hatSwayAmount * 0.15f * progressiveMultiplier);

            float totalSway = swayAmount + bobbingSway;
            Quaternion hatRotation = Quaternion.Euler(0f, 0f, totalSway);
            hatBones[i].localRotation = hatBonesOriginalRotations[i] * hatRotation;
        }
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
