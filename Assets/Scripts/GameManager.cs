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

    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float timeBetweenSpeedIncreases = 2f;
    [SerializeField] private float timeSpeedIncreaseAmount = 1f;
    [SerializeField] private float speedLerpRate = 5f;
    [SerializeField] private Boosters boosters;

    private int totalCoins;
    private int currentLevel = 1;
    public bool IsGameActive => !isGameOver && Time.timeScale > 0;
    private bool isGameOver = false;
    private bool isGamePaused = false;
    private int score = 0;
    private float displayedSpeedLerp = 0f;

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
        playerCustomization.LoadCustomization();
        totalCoins = WalletManager.GetTotalCoins();
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        useKmh = PlayerPrefs.GetInt("SpeedUnit", 0) == 0;
        RefreshSpeedUnit();

        if (Fog.Instance != null)
        {
            Fog.Instance.InitializeFog();
        }

        StartNewGame();

        // Erase All and Add Coins
        // EraseAllData();
        // WalletManager.AddDeveloperCoins();
    }

    private void Update()
    {
        if (!isGameOver && !isGamePaused)
        {
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
        StartCoroutine(InitialAcceleration());
        StartCoroutine(IncreaseSpeedOverTime());
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
        StartCoroutine(InitialAcceleration());
        StartCoroutine(IncreaseSpeedOverTime());
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
        playerMovement.StartCoroutine(playerMovement.ResumeInputBuffer(0.1f));
    }

    public void ExitToStartScreen()
    {
        isGameOver = true;
        pauseScreen.SetActive(false);
        startScreenCanvas.SetActive(true);
        playerMovement.EndGame();
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
        AudioManager.Instance.PlaySound(AudioManager.SoundType.Crash);
        StartCoroutine(DelayedStartScreen());
        InterstitialAd.Instance.LoadAd();
    }

    private void UpdateSpeedUI()
    {
        float playerSpeed = playerMovement.GetSpeed();
        displayedSpeedLerp = Mathf.Lerp(displayedSpeedLerp, playerSpeed, Time.deltaTime * speedLerpRate);
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
        if (StartScreenManager.Instance != null)
        {
            StartScreenManager.Instance.ShowStartScreen();
        }
    }

    public void EraseAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
