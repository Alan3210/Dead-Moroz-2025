using UnityEngine;

public class EnhancedSpeedLines : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem speedLinesParticles;
    [SerializeField] private PlayerController playerController;

    [Header("Speed Thresholds")]
    [SerializeField] private float minSpeedForEffect = 5f;
    [SerializeField] private float maxSpeed = 25f;

    [Header("Emission")]
    [SerializeField] private float minEmissionRate = 5f;
    [SerializeField] private float maxEmissionRate = 150f;

    [Header("Visual Settings")]
    [SerializeField] private Color slowSpeedColor = new Color(0.5f, 0.8f, 1f, 0.3f);
    [SerializeField] private Color fastSpeedColor = new Color(0.2f, 1f, 1f, 0.8f);
    [SerializeField] private float minParticleSpeed = 10f;
    [SerializeField] private float maxParticleSpeed = 30f;

    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;
    private float currentIntensity = 0f;

    void Start()
    {
        InitializeParticleSystem();
    }

    void InitializeParticleSystem()
    {
        if (speedLinesParticles == null)
        {
            speedLinesParticles = GetComponent<ParticleSystem>();
        }

        if (speedLinesParticles != null)
        {
            emissionModule = speedLinesParticles.emission;
            mainModule = speedLinesParticles.main;

            ConfigureParticleSystem();
        }
        else
        {
            Debug.LogWarning("SpeedLines: ParticleSystem not found!");
        }
    }

    void ConfigureParticleSystem()
    {
        mainModule.startSpeed = new ParticleSystem.MinMaxCurve(minParticleSpeed, maxParticleSpeed);
        mainModule.startLifetime = 0.5f;
        mainModule.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        mainModule.maxParticles = 200;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;

        var shape = speedLinesParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        shape.radiusThickness = 1f;

        var velocityOverLifetime = speedLinesParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-20f, -40f);

        var colorOverLifetime = speedLinesParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = speedLinesParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = AnimationCurve.Linear(0f, 0.2f, 1f, 1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
    }

    void Update()
    {
        if (playerController == null || speedLinesParticles == null) return;

        UpdateSpeedLines();
    }

    void UpdateSpeedLines()
    {
        float currentSpeed = playerController.GetCurrentSpeed();

        float targetIntensity = 0f;

        if (currentSpeed >= minSpeedForEffect)
        {
            targetIntensity = Mathf.Clamp01((currentSpeed - minSpeedForEffect) / (maxSpeed - minSpeedForEffect));
        }

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 3f);

        float emissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, currentIntensity);
        emissionModule.rateOverTime = emissionRate;

        Color currentColor = Color.Lerp(slowSpeedColor, fastSpeedColor, currentIntensity);
        mainModule.startColor = currentColor;

        float particleSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, currentIntensity);
        mainModule.startSpeed = new ParticleSystem.MinMaxCurve(particleSpeed * 0.8f, particleSpeed);
    }

    public void Burst(int count = 20)
    {
        if (speedLinesParticles != null)
        {
            speedLinesParticles.Emit(count);
        }
    }
}
