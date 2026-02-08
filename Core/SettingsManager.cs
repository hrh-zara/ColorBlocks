using UnityEngine;
using UnityEngine.Events;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public UnityEvent OnSettingsChanged;

    // Settings
    private bool soundEnabled = true;
    private bool musicEnabled = true;
    private bool vibrationEnabled = true;

    private const string SOUND_KEY = "SoundEnabled";
    private const string MUSIC_KEY = "MusicEnabled";
    private const string VIBRATION_KEY = "VibrationEnabled";

    public bool SoundEnabled => soundEnabled;
    public bool MusicEnabled => musicEnabled;
    public bool VibrationEnabled => vibrationEnabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (OnSettingsChanged == null)
                OnSettingsChanged = new UnityEvent();
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadSettings()
    {
        soundEnabled = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
        musicEnabled = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(SOUND_KEY, soundEnabled ? 1 : 0);
        PlayerPrefs.SetInt(MUSIC_KEY, musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt(VIBRATION_KEY, vibrationEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSoundEnabled(bool enabled)
    {
        soundEnabled = enabled;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetMusicEnabled(bool enabled)
    {
        musicEnabled = enabled;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetVibrationEnabled(bool enabled)
    {
        vibrationEnabled = enabled;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void ToggleSound()
    {
        SetSoundEnabled(!soundEnabled);
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!musicEnabled);
    }

    public void ToggleVibration()
    {
        SetVibrationEnabled(!vibrationEnabled);
    }

    public void ResetToDefaults()
    {
        soundEnabled = true;
        musicEnabled = true;
        vibrationEnabled = true;
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
