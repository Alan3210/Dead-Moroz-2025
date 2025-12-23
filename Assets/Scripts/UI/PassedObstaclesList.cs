using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PassedObstaclesList : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject listItemPrefab;

    [Header("Settings")]
    [SerializeField] private float itemHeight = 40f;
    [SerializeField] private Color itemColor = Color.white;
    [SerializeField] private int fontSize = 16;

    public void DisplayPassedObstacles(List<string> obstacles)
    {
        Debug.Log($"[PassedObstaclesList] DisplayPassedObstacles called with {obstacles?.Count ?? 0} obstacles");
        Debug.Log($"[PassedObstaclesList] contentParent is null: {contentParent == null}");

        ClearList();

        if (obstacles == null || obstacles.Count == 0)
        {
            Debug.Log("[PassedObstaclesList] No obstacles, creating empty message");
            CreateListItem("Не пройдено ни одного препятствия");
            return;
        }

        Debug.Log($"[PassedObstaclesList] Creating {obstacles.Count} list items");
        foreach (string obstacle in obstacles)
        {
            Debug.Log($"[PassedObstaclesList] Creating item: {obstacle}");
            CreateListItem(obstacle);
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
        textComponent.overflowMode = TextOverflowModes.Truncate;
    }





    private void ClearList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}
