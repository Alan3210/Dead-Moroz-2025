using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VFX
{
    [RequireComponent(typeof(Volume))]
    public class SpeedPostProcessing : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;

        [Header("Speed Settings")]
        [Tooltip("Speed at which effects start increasing")]
        [SerializeField] private float minSpeed = 5f;
        [Tooltip("Speed at which effects reach maximum intensity")]
        [SerializeField] private float maxSpeed = 25f;

        [Header("Vignette")]
        [SerializeField] private bool enableVignette = true;
        [Range(0f, 1f)] [SerializeField] private float minVignette = 0.2f;
        [Range(0f, 1f)] [SerializeField] private float maxVignette = 0.45f;

        [Header("Chromatic Aberration")]
        [SerializeField] private bool enableAberration = true;
        [Range(0f, 1f)] [SerializeField] private float minAberration = 0f;
        [Range(0f, 1f)] [SerializeField] private float maxAberration = 0.8f;

        [Header("Color Adjustments (Contrast)")]
        [SerializeField] private bool enableColorAdj = false;
        [Range(-100f, 100f)] [SerializeField] private float minContrast = 0f;
        [Range(-100f, 100f)] [SerializeField] private float maxContrast = 20f;

        [Header("Film Grain")]
        [SerializeField] private bool enableFilmGrain = false;
        [Range(0f, 1f)] [SerializeField] private float minGrain = 0f;
        [Range(0f, 1f)] [SerializeField] private float maxGrain = 0.5f;

        [Header("Depth Of Field (Focus Distance)")]
        [SerializeField] private bool enableDOF = false;
        [SerializeField] private float minFocusDist = 10f;
        [SerializeField] private float maxFocusDist = 5f;

        [Header("Smoothing")]
        [SerializeField] private float smoothTime = 0.2f;

        private Volume volume;
        private Vignette vignette;
        private ChromaticAberration aberration;
        private ColorAdjustments colorAdj;
        private FilmGrain filmGrain;
        private DepthOfField dof;
        private float currentSpeedFactor;
        private float velocityHelper;

        void Start()
        {
            volume = GetComponent<Volume>();

            // Try to find PlayerController if not assigned
            if (playerController == null)
            {
                playerController = FindAnyObjectByType<PlayerController>();
                if (playerController == null)
                {
                    Debug.LogWarning("SpeedPostProcessing: PlayerController not found!");
                    enabled = false;
                    return;
                }
            }

            // Get Overrides from Volume Profile
            if (!volume.profile.TryGet(out vignette) && enableVignette) Debug.LogWarning("SpeedPostProcessing: Vignette missing.");
            if (!volume.profile.TryGet(out aberration) && enableAberration) Debug.LogWarning("SpeedPostProcessing: Chromatic Aberration missing.");
            if (!volume.profile.TryGet(out colorAdj) && enableColorAdj) Debug.LogWarning("SpeedPostProcessing: Color Adjustments missing.");
            if (!volume.profile.TryGet(out filmGrain) && enableFilmGrain) Debug.LogWarning("SpeedPostProcessing: Film Grain missing.");
            if (!volume.profile.TryGet(out dof) && enableDOF) Debug.LogWarning("SpeedPostProcessing: Depth of Field missing.");
        }

        void Update()
        {
            if (playerController == null) return;

            // Calculate Speed Factor (0 to 1)
            float speed = playerController.GetCurrentSpeed();
            float targetFactor = Mathf.InverseLerp(minSpeed, maxSpeed, speed);

            // Smooth the transition
            currentSpeedFactor = Mathf.SmoothDamp(currentSpeedFactor, targetFactor, ref velocityHelper, smoothTime);

            // Apply Effects
            ApplyVignette(currentSpeedFactor);
            ApplyAberration(currentSpeedFactor);
            ApplyColorAdj(currentSpeedFactor);
            ApplyFilmGrain(currentSpeedFactor);
            ApplyDOF(currentSpeedFactor);
        }

        void ApplyVignette(float factor)
        {
            if (!enableVignette || vignette == null) return;
            float intensity = Mathf.Lerp(minVignette, maxVignette, factor);
            vignette.intensity.Override(intensity);
        }

        void ApplyAberration(float factor)
        {
            if (!enableAberration || aberration == null) return;
            float intensity = Mathf.Lerp(minAberration, maxAberration, factor);
            aberration.intensity.Override(intensity);
        }

        void ApplyColorAdj(float factor)
        {
            if (!enableColorAdj || colorAdj == null) return;
            float contrast = Mathf.Lerp(minContrast, maxContrast, factor);
            colorAdj.contrast.Override(contrast);
        }

        void ApplyFilmGrain(float factor)
        {
            if (!enableFilmGrain || filmGrain == null) return;
            float intensity = Mathf.Lerp(minGrain, maxGrain, factor);
            filmGrain.intensity.Override(intensity);
        }

        void ApplyDOF(float factor)
        {
            if (!enableDOF || dof == null) return;
            float focus = Mathf.Lerp(minFocusDist, maxFocusDist, factor);
            dof.focusDistance.Override(focus);
        }
    }
}
