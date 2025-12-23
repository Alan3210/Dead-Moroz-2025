using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private GameObject healthIconPrefab;
    [SerializeField] private Transform healthIconContainer;
    [SerializeField] private Sprite tangerineFullSprite;
    [SerializeField] private Sprite tangerineEmptySprite;

    [Header("Layout")]
    [SerializeField] private float iconSpacing = 10f;

    private List<Image> healthIcons = new List<Image>();

    void Start()
    {
        if (HealthSystem.Instance != null)
        {
            CreateHealthIcons(HealthSystem.Instance.GetMaxHealth());
            HealthSystem.Instance.OnHealthChanged.AddListener(UpdateHealthDisplay);
            UpdateHealthDisplay(HealthSystem.Instance.GetCurrentHealth());
        }
    }

    void CreateHealthIcons(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject icon;

            if (healthIconPrefab != null)
            {
                icon = Instantiate(healthIconPrefab, healthIconContainer);
            }
            else
            {
                icon = new GameObject($"HealthIcon_{i}");
                icon.transform.SetParent(healthIconContainer);
            }

            Image iconImage = icon.GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = icon.AddComponent<Image>();
            }

            iconImage.sprite = tangerineFullSprite;
            iconImage.preserveAspect = true;

            RectTransform rectTransform = icon.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(50, 50);

            healthIcons.Add(iconImage);
        }
    }

    void UpdateHealthDisplay(int currentHealth)
    {
        for (int i = 0; i < healthIcons.Count; i++)
        {
            if (i < currentHealth)
            {
                healthIcons[i].sprite = tangerineFullSprite;
                healthIcons[i].color = Color.white;
            }
            else
            {
                if (tangerineEmptySprite != null)
                {
                    healthIcons[i].sprite = tangerineEmptySprite;
                }
                healthIcons[i].color = new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }

    void OnDestroy()
    {
        if (HealthSystem.Instance != null)
        {
            HealthSystem.Instance.OnHealthChanged.RemoveListener(UpdateHealthDisplay);
        }
    }
}
