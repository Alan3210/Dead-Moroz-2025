using UnityEngine;

public class TextScreenPhysics : MonoBehaviour
{
    public enum ReactionMode
    {
        TiltAndFall,
        PushBack,
        SpinAway,
        Dissolve
    }

    private float fallSpeed = 5f;
    private float tiltAngle = 90f;
    private float pushBackDistance = 2f;
    private float reactionDuration = 1.0f;
    private ReactionMode reactionMode = ReactionMode.TiltAndFall;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private bool hasReacted = false;
    private float reactionStartTime;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvas = GetComponent<Canvas>();
        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void SetPhysicsSettings(ReactionMode mode, float fall, float tilt, float pushBack, float duration)
    {
        reactionMode = mode;
        fallSpeed = fall;
        tiltAngle = tilt;
        pushBackDistance = pushBack;
        reactionDuration = duration;
    }

    void Update()
    {
        if (hasReacted)
        {
            float elapsed = Time.time - reactionStartTime;
            float progress = elapsed / reactionDuration;

            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            ApplyReaction(progress);
        }
    }

    public void TriggerReaction(Vector3 hitDirection)
    {
        if (hasReacted) return;

        Debug.Log($"★★★ SCREEN REACTION TRIGGERED! ★★★");

        hasReacted = true;
        reactionStartTime = Time.time;
    }

    void ApplyReaction(float progress)
    {
        switch (reactionMode)
        {
            case ReactionMode.TiltAndFall:
                ApplyTiltAndFall(progress);
                break;
            case ReactionMode.PushBack:
                ApplyPushBack(progress);
                break;
            case ReactionMode.SpinAway:
                ApplySpinAway(progress);
                break;
            case ReactionMode.Dissolve:
                ApplyDissolve(progress);
                break;
        }
    }

    void ApplyTiltAndFall(float progress)
    {
        float currentTilt = Mathf.Lerp(0, tiltAngle, progress);
        transform.localRotation = originalLocalRotation * Quaternion.Euler(currentTilt, 0, 0);

        float fallDistance = fallSpeed * progress;
        transform.localPosition = originalLocalPosition - new Vector3(0, fallDistance, 0);

        canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
    }

    void ApplyPushBack(float progress)
    {
        float pushCurve = Mathf.Sin(progress * Mathf.PI);
        transform.localPosition = originalLocalPosition + new Vector3(0, 0, -pushBackDistance * pushCurve);

        canvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(progress, 2));
    }

    void ApplySpinAway(float progress)
    {
        float spin = progress * 360f;
        transform.localRotation = originalLocalRotation * Quaternion.Euler(0, spin, spin * 0.5f);

        float moveAway = progress * 2f;
        transform.localPosition = originalLocalPosition + new Vector3(Random.Range(-1f, 1f) * moveAway, -moveAway, 0);

        canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
    }

    void ApplyDissolve(float progress)
    {
        canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);

        float scaleDown = Mathf.Lerp(1f, 0.3f, progress);
        transform.localScale = Vector3.one * scaleDown;
    }
}
