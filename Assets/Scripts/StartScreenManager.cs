using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class StartScreenManager : MonoBehaviour
{
    public static StartScreenManager Instance;

    [SerializeField] private GameObject startScreenCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject settingsScreenCanvas;
    [SerializeField] private GameObject shopScreenCanvas;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text currentLevelText;

    [Header("Camera Reference")]
    [SerializeField] private PreviewCameraController previewCamera;

    [Header("Level Up Celebration")]
    [SerializeField] private float celebrationSeconds = 4f;

    private GameObject celebrationPanel;
    private TMP_Text celebrationTitle;
    private TMP_Text celebrationDetails;
    private Coroutine celebrationRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }
        ShowStartScreen();
    }

    private void OnEnable()
    {
        WalletManager.OnCoinsChanged += HandleCoinsChanged;
    }

    private void OnDisable()
    {
        WalletManager.OnCoinsChanged -= HandleCoinsChanged;
    }

    private void HandleCoinsChanged(int totalCoins)
    {
        UpdateUI();
    }

    public void ShowStartScreen()
    {
        startScreenCanvas.SetActive(true);
        HideLevelUpCelebration();
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(false);
        }

        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(false);
        }

        if (shopScreenCanvas != null)
        {
            shopScreenCanvas.SetActive(false);
        }

        Time.timeScale = 0;
        UpdateUI();

        if (previewCamera != null)
        {
            previewCamera.ResetCameraPosition();
            previewCamera.MoveCameraToPreviewPosition();
        }

        StartCoroutine(RefreshRewardedAdButtonDelayed());
    }

    private IEnumerator RefreshRewardedAdButtonDelayed()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (RewardedAdButton.Instance != null)
        {
            RewardedAdButton.Instance.RefreshButtonVisibility();
        }
    }

    public void StartGame()
    {
        if (gameCanvas != null)
        {
            gameCanvas.SetActive(true);
        }

        startScreenCanvas.SetActive(false);
        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(false);
        }

        if (shopScreenCanvas != null)
        {
            shopScreenCanvas.SetActive(false);
        }

        AudioManager.Instance.PlaySound(AudioManager.SoundType.Wee);
        Time.timeScale = 1;
        GameManager.inst.StartNewGame();
    }

    public void OpenSettings()
    {
        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(true);
        }

        startScreenCanvas.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsScreenCanvas != null)
        {
            settingsScreenCanvas.SetActive(false);
        }

        startScreenCanvas.SetActive(true);
    }

    public void OpenShop()
    {
        if (shopScreenCanvas != null)
        {
            shopScreenCanvas.SetActive(true);
        }
        startScreenCanvas.SetActive(false);

        if (RevenueCatManager.Instance != null)
            RevenueCatManager.Instance.PresentCoinPaywallAfterShopOpened();
    }

    private void UpdateUI()
    {
        int totalCoins = WalletManager.GetTotalCoins();
        totalCoinsText.text = $"{totalCoins}";
        int currentLevel = PlayerPrefs.GetInt("UnlockedLevels", 1);
        currentLevelText.text = $"Level {currentLevel}";
    }

    // Shown after returning from a run that unlocked a new level; grants the
    // level bonus and celebrates it. Built at runtime with the game font.
    public void ShowLevelUpCelebration(int newLevel, int rewardCoins)
    {
        EnsureCelebrationPanel();

        celebrationTitle.text = $"LEVEL {newLevel}!";
        celebrationDetails.text = $"Congratulations!\n+{rewardCoins} coins";

        if (rewardCoins > 0)
        {
            WalletManager.AddCoins(rewardCoins);
        }

        AudioManager.Instance?.PlaySound(AudioManager.SoundType.Coin);
        NativeHaptics.TriggerSuccessNotification();

        if (celebrationRoutine != null)
        {
            StopCoroutine(celebrationRoutine);
        }
        celebrationRoutine = StartCoroutine(CelebrationRoutine());
    }

    private void HideLevelUpCelebration()
    {
        if (celebrationRoutine != null)
        {
            StopCoroutine(celebrationRoutine);
            celebrationRoutine = null;
        }
        if (celebrationPanel != null)
        {
            celebrationPanel.SetActive(false);
        }
    }

    private IEnumerator CelebrationRoutine()
    {
        celebrationPanel.SetActive(true);

        // Pop-in; realtime because the start screen runs with Time.timeScale = 0.
        Transform panelTransform = celebrationPanel.transform;
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / 0.25f);
            panelTransform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1f, k);
            yield return null;
        }
        panelTransform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(celebrationSeconds);

        celebrationPanel.SetActive(false);
        celebrationRoutine = null;
    }

    private void EnsureCelebrationPanel()
    {
        if (celebrationPanel != null)
            return;

        // Same font and red as the settings/pause dialog titles.
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Numbers SDF");
        Color dialogRed = new Color(0.909804f, 0.007843138f, 0.039215688f, 1f);

        celebrationPanel = new GameObject("LevelUpCelebration", typeof(RectTransform));
        celebrationPanel.transform.SetParent(startScreenCanvas.transform, false);

        RectTransform panelRect = (RectTransform)celebrationPanel.transform;
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 190f);
        panelRect.sizeDelta = new Vector2(860f, 560f);

        Image backdrop = celebrationPanel.AddComponent<Image>();
        backdrop.raycastTarget = false;

        // Reuse the exact bubble the settings dialog uses, so the celebration
        // matches the dialogs even if their styling changes later.
        Image bubble = FindSettingsBubble();
        if (bubble != null)
        {
            backdrop.sprite = bubble.sprite;
            backdrop.type = bubble.type;
            backdrop.preserveAspect = bubble.preserveAspect;
            backdrop.color = bubble.color;
        }
        else
        {
            backdrop.color = new Color(1f, 1f, 1f, 0.3f);
        }

        celebrationTitle = CreateCelebrationText("Title", font, dialogRed, 96f, new Vector2(0f, 80f));
        celebrationDetails = CreateCelebrationText("Details", font, dialogRed, 44f, new Vector2(0f, -70f));
    }

    private Image FindSettingsBubble()
    {
        if (settingsScreenCanvas == null)
            return null;

        foreach (Image image in settingsScreenCanvas.GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject.name == "Background" && image.sprite != null)
                return image;
        }
        return null;
    }

    private TMP_Text CreateCelebrationText(string name, TMP_FontAsset font, Color color, float size, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(celebrationPanel.transform, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(760f, 180f);

        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        if (font != null)
            text.font = font;
        text.fontSize = size;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.color = color;
        return text;
    }
}
