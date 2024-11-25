using UnityEngine;
using System.Collections.Generic;

public class GroundTile : MonoBehaviour
{
    GroundSpawner groundSpawner;
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject obstaclePrefab;
    [SerializeField] GameObject rampPrefab;

    private HashSet<Vector3> usedPositions = new HashSet<Vector3>();

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

            if (!IsPositionOccupied(spawnPosition, obstaclePrefab))
            {
                Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
                usedPositions.Add(spawnPosition);
            }
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

        if (!IsPositionOccupied(spawnPosition, rampPrefab))
        {
            Instantiate(rampPrefab, spawnPosition, Quaternion.identity, transform);
            usedPositions.Add(spawnPosition);
        }
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

            if (!IsPositionOccupied(spawnPosition, coinPrefab))
            {
                GameObject temp = Instantiate(coinPrefab, transform);
                temp.transform.position = spawnPosition;
                usedPositions.Add(spawnPosition);
            }
        }
    }

    private bool IsPositionOccupied(Vector3 position, GameObject prefab)
    {
        float minimumDistance = GetMinimumDistanceForType(prefab);

        foreach (var usedPosition in usedPositions)
        {
            if (Vector3.Distance(usedPosition, position) < minimumDistance)
            {
                return true;
            }
        }
        return false;
    }

    private float GetMinimumDistanceForType(GameObject prefab)
    {
        if (prefab == coinPrefab) return 1f;        
        if (prefab == rampPrefab) return 3f;       
        if (prefab == obstaclePrefab) return 2f;   
        return 2f;                               
    }
}