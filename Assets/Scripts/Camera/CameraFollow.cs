using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -8);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool followX = false;

    private CameraShake cameraShake;

    private void Start()
    {
        cameraShake = GetComponent<CameraShake>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        if (!followX)
        {
            desiredPosition.x = offset.x;
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        if (cameraShake != null && cameraShake.IsShaking())
        {
            smoothedPosition += cameraShake.ShakeOffset;
        }

        transform.position = smoothedPosition;
    }
}
