using UnityEngine;
using System.Collections;

public class Boosters : MonoBehaviour
{
    [Header("Booster Settings")]
    [SerializeField] private float gogglesFogReduction = 0.8f;
    [SerializeField] private float gogglesDuration = 7f;
    public float GogglesDuration => gogglesDuration;

    private bool gogglesActive = false;
    private float fogDensityAtActivation;
    private Fog Fog => Fog.Instance;

    public void ActivateGoggles()
    {
        if (gogglesActive || Fog == null)
        {
            return;
        }
        gogglesActive = true;
        Fog.HoldFogIncrement(true);
        fogDensityAtActivation = Fog.GetCurrentFogDensity();
        Fog.ReduceFogDensity(gogglesFogReduction);
        StartCoroutine(GogglesCooldown());
    }

    private IEnumerator GogglesCooldown()
    {
        yield return new WaitForSeconds(gogglesDuration);
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
