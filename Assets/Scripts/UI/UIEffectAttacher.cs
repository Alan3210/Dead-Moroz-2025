using UnityEngine;

public class UIEffectAttacher : MonoBehaviour
{
    [Header("VFX Settings")]
    [Tooltip("The Particle System or Effect Prefab to attach to this UI element")]
    [SerializeField] private GameObject effectPrefab;
    [Tooltip("Offset position relative to this UI element")]
    [SerializeField] private Vector3 offset = Vector3.zero;
    [Tooltip("Scale of the effect")]
    [SerializeField] private Vector3 scale = Vector3.one;
    [Tooltip("If true, sets sorting order to max to appear over UI")]
    [SerializeField] private bool renderOnTop = true;

    private GameObject instantiatedEffect;

    void Start()
    {
        if (effectPrefab != null)
        {
            AttachEffect();
        }
    }

    private void AttachEffect()
    {
        // Instantiate as child of this UI element
        instantiatedEffect = Instantiate(effectPrefab, transform);
        
        // Apply transform settings
        instantiatedEffect.transform.localPosition = offset;
        instantiatedEffect.transform.localScale = scale;
        instantiatedEffect.transform.localRotation = Quaternion.identity;

        // Handle sorting if requested
        if (renderOnTop)
        {
            SetRenderOrder(instantiatedEffect);
        }
    }

    private void SetRenderOrder(GameObject fx)
    {
        // Force all renderers to draw on top
        Renderer[] renderers = fx.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.sortingOrder = 32767;
        }

        // Also handle Canvas sorting if the effect uses UI particles/Canvas
        Canvas c = fx.GetComponent<Canvas>();
        if (c != null)
        {
            c.sortingOrder = 32767;
        }
    }
}
