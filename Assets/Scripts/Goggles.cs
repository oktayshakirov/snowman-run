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
                spawner.HideAllGoggles();
            }

            DeactivateGoggles();
        }
    }

    private void DeactivateGoggles()
    {
        gameObject.SetActive(false);
    }
}
