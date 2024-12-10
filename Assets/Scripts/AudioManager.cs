using UnityEngine;
using System.Runtime.InteropServices;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public AudioClip swipeSound;
    [Range(0f, 1f)] public float swipeSoundVolume = 1f;

    public AudioClip jumpSound;
    [Range(0f, 1f)] public float jumpSoundVolume = 1f;

    public AudioClip coinSound;
    [Range(0f, 1f)] public float coinSoundVolume = 1f;

    public AudioClip weeSound;
    [Range(0f, 1f)] public float weeSoundVolume = 1f;

    public AudioClip crashSound;
    [Range(0f, 1f)] public float crashSoundVolume = 1f;

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.3f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private bool isMusicMuted = false;
    private bool isSfxMuted = false;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SetAVAudioSessionPlayback();
#endif

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

#if UNITY_IOS && !UNITY_EDITOR
        SetAVAudioSessionPlayback();
#endif
    }

    private void Start()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = backgroundMusicVolume;
            musicSource.Play();
        }
    }

    public void PlaySound(AudioClip clip, float volume)
    {
        if (!isSfxMuted && clip != null)
        {
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }

    public void PlaySwipeSound()
    {
        PlaySound(swipeSound, swipeSoundVolume);
    }

    public void PlayJumpSound()
    {
        PlaySound(jumpSound, jumpSoundVolume);
    }

    public void PlayCoinSound()
    {
        PlaySound(coinSound, coinSoundVolume);
    }

    public void PlayWeeSound()
    {
        PlaySound(weeSound, weeSoundVolume);
    }

    public void PlayCrashSound()
    {
        PlaySound(crashSound, crashSoundVolume);
    }

    public void ToggleMusic(bool isMuted)
    {
        isMusicMuted = isMuted;
        musicSource.mute = isMuted;
    }

    public void ToggleSFX(bool isMuted)
    {
        isSfxMuted = isMuted;
    }

    public void SetMusicVolume(float volume)
    {
        backgroundMusicVolume = Mathf.Clamp01(volume);
        musicSource.volume = backgroundMusicVolume;
    }
}
