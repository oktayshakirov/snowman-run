using System.Collections;
using UnityEngine;

public class PlayerCrash : MonoBehaviour
{
    public GameObject[] snowmanParts;
    public float explosionForce = 500f;
    public float explosionRadius = 5f;
    public Transform explosionCenter;
    public GameObject snowBurstPrefab;

    private bool isExploded = false;

    // Parts do not sit at local zero; their authored offsets must be cached so
    // ResetSnowman can restore the exact original pose after an explosion.
    private Vector3[] initialLocalPositions;
    private Quaternion[] initialLocalRotations;

    void Awake()
    {
        initialLocalPositions = new Vector3[snowmanParts.Length];
        initialLocalRotations = new Quaternion[snowmanParts.Length];
        for (int i = 0; i < snowmanParts.Length; i++)
        {
            initialLocalPositions[i] = snowmanParts[i].transform.localPosition;
            initialLocalRotations[i] = snowmanParts[i].transform.localRotation;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !isExploded)
        {
            isExploded = true;
            Explode();
            SpawnSnowBurst();
            StartCoroutine(HandlePlayerCrash());
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

    private IEnumerator HandlePlayerCrash()
    {
        if (GetComponent<Player>() != null)
        {
            GetComponent<Player>().enabled = false;
        }
        yield return new WaitForSeconds(2f);
        if (GameManager.inst != null)
        {
            GameManager.inst.OnPlayerCrash();
        }
    }
    public void ResetSnowman()
    {
        isExploded = false;
        for (int i = 0; i < snowmanParts.Length; i++)
        {
            GameObject part = snowmanParts[i];
            Rigidbody rb = part.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
            }
            part.transform.localPosition = initialLocalPositions[i];
            part.transform.localRotation = initialLocalRotations[i];
        }
    }

}
