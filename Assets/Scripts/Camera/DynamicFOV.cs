using UnityEngine;

public class DynamicFOV : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController playerController;

    [Header("FOV Settings")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float maxFOV = 75f;
    [SerializeField] private float minSpeedForFOV = 10f;
    [SerializeField] private float maxSpeedForFOV = 30f;
    [SerializeField] private float fovChangeSpeed = 3f;

    private float targetFOV;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
        }

        if (mainCamera != null)
        {
            mainCamera.fieldOfView = baseFOV;
            targetFOV = baseFOV;
        }
    }

    void Update()
    {
        if (playerController == null || mainCamera == null) return;

        float currentSpeed = playerController.GetCurrentSpeed();

        if (currentSpeed < minSpeedForFOV)
        {
            targetFOV = baseFOV;
        }
        else
        {
            float speedRatio = Mathf.Clamp01((currentSpeed - minSpeedForFOV) / (maxSpeedForFOV - minSpeedForFOV));
            targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedRatio);
        }

        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }
}
