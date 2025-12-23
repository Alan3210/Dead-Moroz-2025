using UnityEngine;
using TMPro;

public class ObstacleTextDisplay : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float textOffsetY = 2.5f;
    [SerializeField] private float fontSize = 18f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
    [SerializeField] private Vector2 canvasSize = new Vector2(300f, 80f);
    [SerializeField] private float canvasScale = 0.01f;

    [Header("Screen Physics Reaction")]
    [SerializeField] private TextScreenPhysics.ReactionMode reactionMode = TextScreenPhysics.ReactionMode.TiltAndFall;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private float tiltAngle = 90f;
    [SerializeField] private float pushBackDistance = 2f;
    [SerializeField] private float reactionDuration = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private Canvas canvas;
    private TextMeshProUGUI textMeshPro;
    private UnityEngine.UI.Image backgroundImage;
    private Camera mainCamera;

    private void Awake()
    {
        CreateWorldSpaceCanvas();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (canvas != null && mainCamera != null)
        {
            canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }

    private void CreateWorldSpaceCanvas()
    {
        GameObject canvasObj = new GameObject("TextCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = new Vector3(0, textOffsetY, 0);
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one * canvasScale;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = canvasSize;

        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        TextScreenPhysics screenPhysics = canvasObj.AddComponent<TextScreenPhysics>();
        screenPhysics.SetPhysicsSettings(reactionMode, fallSpeed, tiltAngle, pushBackDistance, reactionDuration);

        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(canvasObj.transform, false);

        backgroundImage = backgroundObj.AddComponent<UnityEngine.UI.Image>();
        backgroundImage.color = backgroundColor;

        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(canvasObj.transform, false);

        textMeshPro = textObj.AddComponent<TextMeshProUGUI>();
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-10, -10);
        textRect.anchoredPosition = Vector2.zero;

        textMeshPro.fontSize = fontSize;
        textMeshPro.color = textColor;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.textWrappingMode = TextWrappingModes.Normal;
        textMeshPro.overflowMode = TextOverflowModes.Truncate;
        textMeshPro.fontStyle = FontStyles.Bold;

        textMeshPro.text = "Loading...";

        if (enableDebugLogs)
        {
            Debug.Log($"[ObstacleTextDisplay] World space canvas created for {gameObject.name}");
        }
    }

    public void TriggerScreenReaction(Vector3 hitDirection)
    {
        Debug.Log($"[ObstacleTextDisplay] TriggerScreenReaction called on {gameObject.name}");

        if (canvas != null)
        {
            Debug.Log($"[ObstacleTextDisplay] Canvas found: {canvas.gameObject.name}");
            TextScreenPhysics physics = canvas.GetComponent<TextScreenPhysics>();
            if (physics != null)
            {
                Debug.Log($"[ObstacleTextDisplay] TextScreenPhysics found, triggering reaction");
                physics.TriggerReaction(hitDirection);
            }
            else
            {
                Debug.LogWarning($"[ObstacleTextDisplay] TextScreenPhysics NOT found on canvas!");
            }
        }
        else
        {
            Debug.LogWarning($"[ObstacleTextDisplay] Canvas is NULL!");
        }
    }

    public void SetText(string text)
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = text;

            if (enableDebugLogs)
            {
                Debug.Log($"[ObstacleTextDisplay] Text set to: '{text}'");
            }
        }
    }

    public void SetTextColor(Color color)
    {
        if (textMeshPro != null)
        {
            textMeshPro.color = color;
        }
    }

    public void SetBackgroundColor(Color color)
    {
        backgroundColor = color;
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }

    public void SetFontSize(float size)
    {
        fontSize = size;
        if (textMeshPro != null)
        {
            textMeshPro.fontSize = size;
        }
    }
}
