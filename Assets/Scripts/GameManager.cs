using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    int score;
    public static GameManager inst;

    [SerializeField] TMP_Text coinsText;
    [SerializeField] TMP_Text speedText;
    [SerializeField] PlayerMovement playerMovement;

    [SerializeField] float baseSpeed = 1f;
    [SerializeField] float speedIncreaseAmount = 1f;
    [SerializeField] float speedLerpRate = 0.5f;

    private float currentSpeed;
    private bool isGameOver = false;
    private bool isAccelerating = true;

    public void IncrementScore()
    {
        if (isGameOver) return;

        score++;
        coinsText.text = score.ToString();
        playerMovement.speed += speedIncreaseAmount;
    }

    public void OnPlayerCrash()
    {
        isGameOver = true;
        playerMovement.speed = 0;
        currentSpeed = 0;
        UpdateSpeedUI();
    }

    private void Awake()
    {
        inst = this;
    }

    private void Start()
    {
        currentSpeed = 0;
        playerMovement.speed = baseSpeed;
        StartCoroutine(InitialAcceleration());
    }

    private void Update()
    {
        if (!isGameOver && !isAccelerating)
        {
            UpdateSpeedUI();
        }
    }

    private void UpdateSpeedUI()
    {
        if (isGameOver)
        {
            speedText.text = "0.0 km/h";
            return;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, playerMovement.speed, Time.deltaTime * speedLerpRate);
        float speedInKmH = currentSpeed * 3.6f;
        speedText.text = $"{speedInKmH:F1} km/h";
    }

    private IEnumerator InitialAcceleration()
    {
        float duration = 3f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            currentSpeed = Mathf.Lerp(0, baseSpeed, elapsedTime / duration);
            float speedInKmH = currentSpeed * 3.6f;
            speedText.text = $"{speedInKmH:F1} km/h";
            yield return null;
        }

        currentSpeed = baseSpeed;
        isAccelerating = false;
    }
}