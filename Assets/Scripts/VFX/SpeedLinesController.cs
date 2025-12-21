using UnityEngine;

public class SpeedLinesController : MonoBehaviour
{
    [Header("Particle System")]
    [SerializeField] private ParticleSystem speedLinesParticles;

    [Header("Speed Settings")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float minSpeedForEffect = 15f;
    [SerializeField] private float maxSpeed = 30f;

    [Header("Emission Settings")]
    [SerializeField] private float minEmissionRate = 0f;
    [SerializeField] private float maxEmissionRate = 100f;

    private ParticleSystem.EmissionModule emissionModule;

    void Start()
    {
        if (speedLinesParticles != null)
        {
            emissionModule = speedLinesParticles.emission;
        }
    }

    void Update()
    {
        if (playerController == null || speedLinesParticles == null) return;

        float currentSpeed = playerController.GetCurrentSpeed();

        if (currentSpeed < minSpeedForEffect)
        {
            emissionModule.rateOverTime = 0f;
        }
        else
        {
            float speedRatio = Mathf.Clamp01((currentSpeed - minSpeedForEffect) / (maxSpeed - minSpeedForEffect));
            float targetEmission = Mathf.Lerp(minEmissionRate, maxEmissionRate, speedRatio);
            emissionModule.rateOverTime = targetEmission;
        }
    }
}
