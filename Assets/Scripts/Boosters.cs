using UnityEngine;
using System.Collections;

public class Boosters : MonoBehaviour
{
    [Header("Booster Settings")]
    [SerializeField] private Fog fog;
    [SerializeField] private float gogglesFogReduction = 0.3f;
    [SerializeField] private float gogglesDuration = 10f;

    private bool gogglesActive = false;
    private float fogDensityAtActivation;

    public void ActivateGoggles()
    {
        if (gogglesActive || fog == null) return;

        Debug.Log("Goggles activated: Reducing fog density.");
        gogglesActive = true;
        fog.HoldFogIncrement(true);
        fogDensityAtActivation = fog.GetCurrentFogDensity();
        fog.ReduceFogDensity(gogglesFogReduction);
        StartCoroutine(GogglesCooldown());
    }

    private IEnumerator GogglesCooldown()
    {
        yield return new WaitForSeconds(gogglesDuration);

        Debug.Log("Goggles effect ended: Restoring fog density to continue incrementing.");
        fog.RestoreFogDensityTo(fogDensityAtActivation);
        GroundSpawner spawner = Object.FindFirstObjectByType<GroundSpawner>();
        if (spawner != null)
        {
            spawner.SetGogglesActive(false);
        }
        fog.HoldFogIncrement(false);
        gogglesActive = false;
    }
}