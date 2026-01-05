using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PassedObstaclesList : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Settings")]
    [SerializeField] private float itemHeight = 40f;
    [SerializeField] private Color itemColor = Color.white;
    [SerializeField] private int fontSize = 24;

    [Header("Auto-Scroll Settings")]
    [SerializeField] private bool enableAutoScroll = true;
    [SerializeField] private float autoScrollSpeed = 0.5f;
    [SerializeField] private float autoScrollDelay = 1f;

    private Coroutine autoScrollCoroutine;

    public void DisplayPassedObstacles(List<string> obstacles)
    {
        Debug.Log($"[PassedObstaclesList] DisplayPassedObstacles called with {obstacles?.Count ?? 0} obstacles");

        ClearList();

        if (obstacles == null || obstacles.Count == 0)
        {
            Debug.Log("[PassedObstaclesList] No obstacles, creating empty message");
            CreateListItem("Вы избежали всех препятствий!");
            return;
        }

        Debug.Log($"[PassedObstaclesList] Creating {obstacles.Count} list items");
        foreach (string obstacle in obstacles)
        {
            CreateListItem(obstacle);
        }

        // Force layout rebuild
        StartCoroutine(RebuildLayoutAndStartScroll());
    }

    private IEnumerator RebuildLayoutAndStartScroll()
    {
        // Wait for end of frame to ensure layout is rebuilt
        yield return new WaitForEndOfFrame();

        // Force canvas update
        if (contentParent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
        }

        // Reset scroll to top
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }

        // Start auto-scroll
        if (enableAutoScroll)
        {
            if (autoScrollCoroutine != null)
            {
                StopCoroutine(autoScrollCoroutine);
            }
            autoScrollCoroutine = StartCoroutine(AutoScrollCoroutine());
        }
    }

    private IEnumerator AutoScrollCoroutine()
    {
        // Wait before starting scroll
        yield return new WaitForSecondsRealtime(autoScrollDelay);

        // Scroll from top to bottom
        while (scrollRect != null && scrollRect.verticalNormalizedPosition > 0f)
        {
            scrollRect.verticalNormalizedPosition -= autoScrollSpeed * Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure we're at the bottom
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    private void CreateListItem(string text)
    {
        GameObject item = new GameObject("ListItem");
        item.layer = LayerMask.NameToLayer("UI");
        item.transform.SetParent(contentParent, false);

        RectTransform rectTransform = item.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(0.5f, 1);
        rectTransform.sizeDelta = new Vector2(0, itemHeight);

        LayoutElement layoutElement = item.AddComponent<LayoutElement>();
        layoutElement.minHeight = itemHeight;
        layoutElement.preferredHeight = itemHeight;
        layoutElement.flexibleHeight = 0;

        TextMeshProUGUI textComponent = item.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = itemColor;
        textComponent.alignment = TextAlignmentOptions.Left;
        textComponent.margin = new Vector4(10, 5, 10, 5);
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        textComponent.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void ClearList()
    {
        if (contentParent == null) return;

        // Stop auto-scroll if running
        if (autoScrollCoroutine != null)
        {
            StopCoroutine(autoScrollCoroutine);
            autoScrollCoroutine = null;
        }

        // Clear all children
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in contentParent)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }
    }

    private void OnDisable()
    {
        // Stop auto-scroll when panel is disabled
        if (autoScrollCoroutine != null)
        {
            StopCoroutine(autoScrollCoroutine);
            autoScrollCoroutine = null;
        }
    }
}
