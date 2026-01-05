using UnityEngine;
using UnityEngine.UI;

public class MobileInputButtons : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    [Header("Visibility Settings")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;

    private void Start()
    {
        if (leftButton != null)
        {
            leftButton.onClick.AddListener(OnLeftButtonPressed);
        }

        if (rightButton != null)
        {
            rightButton.onClick.AddListener(OnRightButtonPressed);
        }

        bool isMobile = Application.isMobilePlatform ||
                        UnityEngine.Device.Application.isMobilePlatform;

        if (isMobile || Application.isEditor)
        {
            ShowControls();
        }
        else
        {
            HideControls();
        }
    }


    private void OnDestroy()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveListener(OnLeftButtonPressed);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveListener(OnRightButtonPressed);
        }
    }

    private void OnLeftButtonPressed()
    {
        if (playerController != null)
        {
            playerController.ChangeLaneViaButton(-1);
        }
    }

    private void OnRightButtonPressed()
    {
        if (playerController != null)
        {
            playerController.ChangeLaneViaButton(1);
        }
    }

    public void ShowControls()
    {
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 1f;
            controlsCanvasGroup.interactable = true;
            controlsCanvasGroup.blocksRaycasts = true;
        }
    }

    public void HideControls()
    {
        if (controlsCanvasGroup != null)
        {
            controlsCanvasGroup.alpha = 0f;
            controlsCanvasGroup.interactable = false;
            controlsCanvasGroup.blocksRaycasts = false;
        }
    }
}
