using UnityEngine;

public class DynamicSleighTrails : MonoBehaviour
{
    [Header("Trail References")]
    [SerializeField] private TrailRenderer leftTrail;
    [SerializeField] private TrailRenderer rightTrail;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    [Header("Width Animation")]
    [SerializeField] private float minWidth = 0.15f;
    [SerializeField] private float maxWidth = 0.4f;
    [SerializeField] private float widthPulseSpeed = 3f;
    [SerializeField] private bool enableSpeedBasedWidth = true;
    [SerializeField] private float speedWidthMultiplier = 0.02f;

    [Header("Color Animation")]
    [SerializeField] private Color baseColor = new Color(0.19f, 0.84f, 0.88f, 1f);
    [SerializeField] private Color accentColor = new Color(0.4f, 1f, 1f, 1f);
    [SerializeField] private float colorPulseSpeed = 2f;
    [SerializeField] private bool enableLaneChangeFlash = true;
    [SerializeField] private float flashIntensity = 2f;
    [SerializeField] private float flashDuration = 0.3f;

    [Header("Emission Intensity")]
    [SerializeField] private float minEmission = 2f;
    [SerializeField] private float maxEmission = 5f;
    [SerializeField] private float emissionPulseSpeed = 2.5f;

    [Header("Trail Time")]
    [SerializeField] private float minTrailTime = 0.2f;
    [SerializeField] private float maxTrailTime = 0.5f;
    [SerializeField] private bool enableSpeedBasedTime = true;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem leftSparkles;
    [SerializeField] private ParticleSystem rightSparkles;
    [SerializeField] private bool enableSparkles = true;
    [SerializeField] private float sparkleEmissionRate = 20f;

