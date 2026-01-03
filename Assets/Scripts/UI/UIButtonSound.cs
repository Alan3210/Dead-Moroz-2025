using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Sound Settings")]
    [Tooltip("Sound played when mouse hovers over the button")]
    [SerializeField] private AudioClip hoverSound;

    [Tooltip("Sound played when button is clicked")]
    [SerializeField] private AudioClip clickSound;

    [Header("Volume Settings")]
    [Range(0f, 2f)]
    [SerializeField] private float hoverVolume = 1f;

    [Range(0f, 3f)]
    [SerializeField] private float clickVolume = 2f;

    [Header("Options")]
    [Tooltip("Play hover sound only once when entering button area")]
    [SerializeField] private bool playHoverOnce = true;

    private Button button;
    private bool hasPlayedHover = false;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (playHoverOnce && hasPlayedHover) return;

        if (hoverSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hoverSound, hoverVolume);
            hasPlayedHover = true;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable) return;

        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSound, clickVolume);

        }

        hasPlayedHover = false;
    }

    public void PlayClickSound()
    {
        if (clickSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clickSound);
        }
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hoverSound);
        }
    }

    private void OnDisable()
    {
        hasPlayedHover = false;
    }
}
