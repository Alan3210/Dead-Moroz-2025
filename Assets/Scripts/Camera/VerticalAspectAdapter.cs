using UnityEngine;

[RequireComponent(typeof(Camera))]
public class VerticalAspectAdapter : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The total width of the play area that must be visible (e.g., road width + margins).")]
    [SerializeField] private float requiredVisibleWidth = 8.0f;
    
    [Tooltip("If true, adjusts the Y height proportionally to the Z distance change to maintain the viewing angle.")]
    [SerializeField] private bool maintainViewingAngle = true;

    [Header("References")]
    [SerializeField] private CameraFollow cameraFollow;

    private Camera cam;
    private float defaultAspect;
    private Vector3 originalOffset;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cameraFollow == null)
        {
            cameraFollow = GetComponent<CameraFollow>();
        }

        if (cameraFollow != null)
        {
            originalOffset = cameraFollow.GetOffset();
        }
        else
        {
            Debug.LogError("[VerticalAspectAdapter] No CameraFollow script found!");
            enabled = false;
        }
    }

    void Start()
    {
        AdjustCamera();
    }

    // Optional: Call this if screen orientation changes at runtime
    public void AdjustCamera()
    {
        if (cam == null || cameraFollow == null) return;

        float aspect = cam.aspect;

        // Calculate required distance to see 'requiredVisibleWidth'
        // Formula: Distance = (Width / 2) / (Aspect * tan(VerticalFOV / 2))
        float halfFovRad = cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float tanHalfFov = Mathf.Tan(halfFovRad);
        
        float requiredDistance = (requiredVisibleWidth * 0.5f) / (aspect * tanHalfFov);

        // Check current distance (Assuming camera looks mostly forward/down)
        // We use the offset.z magnitude effectively. 
        // originalOffset is (0, 5, -8). Distance ~ 8 along Z axis relative to target.
        // Actually, strictly speaking, distance is just the Z offset magnitude if looking straight on.
        // If we are tilted, it's more complex, but adjusting Z offset is usually what we want.
        
        float currentTargetZDist = Mathf.Abs(originalOffset.z);

        if (requiredDistance > currentTargetZDist)
        {
            Debug.Log($"[VerticalAspectAdapter] Portrait mode detected (Aspect: {aspect:F2}). Adjusting camera distance from {currentTargetZDist} to {requiredDistance:F2}");

            Vector3 newOffset = originalOffset;
            newOffset.z = -requiredDistance;

            if (maintainViewingAngle)
            {
                // Scale Y to match the new Z so the angle (atan(Y/Z)) stays the same
                float scale = requiredDistance / currentTargetZDist;
                newOffset.y = originalOffset.y * scale;
            }

            cameraFollow.SetOffset(newOffset);
        }
        else
        {
            // If we are in landscape (wide aspect), the required distance is usually small,
            // so we stick to the original designer-set offset.
            Debug.Log($"[VerticalAspectAdapter] Landscape or wide enough (Aspect: {aspect:F2}). Keeping original offset.");
            cameraFollow.SetOffset(originalOffset);
        }
    }
}
