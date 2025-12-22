using UnityEngine;
using TMPro;
using System.Collections;

public class MonthTransitionEffect : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI monthTransitionText;
    [SerializeField] private RectTransform panelTransform;

    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float scaleAnimationDuration = 0.6f;
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 endScale = new Vector3(1f, 1f, 1f);

    [Header("Visual Effects")]
    [SerializeField] private bool useTimeSlowEffect = true;
    [SerializeField] private float slowMotionTimeScale = 0.3f;
    [SerializeField] private float slowMotionDuration = 0.8f;

    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (panelTransform == null)
            panelTransform = GetComponent<RectTransform>();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void ShowTransition(int month, string monthName)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(TransitionSequence(month, monthName));
    }

    private IEnumerator TransitionSequence(int month, string monthName)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }

        if (monthTransitionText != null)
        {
            monthTransitionText.text = monthName;
        }

        if (panelTransform != null)
        {
            panelTransform.localScale = startScale;
        }

        if (useTimeSlowEffect)
        {
            StartCoroutine(SlowMotionEffect());
        }

        yield return StartCoroutine(FadeIn());

        yield return new WaitForSecondsRealtime(displayDuration);

        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeInDuration;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            }

            if (panelTransform != null && elapsed < scaleAnimationDuration)
            {
                float scaleT = elapsed / scaleAnimationDuration;
                float easedT = EaseOutBack(scaleT);
                panelTransform.localScale = Vector3.Lerp(startScale, endScale, easedT);
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (panelTransform != null)
        {
            panelTransform.localScale = endScale;
        }
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private IEnumerator SlowMotionEffect()
    {
        float originalTimeScale = Time.timeScale;
        float elapsed = 0f;
        float halfDuration = slowMotionDuration * 0.5f;

        while (elapsed < slowMotionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            if (elapsed < halfDuration)
            {
                float t = elapsed / halfDuration;
                Time.timeScale = Mathf.Lerp(originalTimeScale, slowMotionTimeScale, t);
            }
            else
            {
                float t = (elapsed - halfDuration) / halfDuration;
                Time.timeScale = Mathf.Lerp(slowMotionTimeScale, originalTimeScale, t);
            }

            yield return null;
        }

        Time.timeScale = originalTimeScale;
    }

    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
