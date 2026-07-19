using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponentInParent<Player>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}
