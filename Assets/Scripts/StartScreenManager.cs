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

    [Tooltip("Optional: style your own panel in the scene and assign it here. " +
             "Leave empty to use the built-in runtime panel.")]
    [SerializeField] private GameObject celebrationPanelOverride;
    [Tooltip("Title text of the assigned panel, e.g. \"LEVEL 5!\".")]
    [SerializeField] private TMP_Text celebrationTitleOverride;
    [Tooltip("Details text of the assigned panel, e.g. \"Congratulations!\\n+125 coins\".")]
    [SerializeField] private TMP_Text celebrationDetailsOverride;

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

        // A panel styled in the scene wins; the runtime one is just a fallback.
        if (celebrationPanelOverride != null &&
            celebrationTitleOverride != null &&
            celebrationDetailsOverride != null)
        {
            celebrationPanel = celebrationPanelOverride;
            celebrationTitle = celebrationTitleOverride;
            celebrationDetails = celebrationDetailsOverride;
            celebrationPanel.SetActive(false);
            return;
        }

        BuildCelebrationPanel(out celebrationPanel, out celebrationTitle, out celebrationDetails);
    }

    // Builds the panel hierarchy. Used both at runtime and by the editor
    // context menu that bakes an editable copy into the scene.
    private void BuildCelebrationPanel(out GameObject panel, out TMP_Text title, out TMP_Text details)
    {
        // Same font and red as the settings/pause dialog titles.
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Numbers SDF");
        Color dialogRed = new Color(0.909804f, 0.007843138f, 0.039215688f, 1f);

        panel = new GameObject("LevelUpCelebration", typeof(RectTransform));
        panel.transform.SetParent(startScreenCanvas.transform, false);

        RectTransform panelRect = (RectTransform)panel.transform;
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, 190f);
        panelRect.sizeDelta = new Vector2(860f, 560f);

        Image backdrop = panel.AddComponent<Image>();
        backdrop.raycastTarget = false;

        // Reuse the bubble sprite the settings dialog uses, but fully opaque.
        // The dialog can be translucent because it sits on its own solid
        // background; this one floats over the start screen and must be solid
        // on its own for the text to stay readable.
        Image bubble = FindSettingsBubble();
        if (bubble != null)
        {
            backdrop.sprite = bubble.sprite;
            backdrop.type = bubble.type;
            backdrop.preserveAspect = bubble.preserveAspect;
        }
        backdrop.color = Color.white;

        title = CreateCelebrationText(panel, "Title", font, dialogRed, 96f, new Vector2(0f, 80f));
        details = CreateCelebrationText(panel, "Details", font, dialogRed, 44f, new Vector2(0f, -70f));
    }

#if UNITY_EDITOR
    // Right-click the StartScreenManager component header -> this menu item.
    // Creates a real, editable copy of the panel in the scene and wires it to
    // the override fields, so the look can be designed in the editor.
    [ContextMenu("Create Level Up Panel In Scene")]
    private void CreateLevelUpPanelInScene()
    {
        if (celebrationPanelOverride != null)
        {
            Debug.LogWarning("A celebration panel is already assigned. Delete it first to recreate.", this);
            return;
        }

        BuildCelebrationPanel(out GameObject panel, out TMP_Text title, out TMP_Text details);

        // Sample text so the panel is easy to judge in the editor; both strings
        // are overwritten at runtime.
        title.text = "LEVEL 5!";
        details.text = "Congratulations!\n+125 coins";

        celebrationPanelOverride = panel;
        celebrationTitleOverride = title;
        celebrationDetailsOverride = details;

        UnityEditor.Undo.RegisterCreatedObjectUndo(panel, "Create Level Up Panel");
        UnityEditor.Selection.activeGameObject = panel;
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);

        Debug.Log("Level up panel created under the start screen. Style it, then " +
                  "deactivate it - the game shows it automatically on level up.", panel);
    }
#endif

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

    private TMP_Text CreateCelebrationText(GameObject parent, string name, TMP_FontAsset font, Color color, float size, Vector2 position)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);

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
