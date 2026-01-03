using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip collisionSFX;
    [SerializeField] private AudioClip laneChangeSFX;
    [SerializeField] private AudioClip monthTransitionSFX;
    [SerializeField] private AudioClip moneyPickupSFX;

    [Header("Ded Moroz Voice Clips")]
    [SerializeField] private AudioClip[] dedMorozCollisionVoices;
    [SerializeField] private float[] voiceClipVolumes = new float[] { 1.5f, 1.0f, 1.0f };
    [SerializeField] private float voiceVolume = 1f;

    [Header("Volume Settings")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicVolume = 0.7f;

    private int currentVoiceIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
        }

        sfxSource.volume = sfxVolume;
        musicSource.volume = musicVolume;
        voiceSource.volume = voiceVolume;
    }

    public void PlayVoiceClip(AudioClip clip)
    {
        if (voiceSource == null || clip == null) return;

        if (voiceSource.isPlaying)
        {
            voiceSource.Stop();
        }

        voiceSource.PlayOneShot(clip, voiceVolume);
    }

    public void PlayCollisionSound()
    {
        PlaySFX(collisionSFX);
        PlaySequentialDedMorozVoice();
    }

    public void PlaySequentialDedMorozVoice()
    {
        if (dedMorozCollisionVoices == null || dedMorozCollisionVoices.Length == 0)
        {
            Debug.LogWarning("No Ded Moroz voice clips assigned!");
            return;
        }

        AudioClip selectedVoice = dedMorozCollisionVoices[currentVoiceIndex];

        if (selectedVoice != null && voiceSource != null)
        {
            float clipVolume = 1.0f;
            if (voiceClipVolumes != null && currentVoiceIndex < voiceClipVolumes.Length)
            {
                clipVolume = voiceClipVolumes[currentVoiceIndex];
            }

            Debug.Log($"Playing voice clip: {selectedVoice.name} (Index: {currentVoiceIndex}, Volume: {clipVolume})");
            voiceSource.PlayOneShot(selectedVoice, voiceVolume * clipVolume);
        }

        currentVoiceIndex++;
        if (currentVoiceIndex >= dedMorozCollisionVoices.Length)
        {
            currentVoiceIndex = 0;
        }
    }

    public void PlayLaneChangeSound()
    {
        PlaySFX(laneChangeSFX);
    }

    public void PlayMonthTransitionSound()
    {
        PlaySFX(monthTransitionSFX);
    }

    public void PlayMoneyPickupSound()
    {
        PlaySFX(moneyPickupSFX);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null && musicSource != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        if (voiceSource != null)
        {
            voiceSource.volume = voiceVolume;
        }
    }
}
