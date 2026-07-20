using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private Player playerMovement;
    [SerializeField] private PlayerCustomization playerCustomization;

    [Header("UI Screens")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject startScreenCanvas;
    [SerializeField] private GameObject gameScreenCanvas;


    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float timeBetweenSpeedIncreases = 2f;
    [SerializeField] private float timeSpeedIncreaseAmount = 1f;
    [SerializeField] private float speedLerpRate = 5f;
    [SerializeField] private Boosters boosters;

    [Header("Start Countdown")]
    [SerializeField] private float countdownStepSeconds = 1f;
    [SerializeField] private float countdownGoSeconds = 0.7f;
    [SerializeField] private float countdownFontSize = 210f;

    private int totalCoins;
    private int currentLevel = 1;
    public bool IsGameActive => !isGameOver && Time.timeScale > 0;
    private bool isGameOver = false;
    private bool isGamePaused = false;
    private int score = 0;
    private float displayedSpeedLerp = 0f;

    private bool useKmh = true;

    private const float SpeedUiInterval = 0.1f;
    private float nextSpeedUiTime;
    private float lastSpeedUiTime;

    private GroundSpawner groundSpawner;
    private CameraFollow cameraFollow;
    // The scene has one repeater per side of the road; every one must be reset.
    private MountainRepeater[] mountainRepeaters;

    // Tracked so a soft reset can stop them; without a scene reload, stale
    // coroutines from the previous run would keep running and stack up.
    private Coroutine initialAccelerationRoutine;
    private Coroutine speedIncreaseRoutine;
    private Coroutine countdownRoutine;
    private TMP_Text countdownText;

    // Set when a run pushes the player past a level threshold; the celebration
    // is shown once the start screen returns.
    private int pendingLevelUpLevel;
    private int pendingLevelUpReward;

    private void Awake()
    {
        if (inst == null)
        {
            inst = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Mobile platforms default to 30 FPS unless a target is set explicitly.
        Application.targetFrameRate = 60;
        UIAnimatorAutoDisable.AttachToFinishedUIAnimators();
    }

    private void Start()
    {
        groundSpawner = FindFirstObjectByType<GroundSpawner>();
        cameraFollow = FindFirstObjectByType<CameraFollow>(FindObjectsInactive.Include);
        mountainRepeaters = FindObjectsByType<MountainRepeater>(FindObjectsSortMode.None);

        playerCustomization.LoadCustomization();
        totalCoins = WalletManager.GetTotalCoins();
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        useKmh = PlayerPrefs.GetInt("SpeedUnit", 0) == 0;
        RefreshSpeedUnit();

        if (Fog.Instance != null)
        {
            Fog.Instance.InitializeFog();
        }
        gameScreenCanvas.SetActive(false);
        // Erase All and Add Coins
        // EraseAllData();
        // WalletManager.AddDeveloperCoins();
    }

    private void Update()
    {
        // Updating the text every frame allocates a string per frame; ~10 Hz is
        // visually identical and removes the GC churn.
        if (!isGameOver && !isGamePaused && Time.time >= nextSpeedUiTime)
        {
            nextSpeedUiTime = Time.time + SpeedUiInterval;
            UpdateSpeedUI();
        }
    }

    public void StartNewGame()
    {
        isGameOver = false;
        isGamePaused = false;
        score = 0;
        coinsText.text = score.ToString();
        playerMovement.gameObject.SetActive(true);
        playerMovement.InitializeSpeed(baseSpeed);
        gameScreenCanvas.SetActive(true);
        RestartSpeedRoutines();
    }

    private void RestartSpeedRoutines()
    {
        if (initialAccelerationRoutine != null) StopCoroutine(initialAccelerationRoutine);
        if (speedIncreaseRoutine != null) StopCoroutine(speedIncreaseRoutine);
        initialAccelerationRoutine = StartCoroutine(InitialAcceleration());
        speedIncreaseRoutine = StartCoroutine(IncreaseSpeedOverTime());

        if (countdownRoutine != null) StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(RunStartCountdown());
    }

    // Blocks input for the first ~3 seconds of a run and shows 3-2-1-GO!
    // in the middle of the screen; controls unlock on GO.
    private IEnumerator RunStartCountdown()
    {
        playerMovement.SetControlsEnabled(false);
        EnsureCountdownText();
        countdownText.gameObject.SetActive(true);

        for (int step = 3; step >= 1; step--)
        {
            yield return CountdownTick(step.ToString(), countdownStepSeconds);
        }

        playerMovement.SetControlsEnabled(true);
        yield return CountdownTick("GO!", countdownGoSeconds);

        countdownText.gameObject.SetActive(false);
        countdownRoutine = null;
    }

    private IEnumerator CountdownTick(string value, float duration)
    {
        countdownText.text = value;

        Transform textTransform = countdownText.transform;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pop = Mathf.Clamp01(elapsed / 0.25f);
            textTransform.localScale = Vector3.one * Mathf.Lerp(1.35f, 1f, pop);
            yield return null;
        }
        textTransform.localScale = Vector3.one;
    }

    private void EnsureCountdownText()
    {
        if (countdownText != null)
            return;

        GameObject go = new GameObject("Countdown", typeof(RectTransform));
        go.transform.SetParent(gameScreenCanvas.transform, false);

        RectTransform rect = (RectTransform)go.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 470f);
        rect.sizeDelta = new Vector2(700f, 320f);

        // Same font, color, and default material as the speed/coins HUD texts.
        TextMeshProUGUI text = go.AddComponent<TextMeshProUGUI>();
        text.font = Resources.Load<TMP_FontAsset>("Fonts & Materials/Numbers SDF");
        text.fontSize = countdownFontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.color = new Color(0.05882353f, 0.05882353f, 0.05882353f, 1f);
        countdownText = text;

        go.SetActive(false);
    }

    public void IncrementScore()
    {
        if (isGameOver) return;

        score++;
        coinsText.text = score.ToString();
        AudioManager.Instance.PlaySound(AudioManager.SoundType.Coin);
        NativeHaptics.TriggerMediumHaptic();
        Fog.Instance?.IncrementFogDensity();
    }

    public void StartLevel(int level)
    {
        currentLevel = Mathf.Max(level, PlayerPrefs.GetInt("CurrentLevel", 1));
        isGameOver = false;
        isGamePaused = false;
        score = 0;
        float levelSpeed = baseSpeed + (currentLevel - 1) * 2f;
        playerMovement.InitializeSpeed(levelSpeed);
        coinsText.text = score.ToString();
        playerMovement.gameObject.SetActive(true);
        RestartSpeedRoutines();
    }

    public void PauseGame()
    {
        if (isGamePaused) return;

        isGamePaused = true;
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
        gameScreenCanvas.SetActive(false);
    }

    public void ResumeGame()
    {
        if (!isGamePaused) return;

        isGamePaused = false;
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        gameScreenCanvas.SetActive(true);
        playerMovement.StartCoroutine(playerMovement.ResumeInputBuffer(0.1f));
    }

    public void ExitToStartScreen()
    {
        isGameOver = true;
        pauseScreen.SetActive(false);
        gameScreenCanvas.SetActive(false);
        playerMovement.EndGame();
        ResetRun();

        if (StartScreenManager.Instance != null)
        {
            StartScreenManager.Instance.ShowStartScreen();
        }
        else
        {
            startScreenCanvas.SetActive(true);
        }
    }

    // Soft reset instead of reloading the scene: puts the player, tiles, fog and
    // camera back to their initial state so a new run can start instantly.
    private void ResetRun()
    {
        score = 0;
        displayedSpeedLerp = 0f;
        coinsText.text = "0";

        if (initialAccelerationRoutine != null) StopCoroutine(initialAccelerationRoutine);
        if (speedIncreaseRoutine != null) StopCoroutine(speedIncreaseRoutine);
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        if (Boosters.Instance != null)
        {
            Boosters.Instance.CancelGoggles();
        }

        playerMovement.ResetForNewRun();

        if (groundSpawner != null)
        {
            groundSpawner.ResetRun();
        }

        foreach (MountainRepeater repeater in mountainRepeaters)
        {
            repeater.ResetRun();
        }

        if (Fog.Instance != null)
        {
            Fog.Instance.InitializeFog();
        }

        if (cameraFollow != null)
        {
            cameraFollow.ResetZoom();
        }
    }

    public void RefreshSpeedUnit()
    {
        useKmh = PlayerPrefs.GetInt("SpeedUnit", 0) == 0;
        UpdateSpeedUI();
    }

    public void SpendCoins(int amount)
    {
        WalletManager.SpendCoins(amount);
        UpdateUnlockedLevels();
    }

    public void OnPlayerCrash()
    {
        if (isGameOver) return;

        NativeHaptics.TriggerErrorNotification();
        isGameOver = true;
        WalletManager.AddCoins(score);
        UpdateUnlockedLevels();
        playerMovement.SetSpeed(0f);
        gameScreenCanvas.SetActive(false);
        AudioManager.Instance.PlaySound(AudioManager.SoundType.Crash);
        StartCoroutine(DelayedStartScreen());
        if (InterstitialAd.Instance != null && !Boosters.IsGameOverInterstitialSuppressed())
            InterstitialAd.Instance.LoadAd();
    }

    private void UpdateSpeedUI()
    {
        float playerSpeed = playerMovement.GetSpeed();
        float elapsed = Mathf.Max(Time.time - lastSpeedUiTime, Time.deltaTime);
        lastSpeedUiTime = Time.time;
        displayedSpeedLerp = Mathf.Lerp(displayedSpeedLerp, playerSpeed, elapsed * speedLerpRate);
        float convertedSpeed = useKmh ? displayedSpeedLerp * 1f : displayedSpeedLerp * 0.7f;
        string unit = useKmh ? "km/h" : "mph";
        speedText.text = $"{convertedSpeed:F1} {unit}";
    }

    private void UpdateUnlockedLevels()
    {
        totalCoins = WalletManager.GetTotalCoins();
        int baseIncrement = 100;
        int unlockedLevels = 1;

        for (int level = 1; ; level++)
        {
            int requiredCoins = baseIncrement * (level - 1) * level;

            if (totalCoins >= requiredCoins)
            {
                unlockedLevels = level;
            }
            else
            {
                break;
            }
        }

        int currentStoredLevel = PlayerPrefs.GetInt("CurrentLevel", 1);

        if (unlockedLevels > currentStoredLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevels", unlockedLevels);
            PlayerPrefs.SetInt("CurrentLevel", unlockedLevels);
            PlayerPrefs.Save();

            // Small, level-scaled bonus: well under one rewarded ad (300) and
            // far below the smallest coin pack (2500), so it stays a treat.
            for (int level = currentStoredLevel + 1; level <= unlockedLevels; level++)
            {
                pendingLevelUpReward += Mathf.Min(25 * level, 250);
            }
            pendingLevelUpLevel = unlockedLevels;
        }
    }

    private IEnumerator InitialAcceleration()
    {
        float accelerationDuration = 1f;
        float elapsedTime = 0f;
        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            UpdateSpeedUI();
            yield return null;
        }
    }

    private IEnumerator IncreaseSpeedOverTime()
    {
        while (!isGameOver && !isGamePaused)
        {
            yield return new WaitForSeconds(timeBetweenSpeedIncreases);
            float newSpeed = Mathf.Min(playerMovement.GetSpeed() + timeSpeedIncreaseAmount, Boosters.Instance.MaxSpeed);
            playerMovement.SetSpeed(newSpeed);
        }
    }

    private IEnumerator DelayedStartScreen()
    {
        yield return new WaitForSeconds(2f);
        ResetRun();
        if (StartScreenManager.Instance != null)
        {
            StartScreenManager.Instance.ShowStartScreen();

            if (pendingLevelUpLevel > 0)
            {
                StartScreenManager.Instance.ShowLevelUpCelebration(pendingLevelUpLevel, pendingLevelUpReward);
                pendingLevelUpLevel = 0;
                pendingLevelUpReward = 0;
            }
        }
    }

    public void EraseAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
