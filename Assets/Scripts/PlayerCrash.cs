using System.Collections;
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
        if (GetComponent<PlayerMovement>() != null)
        {
            GetComponent<PlayerMovement>().enabled = false;
        }
        yield return new WaitForSeconds(2f);
        if (GameManager.inst != null)
        {
            GameManager.inst.OnPlayerCrash();
        }
        if (AdManager.Instance != null && AdManager.Instance.IsAdReady())
        {
            AdManager.Instance.ShowAd(() =>
            {
                StartScreenManager.Instance.ShowStartScreen();
            });
        }
    }
    public void ResetSnowman()
    {
        isExploded = false;
        foreach (GameObject part in snowmanParts)
        {
            Rigidbody rb = part.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;
            }
            part.transform.localPosition = Vector3.zero;
            part.transform.localRotation = Quaternion.identity;
        }
    }

}
