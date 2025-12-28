using UnityEngine;

public class VoiceClipTrigger : MonoBehaviour
{
    [Header("Voice Clip Settings")]
    [SerializeField] private AudioClip voiceClip;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool destroyAfterPlaying = true;

    private bool hasPlayed = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasPlayed && playOnce) return;

        if (other.CompareTag("Player"))
        {
            PlayVoiceClip();
        }
    }

    void PlayVoiceClip()
    {
        if (voiceClip == null)
        {
            Debug.LogWarning($"VoiceClipTrigger on {gameObject.name} has no AudioClip assigned!");
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVoiceClip(voiceClip);
        }

        hasPlayed = true;

        if (destroyAfterPlaying)
        {
            Destroy(gameObject, 0.5f);
        }
    }
}
