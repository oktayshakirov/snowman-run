using UnityEngine;
using System.Collections.Generic;

public class GroundTile : MonoBehaviour
{
    GroundSpawner groundSpawner;
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject rampPrefab;

    private void Start()
    {
        groundSpawner = GameObject.FindFirstObjectByType<GroundSpawner>();
    }

    private void OnTriggerExit(Collider other)
    {
        groundSpawner.SpawnTile(true);
        Destroy(gameObject, 2);
    }

    public void SpawnObstacle()
    {
        float rowSpawnProbability = 0.3f;
        if (Random.value > rowSpawnProbability)
        {
            return;
        }

        int lanesWithObstacles = Random.Range(1, 2);
        HashSet<int> usedLanes = new HashSet<int>();

        for (int i = 0; i < lanesWithObstacles; i++)
        {
            int laneIndex;
            do
            {
                laneIndex = Random.Range(0, 3);
            } while (usedLanes.Contains(laneIndex));

            usedLanes.Add(laneIndex);

            Vector3 spawnPosition = new Vector3(
                (laneIndex - 1) * 3f,
                0f,
                transform.position.z
            );

            Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
        }
    }

public void SpawnRamps()
{
    float rampSpawnProbability = 0.5f; 
    if (Random.value > rampSpawnProbability)
    {
        return;
    }

    int laneIndex = Random.Range(0, 3);
    Vector3 spawnPosition = new Vector3(
        (laneIndex - 1) * 3f,
        0f,
        transform.position.z + Random.Range(-2f, 2f) 
    );

    Instantiate(rampPrefab, spawnPosition, Quaternion.identity, transform); // Removed rotation
}


    public void SpawnCoins()
    {
        int coinsToSpawn = Random.Range(1, 4);
        float laneOffset = 3f;

        for (int i = 0; i < coinsToSpawn; i++)
        {
            int lane = Random.Range(0, 3);
            float lanePositionX = (lane - 1) * laneOffset;
            Vector3 spawnPosition = new Vector3(
                lanePositionX + Random.Range(-0.3f, 0.3f),
                1f,
                transform.position.z + Random.Range(-1f, 1f)
            );

            GameObject temp = Instantiate(coinPrefab, transform);
            temp.transform.position = spawnPosition;
        }
    }

    Vector3 GetRandomPointInCollider(Collider collider)
    {
        Vector3 point = new Vector3(
            Random.Range(collider.bounds.min.x, collider.bounds.max.x),
            Random.Range(collider.bounds.min.y, collider.bounds.max.y),
            Random.Range(collider.bounds.min.z, collider.bounds.max.z)
        );
        if (point != collider.ClosestPoint(point))
        {
            point = GetRandomPointInCollider(collider);
        }

        point.y = 1;
        return point;
    }
}
