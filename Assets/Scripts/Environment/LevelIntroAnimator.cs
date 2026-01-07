using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LevelIntroAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float dropHeight = 80f;
    [SerializeField] private float dropDuration = 0.6f;
    [SerializeField] private float waveDelay = 0.005f; // Very fast ripple for individual items
    [SerializeField] private float categoryOverlap = 0.5f; // How much overlap between categories (0-1)

    [Header("Bounce Settings")]
    [SerializeField] private AnimationCurve bounceCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.4f, 1.1f), // Overshoot
        new Keyframe(0.7f, 0.95f), // Undershoot
        new Keyframe(1f, 1f)      // Settle
    );

    [Header("Audio")]
    [SerializeField] private AudioClip impactSFX;
    [SerializeField] [Range(0f, 1f)] private float impactVolume = 0.3f;

    // Store original positions to restore them precisely
    private Dictionary<Transform, Vector3> originalPositions = new Dictionary<Transform, Vector3>();
    
    // Lists to hold sorted objects
    private List<Transform> snowList;
    private List<Transform> envList;
    private List<Transform> obsList;
    private List<Transform> colList;

    private void Start()
    {
        // Failsafe: If Prepare hasn't been called yet (lists are null), try to grab from LevelBuilder
        if (snowList == null)
        {
            LevelBuilder lb = GetComponent<LevelBuilder>();
            if (lb != null)
            {
                Debug.Log("[LevelIntroAnimator] Self-initializing via Start()...");
                // Check if parents are assigned in LevelBuilder
                if (lb.SnowParent != null || lb.EnvironmentParent != null)
                {
                    Prepare(lb.SnowParent, lb.EnvironmentParent, lb.ObstaclesParent, lb.CollectiblesParent);
                }
                else
                {
                    Debug.LogWarning("[LevelIntroAnimator] LevelBuilder has no parents assigned yet. Maybe waiting for BuildLevel?");
                }
            }
        }
    }

    public void Prepare(Transform snowParent, Transform envParent, Transform obsParent, Transform colParent)
    {
        Debug.Log("[LevelIntroAnimator] Prepare called.");
        StopAllCoroutines();
        originalPositions.Clear();

        // 1. Gather and Sort Objects (by Z position for the "Wave" effect)
        snowList = GatherAllChildren(snowParent);
        envList = GatherAllChildren(envParent);
        obsList = GatherAllChildren(obsParent);
        colList = GatherAllChildren(colParent);

        Debug.Log($"[LevelIntroAnimator] Gathered: Snow={snowList.Count}, Env={envList.Count}, Obs={obsList.Count}, Col={colList.Count}");

        // 2. Move them to the sky
        MoveToSky(snowList);
        MoveToSky(envList);
        MoveToSky(obsList);
        MoveToSky(colList);
    }

    public void Play()
    {
        Debug.Log("[LevelIntroAnimator] Play sequence started.");

        // Lazy Init: If lists are null, we missed Start/Prepare. Try to recover.
        if (snowList == null)
        {
            Debug.LogWarning("[LevelIntroAnimator] Lists are null in Play! Attempting Late Prepare...");
            
            // Try to find LevelBuilder on THIS object first
            LevelBuilder lb = GetComponent<LevelBuilder>();
            
            // If not found, look for it ANYWHERE in the scene
            if (lb == null)
            {
                lb = FindFirstObjectByType<LevelBuilder>();
                if (lb != null) Debug.Log("[LevelIntroAnimator] Found LevelBuilder globally!");
            }

            if (lb != null)
            {
                Prepare(lb.SnowParent, lb.EnvironmentParent, lb.ObstaclesParent, lb.CollectiblesParent);
            }
            else
            {
                Debug.LogError("[LevelIntroAnimator] FATAL: Could not find LevelBuilder anywhere in the scene!");
                return;
            }
        }

        // Ensure this component is enabled so Coroutines can run
        if (!enabled)
        {
            Debug.LogWarning("[LevelIntroAnimator] Component was disabled. Enabling it now.");
            enabled = true;
        }

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        // Calculate delays based on overlap
        // If duration of a wave is (count * waveDelay), next category starts at (duration * (1 - overlap))
        
        // Start Wave 1: Snow
        Coroutine c1 = StartCoroutine(AnimateCategory(snowList));
        yield return new WaitForSeconds(GetWaveDuration(snowList) * (1f - categoryOverlap));

        // Start Wave 2: Environment
        Coroutine c2 = StartCoroutine(AnimateCategory(envList));
        yield return new WaitForSeconds(GetWaveDuration(envList) * (1f - categoryOverlap));

        // Start Wave 3: Obstacles
        Coroutine c3 = StartCoroutine(AnimateCategory(obsList));
        yield return new WaitForSeconds(GetWaveDuration(obsList) * (1f - categoryOverlap));

        // Start Wave 4: Collectibles
        Coroutine c4 = StartCoroutine(AnimateCategory(colList));
    }

    private float GetWaveDuration(List<Transform> list)
    {
        if (list == null || list.Count == 0) return 0f;
        // The total time for the wave to *start* all items. 
        // Not including the drop duration of the last item.
        return list.Count * waveDelay; 
    }

    private IEnumerator AnimateCategory(List<Transform> items)
    {
        if (items == null || items.Count == 0) yield break;

        // Play Sound ONCE per category, timed to hit when the first object lands
        if (impactSFX != null && AudioManager.Instance != null)
        {
            StartCoroutine(PlaySoundDelayed(dropDuration));
        }

        foreach (Transform item in items)
        {
            if (item == null) continue;

            StartCoroutine(DropItem(item));
            yield return new WaitForSeconds(waveDelay);
        }
    }

    private IEnumerator PlaySoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (AudioManager.Instance != null && impactSFX != null)
        {
            AudioManager.Instance.PlaySFX(impactSFX, impactVolume);
        }
    }

    private IEnumerator DropItem(Transform item)
    {
        if (!originalPositions.ContainsKey(item)) yield break;

        Vector3 endPos = originalPositions[item];
        Vector3 startPos = endPos + Vector3.up * dropHeight;
        
        float elapsed = 0f;
        
        // Ensure visible
        item.gameObject.SetActive(true);

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dropDuration;
            float curveValue = bounceCurve.Evaluate(t);

            // Interpolate Y
            float currentY = Mathf.LerpUnclamped(startPos.y, endPos.y, curveValue);
            item.position = new Vector3(endPos.x, currentY, endPos.z);

            yield return null;
        }

        item.position = endPos;
    }

    private void MoveToSky(List<Transform> items)
    {
        foreach (Transform item in items)
        {
            if (item == null) continue;

            if (!originalPositions.ContainsKey(item))
            {
                originalPositions.Add(item, item.position);
            }

            // Move up
            item.position = item.position + Vector3.up * dropHeight;
            // Optionally hide gameobject to prevent shadows/rendering high up? 
            // But we want them to fall into view.
        }
    }

    private List<Transform> GatherAllChildren(Transform parent)
    {
        List<Transform> list = new List<Transform>();
        if (parent == null) return list;

        // Recursive gather to handle Month_XX containers
        GatherRecursive(parent, list);

        // Sort by Z position for the wave effect
        return list.OrderBy(t => t.position.z).ToList();
    }

    private void GatherRecursive(Transform current, List<Transform> list)
    {
        foreach (Transform child in current)
        {
            // If the child is a container (Month_XX), go deeper
            // If it's a leaf object (has MeshRenderer or is a prefab instance), add it
            // Simple heuristic: If it has children and no renderer, it might be a container.
            // But some objects have children (like text on obstacle).
            // We want the ROOT of the falling object.
            
            // For Obstacles/Collectibles: They are under Month_XX.
            // LevelBuilder: Obstacles -> Month_XX -> Obstacle_Prefab
            // So we want the children of Month_XX.
            
            // For Snow/Env: They are direct children of parent.
            
            // Let's look at depth.
            // Parent (Obstacles) -> Child (Month) -> GrandChild (Obstacle).
            // We want GrandChildren.
            
            // If 'current' is the main container (passed in Prepare), we check its children.
            // If those children are "Month_...", we recurse.
            // If they are "Snow_..." or "Pine_...", we add them.
            
            if (child.name.StartsWith("Month_"))
            {
                GatherRecursive(child, list);
            }
            else
            {
                list.Add(child);
            }
        }
    }
}
