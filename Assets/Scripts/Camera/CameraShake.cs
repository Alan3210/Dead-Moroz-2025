using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    [SerializeField] private float defaultShakeDuration = 0.3f;
    [SerializeField] private float defaultShakeMagnitude = 0.2f;
    [SerializeField] private float dampingSpeed = 1.5f;

    private CameraFollow cameraFollow;
    private Coroutine shakeCoroutine;
    private bool isShaking = false;

    public Vector3 ShakeOffset { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        cameraFollow = GetComponent<CameraFollow>();
    }

    public void TriggerShake(float duration = -1f, float magnitude = -1f)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        float shakeDuration = duration > 0 ? duration : defaultShakeDuration;
        float shakeMagnitude = magnitude > 0 ? magnitude : defaultShakeMagnitude;

        shakeCoroutine = StartCoroutine(ShakeCoroutine(shakeDuration, shakeMagnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float percentComplete = elapsed / duration;
            float damper = 1f - Mathf.Clamp01(percentComplete * dampingSpeed);

            float xOffset = Random.Range(-1f, 1f) * magnitude * damper;
            float yOffset = Random.Range(-1f, 1f) * magnitude * damper;

            ShakeOffset = new Vector3(xOffset, yOffset, 0f);

            yield return null;
        }

        ShakeOffset = Vector3.zero;
        isShaking = false;
    }

    public bool IsShaking()
    {
        return isShaking;
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        ShakeOffset = Vector3.zero;
        isShaking = false;
    }
}
