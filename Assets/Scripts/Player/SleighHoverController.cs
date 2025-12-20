using UnityEngine;

public class SleighHoverController : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverHeight = 0.5f;
    [SerializeField] private float hoverSpeed = 5f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Visual Bobbing")]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private float bobbingSpeed = 2f;

    [Header("References")]
    [SerializeField] private Transform visualTransform;

    [Header("Raycast Points")]
    [SerializeField] private Transform[] hoverPoints;

    [Header("Tilt Settings")]
    [SerializeField] private float maxTiltAngle = 10f;
    [SerializeField] private float tiltSpeed = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = true;

    private float currentHeight;
    private float bobbingOffset;
    private Vector3 visualOriginalPosition;

    void Start()
    {
        if (visualTransform == null)
        {
            Transform visual = transform.Find("Visual");
            if (visual != null)
            {
                visualTransform = visual;
            }
            else
            {
                Debug.LogWarning("Visual transform not found! Please assign it in the inspector.");
                enabled = false;
                return;
            }
        }

        visualOriginalPosition = visualTransform.localPosition;
        currentHeight = hoverHeight;
    }

    void Update()
    {
        float targetHeight = CalculateTargetHeight();
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, hoverSpeed * Time.deltaTime);

        float bobbing = 0f;
        if (enableBobbing)
        {
            bobbingOffset += Time.deltaTime * bobbingSpeed;
            bobbing = Mathf.Sin(bobbingOffset) * bobbingAmount;
        }

        Vector3 newPos = visualOriginalPosition;
        newPos.y = currentHeight + bobbing;
        visualTransform.localPosition = newPos;

        ApplyGroundAlignment();
    }

    float CalculateTargetHeight()
    {
        if (hoverPoints == null || hoverPoints.Length == 0)
        {
            return GetGroundDistance(transform.position);
        }

        float totalDistance = 0f;
        int validPoints = 0;

        foreach (Transform hoverPoint in hoverPoints)
        {
            if (hoverPoint != null)
            {
                float distance = GetGroundDistance(hoverPoint.position);
                if (distance > 0f)
                {
                    totalDistance += distance;
                    validPoints++;
                }
            }
        }

        if (validPoints > 0)
        {
            return totalDistance / validPoints;
        }

        return hoverHeight;
    }

    float GetGroundDistance(Vector3 origin)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, -Vector3.up, out hit, hoverHeight * 3f, groundLayer))
        {
            if (showDebugRays)
            {
                Debug.DrawRay(origin, -Vector3.up * hit.distance, Color.green);
            }
            return hoverHeight;
        }
        else
        {
            if (showDebugRays)
            {
                Debug.DrawRay(origin, -Vector3.up * hoverHeight * 3f, Color.red);
            }
        }

        return hoverHeight;
    }

    void ApplyGroundAlignment()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, hoverHeight * 3f, groundLayer))
        {
            Vector3 targetUp = hit.normal;
            Vector3 currentUp = transform.up;
            Vector3 newUp = Vector3.Lerp(currentUp, targetUp, tiltSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, newUp) * transform.rotation;
            Vector3 eulerAngles = targetRotation.eulerAngles;

            eulerAngles.x = NormalizeAngle(eulerAngles.x);
            eulerAngles.z = NormalizeAngle(eulerAngles.z);

            eulerAngles.x = Mathf.Clamp(eulerAngles.x, -maxTiltAngle, maxTiltAngle);
            eulerAngles.z = Mathf.Clamp(eulerAngles.z, -maxTiltAngle, maxTiltAngle);

            transform.rotation = Quaternion.Euler(eulerAngles.x, transform.rotation.eulerAngles.y, eulerAngles.z);
        }
    }

    float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            return angle - 360f;
        return angle;
    }

    void OnDrawGizmos()
    {
        if (!showDebugRays) return;

        Gizmos.color = Color.cyan;

        if (hoverPoints != null && hoverPoints.Length > 0)
        {
            foreach (Transform hoverPoint in hoverPoints)
            {
                if (hoverPoint != null)
                {
                    Gizmos.DrawWireSphere(hoverPoint.position, 0.1f);
                }
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
    }
}
