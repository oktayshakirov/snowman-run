using UnityEngine;

public class Fog : MonoBehaviour
{
    public static Fog Instance;
    [SerializeField] private float minFogDensity = 0.01f;
    [SerializeField] private float maxFogDensity = 0.175f;
    [SerializeField] private float fogIncrementStep = 0.001f;

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

    public void InitializeFog()
    {
        Debug.Log("Initializing fog with density: " + minFogDensity);
        RenderSettings.fog = true;
        RenderSettings.fogDensity = minFogDensity;
        currentFogDensity = minFogDensity;
    }

    public void IncrementFogDensity()
    {
        if (holdFogIncrement)
        {
            Debug.Log("Fog increment is on hold.");
            return;
        }
        currentFogDensity += fogIncrementStep;
        currentFogDensity = Mathf.Clamp(currentFogDensity, minFogDensity, maxFogDensity);

        Debug.Log($"Incrementing fog density to: {currentFogDensity}");
        RenderSettings.fogDensity = currentFogDensity;
    }

    public void ReduceFogDensity(float reductionPercent)
    {
        currentFogDensity = RenderSettings.fogDensity;
        RenderSettings.fogDensity *= (1 - reductionPercent);
    }

    public void RestoreFogDensityTo(float previousDensity)
    {
        currentFogDensity = previousDensity;
        RenderSettings.fogDensity = currentFogDensity;
    }

    public float GetCurrentFogDensity()
    {
        return currentFogDensity;
    }

    public void HoldFogIncrement(bool hold)
    {
        holdFogIncrement = hold;
        if (hold)
        {
            Debug.Log("Fog incrementing is now on hold.");
        }
        else
        {
            Debug.Log("Fog incrementing resumed.");
        }
    }
}