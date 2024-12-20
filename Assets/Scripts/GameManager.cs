using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("UI Screens")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject settingsScreen;
    [SerializeField] private GameObject startScreenCanvas;

    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float timeBetweenSpeedIncreases = 2f;
    [SerializeField] private float timeSpeedIncreaseAmount = 1f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private float speedLerpRate = 5f;

    [Header("Fog Settings")]
    [SerializeField] private float minFogDensity = 0.01f;
    [SerializeField] private float maxFogDensity = 1.75f;
    private int maxCoinsForFog = 100;
    private float currentFogDensity;

    private int totalCoins;
    private int currentLevel = 1;
    public bool IsGameActive => !isGameOver && Time.timeScale > 0;
    private bool isGameOver = false;
    private bool isGamePaused = false;
    private int score = 0;
    private float displayedSpeedLerp = 0f;
    public float MaxSpeed => maxSpeed;

    private bool useKmh = true;

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
    }

    private void Start()
    {
        totalCoins = WalletManager.GetTotalCoins();
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        useKmh = PlayerPrefs.GetInt("SpeedUnit", 0) == 0;
        RefreshSpeedUnit();
        RenderSettings.fog = true;
        RenderSettings.fogDensity = minFogDensity;
        currentFogDensity = minFogDensity;

        StartNewGame();
    }

    private void Update()
    {
        if (!isGameOver && !isGamePaused)
        {
            UpdateSpeedUI();
            SmoothFogTransition();
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
        StartCoroutine(InitialAcceleration());
        StartCoroutine(IncreaseSpeedOverTime());
    }

    public void IncrementScore()
    {
        if (isGameOver) return;

        score++;
        coinsText.text = score.ToString();
        AudioManager.Instance.PlaySound(AudioManager.SoundType.Coin);
        UpdateTargetFogDensity();
    }

    private void UpdateTargetFogDensity()
    {
        float progress = Mathf.Clamp01((float)score / maxCoinsForFog);
        if (progress <= 0.5f)
        {
            currentFogDensity = Mathf.Lerp(minFogDensity, (minFogDensity + maxFogDensity) / 2, progress * 2);
        }
        else
        {
            currentFogDensity = Mathf.Lerp((minFogDensity + maxFogDensity) / 2, maxFogDensity, (progress - 0.5f) * 2);
        }
    }

    private void SmoothFogTransition()
    {
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, currentFogDensity, Time.deltaTime * 0.5f);
    }

    public void OnPlayerCrash()
    {
        if (isGameOver) return;

        isGameOver = true;
        WalletManager.AddCoins(score);
        UpdateUnlockedLevels();
        playerMovement.SetSpeed(0f);
        AudioManager.Instance.PlaySound(AudioManager.SoundType.Crash);
        AdManager.Instance.LoadAd();
        HapticFeedback.TriggerHapticFeedback();

        if (AdManager.Instance != null && AdManager.Instance.IsAdReady())
        {
            AdManager.Instance.ShowAd(() =>
            {
                StartCoroutine(DelayedStartScreen());
            });
        }
        else
        {
            StartCoroutine(DelayedStartScreen());
        }
    }



    public void SpendCoins(int amount)
    {
        WalletManager.SpendCoins(amount);
        UpdateUnlockedLevels();
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
        }
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
        StartCoroutine(InitialAcceleration());
        StartCoroutine(IncreaseSpeedOverTime());
    }

    private IEnumerator DelayedStartScreen()
    {
        yield return new WaitForSeconds(2f);
        if (StartScreenManager.Instance != null)
        {
            StartScreenManager.Instance.ShowStartScreen();
        }
    }

    private void UpdateSpeedUI()
    {
        float playerSpeed = playerMovement.GetSpeed();
        displayedSpeedLerp = Mathf.Lerp(displayedSpeedLerp, playerSpeed, Time.deltaTime * speedLerpRate);
        float convertedSpeed = useKmh ? displayedSpeedLerp * 1.2f : displayedSpeedLerp * 0.746f;
        string unit = useKmh ? "km/h" : "mph";
        speedText.text = $"{convertedSpeed:F1} {unit}";
    }

    public void RefreshSpeedUnit()
    {
        useKmh = PlayerPrefs.GetInt("SpeedUnit", 0) == 0;
        UpdateSpeedUI();
    }

    public void PauseGame()
    {
        if (isGamePaused) return;

        isGamePaused = true;
        Time.timeScale = 0;
        pauseScreen.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isGamePaused) return;

        isGamePaused = false;
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
    }

    public void ExitToStartScreen()
    {
        isGamePaused = false;
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        startScreenCanvas.SetActive(true);
    }

    public void OpenSettings()
    {
        pauseScreen.SetActive(false);
        settingsScreen.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsScreen.SetActive(false);
        pauseScreen.SetActive(true);
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
            float newSpeed = Mathf.Min(playerMovement.GetSpeed() + timeSpeedIncreaseAmount, maxSpeed);
            playerMovement.SetSpeed(newSpeed);
        }
    }
}