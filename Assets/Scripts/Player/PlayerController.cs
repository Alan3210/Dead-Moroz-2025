using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Lane Settings")]
    [SerializeField] private float laneDistance = 2f;
    [SerializeField] private int currentLane = 1;
    [SerializeField] private float laneChangeSpeed = 15f;

    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float speedIncreaseRate = 0.5f;
    [SerializeField] private float maxSpeed = 20f;

    [Header("Input Settings")]
    [SerializeField] private float inputCooldown = 0.3f;

    private PlayerInputActions inputActions;
    private Vector3 targetPosition;
    private float lastInputTime;
    private bool isGameActive = true;

    private const int LEFT_LANE = 0;
    private const int MIDDLE_LANE = 1;
    private const int RIGHT_LANE = 2;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        targetPosition = transform.position;
    }

    void OnEnable()
    {
        inputActions.Gameplay.Enable();
        inputActions.Gameplay.Move.performed += OnMoveInput;
    }

    void OnDisable()
    {
        inputActions.Gameplay.Move.performed -= OnMoveInput;
        inputActions.Gameplay.Disable();
    }

    void Update()
    {
        if (!isGameActive) return;

        MoveForward();
        MoveBetweenLanes();
        HandleTouchInput();
    }

    void MoveForward()
    {
        transform.position += Vector3.forward * forwardSpeed * Time.deltaTime;
    }

    void MoveBetweenLanes()
    {
        Vector3 currentPos = transform.position;
        currentPos.x = Mathf.Lerp(currentPos.x, targetPosition.x, laneChangeSpeed * Time.deltaTime);
        transform.position = currentPos;
    }

    void OnMoveInput(InputAction.CallbackContext context)
    {
        if (!isGameActive) return;
        if (Time.time - lastInputTime < inputCooldown) return;

        float value = context.ReadValue<float>();

        if (value > 0.1f)
        {
            ChangeLane(1);
        }
        else if (value < -0.1f)
        {
            ChangeLane(-1);
        }
    }

    void HandleTouchInput()
    {
        if (Touchscreen.current == null) return;
        if (Time.time - lastInputTime < inputCooldown) return;

        if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            float screenMiddle = Screen.width / 2f;

            if (touchPosition.x > screenMiddle)
            {
                ChangeLane(1);
            }
            else
            {
                ChangeLane(-1);
            }
        }
    }

    void ChangeLane(int direction)
    {
        currentLane = Mathf.Clamp(currentLane + direction, LEFT_LANE, RIGHT_LANE);

        float targetX = (currentLane - 1) * laneDistance;
        targetPosition = new Vector3(targetX, transform.position.y, transform.position.z);

        lastInputTime = Time.time;

        // Add this line:
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLaneChangeSound();
        }
    }


    public void IncreaseSpeed(float amount)
    {
        forwardSpeed = Mathf.Min(forwardSpeed + amount, maxSpeed);
    }

    public void SetSpeed(float speed)
    {
        forwardSpeed = Mathf.Clamp(speed, 0f, maxSpeed);
    }

    public void StopPlayer()
    {
        isGameActive = false;
        forwardSpeed = 0f;
    }

    public void ResumePlayer()
    {
        isGameActive = true;
    }

    public float GetCurrentSpeed()
    {
        return forwardSpeed;
    }
}
