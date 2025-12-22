using UnityEngine;

public class MonthVisualManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light directionalLight;
    [SerializeField] private MonthVisualSettings[] monthVisualSettings;


    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private bool enableTransitions = true;

    private int currentMonth = -1;
    private Coroutine transitionCoroutine;

    void Start()
    {
        if (directionalLight == null)
        {
            directionalLight = FindFirstObjectByType<Light>();
        }

        ApplyMonthVisuals(0, false);
    }

    public void ApplyMonthVisuals(int monthIndex, bool animated = true)
    {
        if (monthIndex < 0 || monthIndex >= monthVisualSettings.Length)
        {
            Debug.LogWarning($"Invalid month index: {monthIndex}");
            return;
        }

        if (monthIndex == currentMonth) return;

        currentMonth = monthIndex;
        MonthVisualSettings settings = monthVisualSettings[monthIndex];

        if (animated && enableTransitions)
        {
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }
            transitionCoroutine = StartCoroutine(TransitionVisuals(settings));
        }
        else
        {
            ApplyVisualsImmediately(settings);
        }
    }

    private void ApplyVisualsImmediately(MonthVisualSettings settings)
    {
        RenderSettings.ambientLight = settings.ambientColor;
        RenderSettings.ambientIntensity = settings.ambientIntensity;

        RenderSettings.fog = settings.useFog;
        RenderSettings.fogColor = settings.fogColor;
        RenderSettings.fogDensity = settings.fogDensity;

        if (directionalLight != null)
        {
            directionalLight.color = settings.lightColor;
            directionalLight.intensity = settings.lightIntensity;
        }

        if (settings.skyboxMaterial != null)
        {
            RenderSettings.skybox = settings.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }

        Debug.Log($"Applied visuals for {settings.monthName} (Month {settings.monthNumber})");
    }

    private System.Collections.IEnumerator TransitionVisuals(MonthVisualSettings targetSettings)
    {
        float elapsed = 0f;

        Color startAmbient = RenderSettings.ambientLight;
        float startAmbientIntensity = RenderSettings.ambientIntensity;
        float startFogDensity = RenderSettings.fogDensity;
        Color startFogColor = RenderSettings.fogColor;
        Color startLightColor = directionalLight != null ? directionalLight.color : Color.white;
        float startLightIntensity = directionalLight != null ? directionalLight.intensity : 1f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;

            RenderSettings.ambientLight = Color.Lerp(startAmbient, targetSettings.ambientColor, t);
            RenderSettings.ambientIntensity = Mathf.Lerp(startAmbientIntensity, targetSettings.ambientIntensity, t);
            RenderSettings.fogDensity = Mathf.Lerp(startFogDensity, targetSettings.fogDensity, t);
            RenderSettings.fogColor = Color.Lerp(startFogColor, targetSettings.fogColor, t);

            if (directionalLight != null)
            {
                directionalLight.color = Color.Lerp(startLightColor, targetSettings.lightColor, t);
                directionalLight.intensity = Mathf.Lerp(startLightIntensity, targetSettings.lightIntensity, t);
            }

            yield return null;
        }

        ApplyVisualsImmediately(targetSettings);
    }
}
