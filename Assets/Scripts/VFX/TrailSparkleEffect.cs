using UnityEngine;

public class TrailSparkleEffect : MonoBehaviour
{
    [Header("Sparkle Settings")]
    [SerializeField] private Color sparkleColor = new Color(0.4f, 1f, 1f, 1f);
    [SerializeField] private float sparkleSize = 0.05f;
    [SerializeField] private float sparkleLifetime = 0.3f;
    [SerializeField] private float sparkleSpeed = 2f;

    [Header("Emission")]
    [SerializeField] private float emissionRate = 20f;
    [SerializeField] private bool burstOnLaneChange = true;
    [SerializeField] private int burstCount = 10;

    private ParticleSystem sparkleParticles;

    void Start()
    {
        CreateParticleSystem();
    }

    void CreateParticleSystem()
    {
        GameObject psObject = new GameObject("TrailSparkles");
        psObject.transform.SetParent(transform);
        psObject.transform.localPosition = Vector3.zero;

        sparkleParticles = psObject.AddComponent<ParticleSystem>();

        var main = sparkleParticles.main;
        main.startColor = sparkleColor;
        main.startSize = sparkleSize;
        main.startLifetime = sparkleLifetime;
        main.startSpeed = sparkleSpeed;
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = sparkleParticles.emission;
        emission.rateOverTime = emissionRate;

        var shape = sparkleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f;

        var colorOverLifetime = sparkleParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(sparkleColor, 0f),
                new GradientColorKey(sparkleColor, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = sparkleParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var renderer = sparkleParticles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
        renderer.material.SetColor("_BaseColor", sparkleColor);
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", sparkleColor * 3f);
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }

    public void TriggerBurst()
    {
        if (burstOnLaneChange && sparkleParticles != null)
        {
            sparkleParticles.Emit(burstCount);
        }
    }

    public void SetEmissionRate(float rate)
    {
        if (sparkleParticles == null) return;

        var emission = sparkleParticles.emission;
        emission.rateOverTime = rate;
    }
}