    private Material leftTrailMaterial;
    private Material rightTrailMaterial;
    private float pulseOffset;
    private float currentFlashIntensity = 0f;
    private Vector3 lastPosition;
    private float lateralVelocity = 0f;
    private AnimationCurve widthCurve;

    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        InitializeTrails();
        InitializeMaterials();
        InitializeWidthCurve();
        lastPosition = transform.position;
    }

    void InitializeTrails()
    {
        if (leftTrail == null)
        {
            Transform leftPoint = transform.Find("HoverPoint_Left/SleighTrail_Left");
            if (leftPoint != null)
                leftTrail = leftPoint.GetComponent<TrailRenderer>();
        }

        if (rightTrail == null)
        {
            Transform rightPoint = transform.Find("HoverPoint_Right");
            if (rightPoint != null)
            {
                TrailRenderer[] trails = rightPoint.GetComponentsInChildren<TrailRenderer>();
                if (trails.Length > 0)
                    rightTrail = trails[0];
            }
        }
    }

    void InitializeMaterials()
    {
        if (leftTrail != null)
        {
            leftTrailMaterial = new Material(leftTrail.material);
            leftTrail.material = leftTrailMaterial;
        }

        if (rightTrail != null)
        {
            rightTrailMaterial = new Material(rightTrail.material);
            rightTrail.material = rightTrailMaterial;
        }
    }

    void InitializeWidthCurve()
    {
        widthCurve = new AnimationCurve();
        widthCurve.AddKey(new Keyframe(0f, 1f, 0f, -1f));
        widthCurve.AddKey(new Keyframe(1f, 0f, -1f, 0f));
    }


    void Update()
    {
        if (playerController == null) return;

        CalculateLateralVelocity();
        UpdatePulseOffset();
        UpdateTrailWidth();
        UpdateTrailColor();
        UpdateTrailTime();
        UpdateFlashEffect();
        UpdateSparkles();
    }

    void CalculateLateralVelocity()
    {
        Vector3 currentPosition = transform.position;
        Vector3 velocity = (currentPosition - lastPosition) / Time.deltaTime;
        lateralVelocity = Mathf.Abs(velocity.x);
        lastPosition = currentPosition;
    }

    void UpdatePulseOffset()
    {
        pulseOffset += Time.deltaTime;
    }

    void UpdateTrailWidth()
    {
        float basePulse = Mathf.Sin(pulseOffset * widthPulseSpeed) * 0.5f + 0.5f;
        float pulseWidth = Mathf.Lerp(minWidth, maxWidth, basePulse);

        float speedBonus = 0f;
        if (enableSpeedBasedWidth && playerController != null)
        {
            float speed = playerController.GetCurrentSpeed();
            speedBonus = speed * speedWidthMultiplier;
        }

        float lateralBonus = lateralVelocity * 0.05f;

        float finalWidth = pulseWidth + speedBonus + lateralBonus;

        ApplyWidthToTrail(leftTrail, finalWidth);
        ApplyWidthToTrail(rightTrail, finalWidth);
    }

    void ApplyWidthToTrail(TrailRenderer trail, float width)
    {
        if (trail == null) return;

        trail.widthCurve = widthCurve;
        trail.widthMultiplier = width;
    }

    void UpdateTrailColor()
    {
        float colorPulse = Mathf.Sin(pulseOffset * colorPulseSpeed) * 0.5f + 0.5f;
        Color currentColor = Color.Lerp(baseColor, accentColor, colorPulse);

        float emissionPulse = Mathf.Sin(pulseOffset * emissionPulseSpeed) * 0.5f + 0.5f;
        float emissionIntensity = Mathf.Lerp(minEmission, maxEmission, emissionPulse);

        emissionIntensity += currentFlashIntensity;

        if (lateralVelocity > 0.5f)
        {
            emissionIntensity += lateralVelocity * 0.5f;
        }

        Color finalEmission = currentColor * emissionIntensity;

        ApplyColorToMaterial(leftTrailMaterial, currentColor, finalEmission);
        ApplyColorToMaterial(rightTrailMaterial, currentColor, finalEmission);
    }

    void ApplyColorToMaterial(Material mat, Color baseCol, Color emission)
    {
        if (mat == null) return;

        mat.SetColor("_BaseColor", baseCol);
        mat.SetColor(EmissionColorProperty, emission);
    }

    void UpdateTrailTime()
    {
        if (!enableSpeedBasedTime || playerController == null) return;

        float speed = playerController.GetCurrentSpeed();
        float normalizedSpeed = Mathf.Clamp01(speed / 30f);
        float trailTime = Mathf.Lerp(minTrailTime, maxTrailTime, normalizedSpeed);

        if (leftTrail != null)
            leftTrail.time = trailTime;
        if (rightTrail != null)
            rightTrail.time = trailTime;
    }

    void UpdateFlashEffect()
    {
        if (currentFlashIntensity > 0f)
        {
            currentFlashIntensity -= Time.deltaTime * (flashIntensity / flashDuration);
            currentFlashIntensity = Mathf.Max(0f, currentFlashIntensity);
        }
    }

    void UpdateSparkles()
    {
        if (!enableSparkles) return;

        float speed = playerController != null ? playerController.GetCurrentSpeed() : 0f;
        float emissionRate = Mathf.Lerp(0f, sparkleEmissionRate, speed / 30f);

        UpdateParticleEmission(leftSparkles, emissionRate);
        UpdateParticleEmission(rightSparkles, emissionRate);
    }

    void UpdateParticleEmission(ParticleSystem ps, float rate)
    {
        if (ps == null) return;

        var emission = ps.emission;
        emission.rateOverTime = rate;
    }

    public void TriggerLaneChangeFlash()
    {
        if (enableLaneChangeFlash)
        {
            currentFlashIntensity = flashIntensity;
        }
    }

    public void SetTrailsEnabled(bool enabled)
    {
        if (leftTrail != null)
            leftTrail.emitting = enabled;
        if (rightTrail != null)
            rightTrail.emitting = enabled;
    }
}
