using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager inst;

    [SerializeField] private TMP_Text coinsText;  
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Speed Settings")]
    [SerializeField] private float baseSpeed = 10f;
    [SerializeField] private float speedIncreaseAmount = 0.5f;
    [SerializeField] private float timeBetweenSpeedIncreases = 2f;
    [SerializeField] private float timeSpeedIncreaseAmount = 1f;
    [SerializeField] private float maxSpeed = 40f;
    [SerializeField] private float speedLerpRate = 5f;

    private float currentSpeed = 0f;
    private bool isGameOver = false;
    private int score = 0;  

    public float BaseSpeed => baseSpeed;
    public float MaxSpeed => maxSpeed;
    public float SpeedIncreaseAmount => speedIncreaseAmount;
    public float TimeBetweenSpeedIncreases => timeBetweenSpeedIncreases;
    public float TimeSpeedIncreaseAmount => timeSpeedIncreaseAmount;

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
        StartNewGame();
    }

    private void Update()
    {
        if (!isGameOver)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, playerMovement.GetSpeed(), Time.deltaTime * speedLerpRate);
            UpdateSpeedUI();
        }
    }

    public void StartNewGame()
    {
        isGameOver = false;
        score = 0;
        coinsText.text = score.ToString();
        playerMovement.gameObject.SetActive(true);
        playerMovement.InitializeSpeed(BaseSpeed);

        currentSpeed = BaseSpeed;
        StartCoroutine(InitialAcceleration());
        StartCoroutine(IncreaseSpeedOverTime());
    }

    public void IncrementScore()
    {
        if (isGameOver) return;

        score++;
        coinsText.text = score.ToString();
        float newSpeed = Mathf.Min(playerMovement.GetSpeed() + SpeedIncreaseAmount, MaxSpeed);
        playerMovement.SetSpeed(newSpeed);
    }

    public void OnPlayerCrash()
    {
        if (isGameOver) return;

        isGameOver = true;
        WalletManager.AddCoins(score);

        playerMovement.SetSpeed(0f);
        currentSpeed = 0f;
        UpdateSpeedUI();
        StartCoroutine(DelayedStartScreen());
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
        float speedInKmH = currentSpeed * 1.2f;
        speedText.text = $"{speedInKmH:F1} km/h";
    }

    private IEnumerator InitialAcceleration()
    {
        float accelerationDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < accelerationDuration)
        {
            elapsedTime += Time.deltaTime;
            currentSpeed = Mathf.Lerp(0f, BaseSpeed, elapsedTime / accelerationDuration);
            UpdateSpeedUI();
            yield return null;
        }

        currentSpeed = BaseSpeed;
    }

    private IEnumerator IncreaseSpeedOverTime()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(TimeBetweenSpeedIncreases);
            float newSpeed = Mathf.Min(playerMovement.GetSpeed() + TimeSpeedIncreaseAmount, MaxSpeed);
            playerMovement.SetSpeed(newSpeed);
        }
    }
}