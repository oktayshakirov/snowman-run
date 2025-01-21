using UnityEngine;
using System.Runtime.InteropServices;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Effects")]
    public AudioClip swipeSound;
    public AudioClip jumpSound;
    public AudioClip rampSound;
    public AudioClip coinSound;
    public AudioClip weeSound;
    public AudioClip crashSound;
    public AudioClip gogglesSound;
    public AudioClip offSound;
    public AudioClip buySound;
    public AudioClip equipSound;

    [Header("Sound Volumes")]
    [Range(0f, 1f)] public float swipeSoundVolume = 1f;
    [Range(0f, 1f)] public float jumpSoundVolume = 1f;
    [Range(0f, 1f)] public float rampSoundVolume = 1f;
    [Range(0f, 1f)] public float coinSoundVolume = 1f;
    [Range(0f, 1f)] public float weeSoundVolume = 1f;
    [Range(0f, 1f)] public float crashSoundVolume = 1f;
    [Range(0f, 1f)] public float gogglesSoundVolume = 1f;
    [Range(0f, 1f)] public float offSoundVolume = 1f;
    [Range(0f, 1f)] public float buySoundVolume = 1f;
    [Range(0f, 1f)] public float equipSoundVolume = 1f;

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.3f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private bool isInitialized = false;

    private bool isMusicMuted = false;
    private bool isSfxMuted = false;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SetAVAudioSessionPlayback();
#endif

    public enum SoundType
    {
        Swipe,
        Ramp,
        Jump,
        Coin,
        Wee,
        Crash,
        Goggles,
        Off,
        Buy,
        Equip
    }

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
        InitializeAudioManager();
        if (AdsInitializer.Instance != null)
        {
            AdsInitializer.Instance.OnInitializationCompleteEvent.AddListener(InitializeAudioManager);
        }
    }

    private void InitializeAudioManager()
    {
        if (isInitialized) return;

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = backgroundMusicVolume;
            musicSource.Play();
        }

        LoadSettings();
        isInitialized = true;
    }

    public void PlaySound(SoundType soundType)
    {
        if (isSfxMuted) return;

        AudioClip clip = soundType switch
        {
            SoundType.Swipe => swipeSound,
            SoundType.Jump => jumpSound,
            SoundType.Ramp => rampSound,
            SoundType.Coin => coinSound,
            SoundType.Wee => weeSound,
            SoundType.Crash => crashSound,
            SoundType.Goggles => gogglesSound,
            SoundType.Off => offSound,
            SoundType.Buy => buySound,
            SoundType.Equip => equipSound,
            _ => null
        };

        float volume = soundType switch
        {
            SoundType.Swipe => swipeSoundVolume,
            SoundType.Jump => jumpSoundVolume,
            SoundType.Ramp => rampSoundVolume,
            SoundType.Coin => coinSoundVolume,
            SoundType.Wee => weeSoundVolume,
            SoundType.Crash => crashSoundVolume,
            SoundType.Goggles => gogglesSoundVolume,
            SoundType.Off => offSoundVolume,
            SoundType.Buy => buySoundVolume,
            SoundType.Equip => equipSoundVolume,
            _ => 1f
        };

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
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

    private void LoadSettings()
    {
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        ToggleMusic(!musicEnabled);

        bool soundEffectsEnabled = PlayerPrefs.GetInt("SoundEffectsEnabled", 1) == 1;
        ToggleSFX(!soundEffectsEnabled);
    }
}
