using UnityEngine;
using System.Collections.Generic;

public class MountainRepeater : MonoBehaviour
{
    [SerializeField] private GameObject mountainPrefab;
    [SerializeField] private int numberOfMountains = 20;
    [SerializeField] private float mountainLength = 20f;

    private Queue<GameObject> mountainPool = new Queue<GameObject>();
    private List<GameObject> activeMountains = new List<GameObject>();
    private Vector3 nextLeftMountainPosition;
    private Vector3 nextRightMountainPosition;
    private Transform playerTransform;

    private float checkInterval = 0.1f;
    private float nextCheckTime = 0;

    private void Start()
    {
        playerTransform = Camera.main.transform;
        InitializeMountains();
    }

    private void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckAndUpdateMountains();
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

    private void CheckAndUpdateMountains()
    {
        if (playerTransform.position.z > activeMountains[activeMountains.Count - 1].transform.position.z - mountainLength * 10)
        {
            SpawnMountainSegment();
        }

        if (activeMountains[0].transform.position.z < playerTransform.position.z - mountainLength * 12)
        {
            RecycleMountain(activeMountains[0]);
            activeMountains.RemoveAt(0);
        }
    }

    private void SpawnMountainSegment()
    {
        GameObject leftMountain = GetMountainFromPool(nextLeftMountainPosition);
        activeMountains.Add(leftMountain);
        nextLeftMountainPosition += Vector3.forward * mountainLength;

        GameObject rightMountain = GetMountainFromPool(nextRightMountainPosition);
        activeMountains.Add(rightMountain);
        nextRightMountainPosition += Vector3.forward * mountainLength;
    }

    private GameObject GetMountainFromPool(Vector3 position)
    {
        GameObject mountain;

        if (mountainPool.Count > 0)
        {
            mountain = mountainPool.Dequeue();
            mountain.transform.position = position;
            mountain.SetActive(true);
        }
        else
        {
            mountain = Instantiate(mountainPrefab, position, Quaternion.identity, transform);
        }
        return mountain;
    }

    private void RecycleMountain(GameObject mountain)
    {
        mountain.SetActive(false);
        mountainPool.Enqueue(mountain);
    }
}