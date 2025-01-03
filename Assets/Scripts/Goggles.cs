using UnityEngine;

public class Goggles : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            DeactivateGoggles();
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (GameManager.inst != null)
            {
                GameManager.inst.ActivateGoggles();
            }

            GroundSpawner spawner = Object.FindFirstObjectByType<GroundSpawner>();
            if (spawner != null)
            {
                spawner.SetGogglesActive(true);
                spawner.DestroyAllGoggles();
            }

            DeactivateGoggles();
        }
    }

    private void DeactivateGoggles()
    {
        GroundSpawner spawner = Object.FindFirstObjectByType<GroundSpawner>();
        if (spawner != null)
        {
            spawner.SetGogglesActive(false);
        }

        gameObject.SetActive(false);
    }
}
