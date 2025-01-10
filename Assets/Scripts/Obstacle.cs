using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Player playerMovement;

    [System.Obsolete]
    private void Start()
    {
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<Player>();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerMovement.Die();
        }
    }
}