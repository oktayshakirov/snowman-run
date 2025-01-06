using UnityEngine;

public class Fog : MonoBehaviour
{
    public static Fog Instance;
    [SerializeField] private float minFogDensity = 0.01f;
    [SerializeField] private float maxFogDensity = 0.15f;
    [SerializeField] private float fogIncrementStep = 0.001f;
    [SerializeField] private float fogLerpSpeed = 0.5f;

    private float currentFogDensity;
    private bool holdFogIncrement = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        SmoothFogTransition();
    }

    public void InitializeFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogDensity = minFogDensity;
        currentFogDensity = minFogDensity;
    }

    public void IncrementFogDensity()
    {
        if (holdFogIncrement)
        {
            return;
        }
        currentFogDensity += fogIncrementStep;
        currentFogDensity = Mathf.Clamp(currentFogDensity, minFogDensity, maxFogDensity);
    }

    public void ReduceFogDensity(float reductionPercent)
    {
        currentFogDensity = RenderSettings.fogDensity * (1 - reductionPercent);
        currentFogDensity = Mathf.Clamp(currentFogDensity, minFogDensity, maxFogDensity);
    }

    public void RestoreFogDensityTo(float previousDensity)
    {
        currentFogDensity = Mathf.Clamp(previousDensity, minFogDensity, maxFogDensity);
    }

    public float GetCurrentFogDensity()
    {
        return currentFogDensity;
    }

    public void HoldFogIncrement(bool hold)
    {
        holdFogIncrement = hold;
    }

    private void SmoothFogTransition()
    {
        RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, currentFogDensity, Time.deltaTime * fogLerpSpeed);
    }
}
