using UnityEngine;

public class VoiceClipPlacer : MonoBehaviour
{
    [System.Serializable]
    public class VoiceClipData
    {
        public string clipName;
        public AudioClip audioClip;
        public float zPosition;
    }

    [Header("Placement Settings")]
    [SerializeField] private GameObject voiceClipTriggerPrefab;
    [SerializeField] private VoiceClipData[] voiceClips;
    [SerializeField] private float triggerHeight = 1f;

    void Start()
    {
        PlaceAllVoiceClips();
    }

    void PlaceAllVoiceClips()
    {
        foreach (VoiceClipData clipData in voiceClips)
        {
            if (clipData.audioClip == null)
            {
                Debug.LogWarning($"Voice clip '{clipData.clipName}' has no AudioClip assigned!");
                continue;
            }

            Vector3 spawnPosition = new Vector3(0f, triggerHeight, clipData.zPosition);
            GameObject trigger = Instantiate(voiceClipTriggerPrefab, spawnPosition, Quaternion.identity, transform);
            trigger.name = $"VoiceClip_{clipData.clipName}";

            VoiceClipTrigger triggerScript = trigger.GetComponent<VoiceClipTrigger>();
            if (triggerScript != null)
            {
                triggerScript.GetType()
                    .GetField("voiceClip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(triggerScript, clipData.audioClip);
            }
        }
    }
}
