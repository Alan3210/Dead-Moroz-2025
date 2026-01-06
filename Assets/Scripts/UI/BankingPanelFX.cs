using UnityEngine;

public class BankingPanelFX : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private GameObject showScreenFX;
    [SerializeField] private GameObject hideScreenFX;

    public void PlayShowFX()
    {
        Debug.Log("BankingPanelFX: PlayShowFX called");
        if (showScreenFX != null)
        {
            // Instantiate as sibling (child of parent)
            GameObject fx = Instantiate(showScreenFX, transform.position, Quaternion.identity, transform.parent);
            
            // Move it forward (towards camera) to ensure it's in front of the UI
            fx.transform.localPosition += new Vector3(0, 0, -100f);
            
            // Ensure scale is correct
            fx.transform.localScale = Vector3.one;

            // Force rendering on top of UI
            SetRenderOrder(fx);
        }
    }

    public void PlayHideFX()
    {
        Debug.Log("BankingPanelFX: PlayHideFX called");
        if (hideScreenFX != null)
        {
            GameObject fx = Instantiate(hideScreenFX, transform.position, Quaternion.identity, transform.parent);
            fx.transform.localPosition += new Vector3(0, 0, -100f);
            fx.transform.localScale = Vector3.one;
            SetRenderOrder(fx);
        }
    }

    private void SetRenderOrder(GameObject fx)
    {
        // Force all renderers (Particle Systems, Meshes) to draw on top of everything
        Renderer[] renderers = fx.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.sortingOrder = 32767; // Max sorting order
        }
        
        // Also handle Canvas logic if the FX itself contains a Canvas
        Canvas c = fx.GetComponent<Canvas>();
        if (c != null)
        {
            c.sortingOrder = 32767;
        }
    }
}
