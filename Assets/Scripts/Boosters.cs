using UnityEngine;
using System.Collections;

public class Boosters : MonoBehaviour
{
    [Header("Booster Settings")]
    [SerializeField] private float gogglesFogReduction = 0.5f;
    [SerializeField] private float gogglesDuration = 5f;
    public float GogglesDuration => gogglesDuration;

    [Header("Timer Settings")]
    [SerializeField] private Timer circleTimer;
    private bool gogglesActive = false;
    private float fogDensityAtActivation;
    private Fog Fog => Fog.Instance;

    private void Start()
    {
        if (circleTimer != null)
        {
            circleTimer.gameObject.SetActive(false);
            circleTimer.hours = 0;
            circleTimer.minutes = 0;
            circleTimer.seconds = (int)gogglesDuration;
            circleTimer.countMethod = Timer.CountMethod.CountDown;
            circleTimer.outputType = Timer.OutputType.StandardText;
        }
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

        Fog.HoldFogIncrement(true);
        fogDensityAtActivation = Fog.GetCurrentFogDensity();
        Fog.ReduceFogDensity(gogglesFogReduction);
        StartCoroutine(GogglesCooldown());
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
}
