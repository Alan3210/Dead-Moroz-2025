    using UnityEngine;

    public class CameraFollow : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 5, -8);
        [SerializeField] private bool followX = false;

        private bool isFollowing = true;

        [Header("Smooth Follow")]
        [SerializeField] private float followLagAmount = 0.1f;

        [Header("Dynamic Tilt")]
        [SerializeField] private bool enableTilt = true;
        [SerializeField] private float tiltAmount = 3f;
        [SerializeField] private float tiltSpeed = 4f;

        [Header("Look Ahead")]
        [SerializeField] private bool enableLookAhead = true;
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private float lookAheadSpeed = 3f;

        [Header("Subtle Movement Shake")]
        [SerializeField] private bool enableMovementShake = true;
        [SerializeField] private float shakeFrequency = 2f;
        [SerializeField] private float shakeMagnitude = 0.03f;

        private CameraShake cameraShake;
        private Vector3 velocity = Vector3.zero;
        private Vector3 lastTargetPosition;
        private float currentTiltZ;
        private float targetTiltZ;
        private float currentLookAheadX;
        private float targetLookAheadX;
        private float shakeTime;

        private void Start()
        {
            cameraShake = GetComponent<CameraShake>();

            if (target != null)
            {
                lastTargetPosition = target.position;
            }
        }

    void LateUpdate()
    {
        if (target == null || !isFollowing || Time.timeScale == 0f) return;

        CalculateDynamicEffects();

        Vector3 desiredPosition = CalculateDesiredPosition();
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            followLagAmount,
            Mathf.Infinity,
            Time.deltaTime
        );

        if (cameraShake != null && cameraShake.IsShaking())
        {
            smoothedPosition += cameraShake.ShakeOffset;
        }

        if (enableMovementShake)
        {
            smoothedPosition += CalculateMovementShake();
        }

        transform.position = smoothedPosition;

        if (enableTilt)
        {
            ApplyTilt();
        }

        lastTargetPosition = target.position;
    }

    public void StopFollowing()
        {
            isFollowing = false;
        }

        public void StartFollowing()
        {
            isFollowing = true;
        }

        private void CalculateDynamicEffects()
        {
            Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;

            if (enableTilt)
            {
                if (Mathf.Abs(targetVelocity.x) > 0.1f)
                {
                    targetTiltZ = -targetVelocity.x * tiltAmount;
                }
                else
                {
                    targetTiltZ = 0f;
                }

                currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, tiltSpeed * Time.deltaTime);
            }

            if (enableLookAhead)
            {
                if (Mathf.Abs(targetVelocity.x) > 0.1f)
                {
                    targetLookAheadX = Mathf.Sign(targetVelocity.x) * lookAheadDistance;
                }
                else
                {
                    targetLookAheadX = 0f;
                }

                currentLookAheadX = Mathf.Lerp(currentLookAheadX, targetLookAheadX, lookAheadSpeed * Time.deltaTime);
            }
        }

        private Vector3 CalculateDesiredPosition()
        {
            Vector3 desiredPosition = target.position + offset;

            if (!followX)
            {
                desiredPosition.x = offset.x;
            }

            if (enableLookAhead)
            {
                desiredPosition.x += currentLookAheadX;
            }

            return desiredPosition;
        }

        private void ApplyTilt()
        {
            Quaternion tiltRotation = Quaternion.Euler(0f, 0f, currentTiltZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, tiltRotation, tiltSpeed * Time.deltaTime);
        }

        private Vector3 CalculateMovementShake()
        {
            shakeTime += Time.deltaTime * shakeFrequency;

            float shakeX = Mathf.PerlinNoise(shakeTime, 0f) * 2f - 1f;
            float shakeY = Mathf.PerlinNoise(0f, shakeTime) * 2f - 1f;

            return new Vector3(shakeX, shakeY, 0f) * shakeMagnitude;
        }
    }
