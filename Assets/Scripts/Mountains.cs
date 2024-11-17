using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MountainRepeater : MonoBehaviour
{
    [SerializeField] GameObject mountainPrefab;
    [SerializeField] int numberOfMountains = 20; 
    [SerializeField] float mountainLength = 20f;
    [SerializeField] float fadeInDuration = 2f;

    private List<GameObject> activeMountains = new List<GameObject>();
    private Vector3 nextLeftMountainPosition;
    private Vector3 nextRightMountainPosition;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = Camera.main.transform;
        InitializeMountains();
    }

    private void Update()
    {
        if (playerTransform.position.z > activeMountains[activeMountains.Count - 1].transform.position.z - mountainLength * 10)
        {
            SpawnMountainSegment();
        }

        if (activeMountains[0].transform.position.z < playerTransform.position.z - mountainLength * 12)
        {
            Destroy(activeMountains[0]);
            activeMountains.RemoveAt(0);
        }
    }

    private void InitializeMountains()
    {
        nextLeftMountainPosition = transform.position + Vector3.left * mountainPrefab.transform.localScale.x;
        nextRightMountainPosition = transform.position + Vector3.right * mountainPrefab.transform.localScale.x;

        for (int i = 0; i < numberOfMountains; i++)
        {
            SpawnMountainSegment();
        }
    }

    private void SpawnMountainSegment()
    {
        GameObject leftMountain = Instantiate(mountainPrefab, nextLeftMountainPosition, Quaternion.identity, transform);
        StartCoroutine(FadeIn(leftMountain));
        activeMountains.Add(leftMountain);
        nextLeftMountainPosition += Vector3.forward * mountainLength;

        GameObject rightMountain = Instantiate(mountainPrefab, nextRightMountainPosition, Quaternion.identity, transform);
        StartCoroutine(FadeIn(rightMountain));
        activeMountains.Add(rightMountain);
        nextRightMountainPosition += Vector3.forward * mountainLength;
    }

    private IEnumerator FadeIn(GameObject mountain)
    {
        Material mountainMaterial = mountain.GetComponent<Renderer>().material;
        Color initialColor = mountainMaterial.color;
        initialColor.a = 0;
        mountainMaterial.color = initialColor;

        float elapsedTime = 0;
        while (elapsedTime < fadeInDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
            mountainMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mountainMaterial.color = new Color(initialColor.r, initialColor.g, initialColor.b, 1);
    }
}
