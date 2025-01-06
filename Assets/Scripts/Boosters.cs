using UnityEngine;
using System.Collections;

public class Boosters : MonoBehaviour
{
    [Header("Booster Settings")]
    [SerializeField] private Fog fog;
    [SerializeField] private float gogglesFogReduction = 0.8f;
    [SerializeField] private float gogglesDuration = 7f;

    private bool gogglesActive = false;
    private float fogDensityAtActivation;

    public void ActivateGoggles()
    {
        if (gogglesActive || fog == null) return;
        gogglesActive = true;
        fog.HoldFogIncrement(true);
        fogDensityAtActivation = fog.GetCurrentFogDensity();
        fog.ReduceFogDensity(gogglesFogReduction);
        StartCoroutine(GogglesCooldown());
    }

    private IEnumerator GogglesCooldown()
    {
        yield return new WaitForSeconds(gogglesDuration);
        fog.RestoreFogDensityTo(fogDensityAtActivation);
        fog.HoldFogIncrement(false);
        gogglesActive = false;
        GroundSpawner spawner = Object.FindFirstObjectByType<GroundSpawner>();
        if (spawner != null)
        {
            spawner.ShowAllGoggles();
        }
    }
}
