using UnityEngine;

public class Coin : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Obstacle"))
        {
            DeactivateCoin();
            return;
        }
        if (other.gameObject.CompareTag("Player"))
        {
            if (GameManager.inst != null)
            {
                GameManager.inst.IncrementScore();
            }
            DeactivateCoin();
        }
    }

    private void DeactivateCoin()
    {
        gameObject.SetActive(false);
    }
}