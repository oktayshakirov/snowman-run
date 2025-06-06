using UnityEngine;
using System.Collections;

public class Boosters : MonoBehaviour
{
    public static Boosters Instance { get; private set; }

    [Header("Booster Settings")]
    [SerializeField] private float defaultGogglesFogReduction = 0.5f;
    [SerializeField] private float defaultGogglesDuration = 5f;
    [SerializeField] private float defaultMaxSpeed = 35f;
    private float gogglesFogReduction;
    private float gogglesDuration;
    private float maxSpeed;
    public float GogglesDuration => gogglesDuration;
    public float MaxSpeed
    {
        get => maxSpeed;
        set
        {
            maxSpeed = value;
            PlayerPrefs.SetFloat("MaxSpeed", maxSpeed);
            PlayerPrefs.Save();
        }
    }

    [Header("Timer Settings")]
    [SerializeField] private Timer circleTimer;

    private bool gogglesActive = false;
    private float fogDensityAtActivation;
    private Fog Fog => Fog.Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        gogglesDuration = PlayerPrefs.GetFloat("GogglesDuration", defaultGogglesDuration);
        gogglesFogReduction = PlayerPrefs.GetFloat("GogglesFogReduction", defaultGogglesFogReduction);
        maxSpeed = PlayerPrefs.GetFloat("MaxSpeed", defaultMaxSpeed);
        if (circleTimer != null)
        {
            circleTimer.gameObject.SetActive(false);
            circleTimer.seconds = (int)gogglesDuration;
            circleTimer.countMethod = Timer.CountMethod.CountDown;
            circleTimer.outputType = Timer.OutputType.StandardText;
        }

        Debug.Log($"Loaded Boosters: GogglesDuration={gogglesDuration}, GogglesFogReduction={gogglesFogReduction}, MaxSpeed={maxSpeed}");
    }

    public void ActivateGoggles()
    {
        if (gogglesActive || Fog == null)
        {
            return;
        }

        gogglesActive = true;

        if (circleTimer != null)
        {
            circleTimer.gameObject.SetActive(true);
            circleTimer.StartTimer();
            circleTimer.onTimerEnd.AddListener(OnTimerEnd);
        }
        AudioManager.Instance.PlaySound(AudioManager.SoundType.Goggles);
        Fog.HoldFogIncrement(true);
        fogDensityAtActivation = Fog.GetCurrentFogDensity();
        Fog.ReduceFogDensity(gogglesFogReduction);
        StartCoroutine(GogglesCooldown());
    }

    public void UpgradeGogglesDuration(float additionalDuration)
    {
        gogglesDuration += additionalDuration;
        PlayerPrefs.SetFloat("GogglesDuration", gogglesDuration);
        PlayerPrefs.Save();
    }

    public void UpgradeGogglesFogReduction(float additionalReduction)
    {
        gogglesFogReduction += additionalReduction;
        PlayerPrefs.SetFloat("GogglesFogReduction", gogglesFogReduction);
        PlayerPrefs.Save();
    }

    public void UpgradeMaxSpeed(float additionalSpeed)
    {
        MaxSpeed += additionalSpeed;
        Debug.Log($"Max speed upgraded to {MaxSpeed}!");
    }

    private void OnTimerEnd()
    {
        Debug.Log("Timer finished!");
    }

    private IEnumerator GogglesCooldown()
    {
        float remainingTime = gogglesDuration;

        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            if (circleTimer != null)
            {
                circleTimer.timeRemaining = remainingTime;
                circleTimer.DisplayInTextObject();
            }

            yield return null;
        }

        if (circleTimer != null)
        {
            circleTimer.StopTimer();
            circleTimer.gameObject.SetActive(false);
        }

        Fog.RestoreFogDensityTo(fogDensityAtActivation);
        Fog.HoldFogIncrement(false);
        gogglesActive = false;

        GroundSpawner spawner = Object.FindFirstObjectByType<GroundSpawner>();
        if (spawner != null)
        {
            spawner.ShowAllGoggles();
        }

        AudioManager.Instance.PlaySound(AudioManager.SoundType.Off);
    }

    public void ApplyBoosterUpgrade(BoosterData boosterData)
    {
        switch (boosterData.boosterType)
        {
            case BoosterData.BoosterType.GogglesDuration:
                UpgradeGogglesDuration(boosterData.upgradeIncrement);
                break;

            case BoosterData.BoosterType.GogglesFogReduction:
                UpgradeGogglesFogReduction(boosterData.upgradeIncrement);
                break;

            case BoosterData.BoosterType.MaxSpeed:
                UpgradeMaxSpeed(boosterData.upgradeIncrement);
                break;

            default:
                Debug.LogWarning($"Booster type {boosterData.boosterType} not handled!");
                break;
        }
    }
}
