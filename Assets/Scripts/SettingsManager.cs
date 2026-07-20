using System;
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

    [Header("Contact & Privacy")]
    [SerializeField] private string contactEmail = "snowman-run@oktayshakirov.com";
    [SerializeField] private string privacyPolicyUrl = "https://oktayshakirov.com/privacy-policy/snowman-run";

    private GameObject previousMenu;

    private void Start()
    {
        LoadSettings();
        BuildFooterLinks();
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

    // Two small links pinned at the bottom of the settings screen, styled from
    // a real settings label so they match the rest of the dialog.
    private void BuildFooterLinks()
    {
        TMP_Text style = FindSettingsLabelStyle();
        CreateFooterLink("Contact Us", "Contact Us", new Vector2(-180f, 120f),
            style, OpenContactEmail);
        CreateFooterLink("Privacy Policy", "Privacy Policy", new Vector2(180f, 120f),
            style, OpenPrivacyPolicy);
    }

    private void CreateFooterLink(string name, string label, Vector2 position,
        TMP_Text style, Action onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(350f, 60f);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = style != null ? style.font : Resources.Load<TMP_FontAsset>("Fonts & Materials/Numbers SDF");
        if (style != null)
            text.fontSharedMaterial = style.fontSharedMaterial;
        text.text = label;
        text.fontSize = 34f;
        text.enableWordWrapping = false;
        text.alignment = TextAlignmentOptions.Center;
        text.color = style != null ? style.color : SettingsRed;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = text;
        button.onClick.AddListener(() => onClick());
    }

    private static readonly Color SettingsRed = new Color(0.909804f, 0.007843138f, 0.039215688f, 1f);

    // A real settings label, used as the style source so the footer links keep
    // matching the rest of the screen if its look is ever changed.
    private TMP_Text FindSettingsLabelStyle()
    {
        Toggle[] sources = { soundEffectsToggle, vibrationToggle, musicToggle };
        foreach (Toggle source in sources)
        {
            if (source == null)
                continue;
            TMP_Text label = source.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
                return label;
        }
        return null;
    }

    public void OpenPrivacyPolicy()
    {
        Application.OpenURL(privacyPolicyUrl);
    }

    // One generic template covering bug reports, feedback and business mail.
    // The device/version footer keeps support replies actionable.
    public void OpenContactEmail()
    {
        string body = "Hello Snowman Run developer,\n\n" +
                      "I'm contacting you because of \n\n\n" +
                      "--- App info ---\n" +
                      $"Version: {Application.version}\n" +
                      $"Device: {SystemInfo.deviceModel}\n" +
                      $"OS: {SystemInfo.operatingSystem}\n";
        SendMail("[Contact] Snowman Run", body);
    }

    private void SendMail(string subject, string body)
    {
        // The OS resolves mailto: to the user's mail app (or app chooser).
        Application.OpenURL($"mailto:{contactEmail}" +
                            $"?subject={Uri.EscapeDataString(subject)}" +
                            $"&body={Uri.EscapeDataString(body)}");
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
