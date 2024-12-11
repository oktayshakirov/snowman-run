using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Toggles")]
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundEffectsToggle;
    [SerializeField] private Toggle vibrationToggle;

    [Header("UI Dropdowns")]
    [SerializeField] private TMP_Dropdown speedUnitDropdown;

    private GameObject previousMenu;

    private void Start()
    {
        LoadSettings();
    }

    public void OpenSettings(GameObject originatingMenu)
    {
        previousMenu = originatingMenu;
        gameObject.SetActive(true);
        if (previousMenu != null)
        {
            previousMenu.SetActive(false);
        }
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
        if (previousMenu != null)
        {
            previousMenu.SetActive(true);
        }
    }

    public void ToggleMusic(bool isEnabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic(!isEnabled);
        }
        PlayerPrefs.SetInt("MusicEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSoundEffects(bool isEnabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSFX(!isEnabled);
        }
        PlayerPrefs.SetInt("SoundEffectsEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleVibration(bool isEnabled)
    {
        PlayerPrefs.SetInt("VibrationEnabled", isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSpeedUnit(int selectedIndex)
    {
        PlayerPrefs.SetInt("SpeedUnit", selectedIndex);
        PlayerPrefs.Save();
        if (GameManager.inst != null)
        {
            GameManager.inst.RefreshSpeedUnit();
        }
    }

    private void LoadSettings()
    {
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        if (musicToggle != null)
        {
            musicToggle.isOn = musicEnabled;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusic(!musicEnabled);
        }
        bool soundEffectsEnabled = PlayerPrefs.GetInt("SoundEffectsEnabled", 1) == 1;
        if (soundEffectsToggle != null)
        {
            soundEffectsToggle.isOn = soundEffectsEnabled;
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSFX(!soundEffectsEnabled);
        }
        bool vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = vibrationEnabled;
        }
        int speedUnitIndex = PlayerPrefs.GetInt("SpeedUnit", 0);
        if (speedUnitDropdown != null)
        {
            speedUnitDropdown.value = speedUnitIndex;
        }
        AddListeners();
    }

    private void AddListeners()
    {
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.AddListener(ToggleMusic);
        }

        if (soundEffectsToggle != null)
        {
            soundEffectsToggle.onValueChanged.AddListener(ToggleSoundEffects);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.AddListener(ToggleVibration);
        }

        if (speedUnitDropdown != null)
        {
            speedUnitDropdown.onValueChanged.AddListener(SetSpeedUnit);
        }
    }

    private void OnDestroy()
    {
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(ToggleMusic);
        }

        if (soundEffectsToggle != null)
        {
            soundEffectsToggle.onValueChanged.RemoveListener(ToggleSoundEffects);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.RemoveListener(ToggleVibration);
        }

        if (speedUnitDropdown != null)
        {
            speedUnitDropdown.onValueChanged.RemoveListener(SetSpeedUnit);
        }
    }
}
