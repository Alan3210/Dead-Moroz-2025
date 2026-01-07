using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TVController : MonoBehaviour
{
    [Header("Appearance Settings")]
    [Tooltip("Time in seconds to wait before the TV appears.")]
    [SerializeField] private float appearDelay = 5f;
    
    [Header("Animation")]
    [SerializeField] private Animator tvAnimator;
    [SerializeField] private string showTriggerName = "Enter";

    [Header("Audio")]
    [Tooltip("Sound played when the TV starts entering the scene.")]
    [SerializeField] private AudioClip appearSFX;
    [Tooltip("Sound played when the TV screen actually turns on.")]
    [SerializeField] private AudioClip screenOnSFX;

    [Header("Ticker Settings")]
    [Tooltip("How long to wait AFTER the TV appears before the SCREEN turns on.")]
    [SerializeField] private float screenTurnOnDelay = 5f;
    [Tooltip("How long to wait AFTER the SCREEN turns on before TEXT starts scrolling.")]
    [SerializeField] private float textStartDelay = 5f;
    [Tooltip("The GameObject (Image) representing the TV screen content.")]
    [SerializeField] private GameObject tvScreenObject;
    [SerializeField] private TextMeshProUGUI tickerText;
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private string[] newsLines;
    [SerializeField] private float spaceBetweenLines = 50f;

    private RectTransform tickerRect;
    private float textWidth;
    private bool isScrolling = false;

    private void Awake()
    {
        // Ensure the screen is off immediately upon creation
        if (tvScreenObject != null)
        {
            tvScreenObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (tvAnimator == null)
        {
            tvAnimator = GetComponent<Animator>();
        }

        if (tickerText != null)
        {
            tickerRect = tickerText.GetComponent<RectTransform>();
            tickerText.text = ""; // Clear initial text
        }
    }

    public void ActivateTV()
    {
        StopAllCoroutines();
        StartCoroutine(ShowTVRoutine());
    }

    public void DeactivateTV()
    {
        StopAllCoroutines();
        if (tvScreenObject != null)
        {
            tvScreenObject.SetActive(false);
        }
        isScrolling = false;
    }

    private IEnumerator ShowTVRoutine()
    {
        yield return new WaitForSeconds(appearDelay);

        if (tvAnimator != null)
        {
            tvAnimator.SetTrigger(showTriggerName);
            
            if (appearSFX != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(appearSFX);
            }
        }

        // Calculate timing for sound (0.5s before screen turns on)
        float waitTime = Mathf.Max(0f, screenTurnOnDelay - 0.5f);
        yield return new WaitForSeconds(waitTime);

        // Play Sound 0.5s early
        if (screenOnSFX != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(screenOnSFX);
        }

        // Wait the remaining 0.5s (or full duration if delay was tiny)
        if (screenTurnOnDelay > 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            // If delay was < 0.5s, we already waited enough, just proceed
        }
        
        // Turn on the TV screen
        if (tvScreenObject != null)
        {
            tvScreenObject.SetActive(true);
        }

        // Wait before text starts
        yield return new WaitForSeconds(textStartDelay);

        StartCoroutine(ScrollTextRoutine());
    }

    private IEnumerator ScrollTextRoutine()
    {
        if (tickerText == null || newsLines == null || newsLines.Length == 0) yield break;

        isScrolling = true;

        // Combine all lines into one long string with spacing
        string fullText = "";
        string separator = new string(' ', 10); // Simple space-based separation
        foreach (string line in newsLines)
        {
            fullText += line.ToUpper() + separator;
        }
        
        // Loop the text by duplicating it once
        tickerText.text = fullText + fullText;
        
        // Force mesh update to get correct width
        tickerText.ForceMeshUpdate();
        textWidth = tickerText.GetRenderedValues(false).x / 2f;

        Vector2 startPos = tickerRect.anchoredPosition;
        float currentX = 0;

        while (isScrolling)
        {
            currentX -= scrollSpeed * Time.deltaTime;
            
            // If we've scrolled past the first half (the original text), reset
            if (currentX <= -textWidth)
            {
                currentX = 0;
            }

            tickerRect.anchoredPosition = new Vector2(currentX, startPos.y);
            yield return null;
        }
    }
}
