using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 90f;

    private void Update()
    {
        transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Obstacle>() != null)
        {
            Destroy(gameObject);
            return;
        }
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.inst.IncrementScore();
            Destroy(gameObject);
        }
    }
}
