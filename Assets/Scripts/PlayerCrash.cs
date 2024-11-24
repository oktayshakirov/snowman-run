using UnityEngine;

public class SnowmanCollision : MonoBehaviour
{
    public GameObject[] snowmanParts; 
    public float explosionForce = 500f; 
    public float explosionRadius = 5f; 
    public Transform explosionCenter; 
    public GameObject snowBurstPrefab; 

    private bool isExploded = false; 

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !isExploded) 
        {
            isExploded = true; 
            Explode();
            SpawnSnowBurst();
        }
    }

    void Explode()
    {
        foreach (GameObject part in snowmanParts)
        {
            Rigidbody rb = part.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(explosionForce, explosionCenter.position, explosionRadius);
            }
        }
    }

    void SpawnSnowBurst()
    {
        if (snowBurstPrefab != null)
        {
            GameObject snowBurst = Instantiate(snowBurstPrefab, explosionCenter.position, Quaternion.identity);
            Destroy(snowBurst, 3f); 
        }
    }
}
