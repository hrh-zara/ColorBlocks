using UnityEngine;

/// <summary>
/// Manages all game audio - sound effects and music
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip placeSound;
    [SerializeField] private AudioClip cancelSound;
    [SerializeField] private AudioClip lineClearSound;
    [SerializeField] private AudioClip comboSound;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip undoSound;
    [SerializeField] private AudioClip achievementSound;
    [SerializeField] private AudioClip newHighScoreSound;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("Settings")]
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float musicVolume = 0.5f;

    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";

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
            return;
        }

        LoadVolumeSettings();
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    /// <summary>
    /// Load saved volume settings
    /// </summary>
    private void LoadVolumeSettings()
    {
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.5f);

        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stop background music
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // Sound effect methods
    public void PlayPickup()
    {
        PlaySound(pickupSound);
    }

    public void PlayPlace()
    {
        PlaySound(placeSound);
    }

    public void PlayCancel()
    {
        PlaySound(cancelSound);
    }

    public void PlayLineClear()
    {
        PlaySound(lineClearSound);
    }

    public void PlayCombo()
    {
        PlaySound(comboSound);
    }

    public void PlayGameOver()
    {
        PlaySound(gameOverSound);
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

    public void PlayUndo()
    {
        PlaySound(undoSound);
    }

    public void PlayAchievement()
    {
        PlaySound(achievementSound);
    }

    public void PlayNewHighScore()
    {
        PlaySound(newHighScoreSound);
    }

    /// <summary>
    /// Play a sound effect
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// Play a sound at specific pitch (for variety)
    /// </summary>
    public void PlaySoundWithPitch(AudioClip clip, float pitch)
    {
        if (sfxSource != null && clip != null)
        {
            float originalPitch = sfxSource.pitch;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, sfxVolume);
            sfxSource.pitch = originalPitch;
        }
    }

    // Getters for UI
    public float GetSFXVolume() => sfxVolume;
    public float GetMusicVolume() => musicVolume;

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
