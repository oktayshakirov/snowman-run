using UnityEngine;
using System.Collections.Generic;

public class GroundTile : MonoBehaviour
{
    private GroundSpawner groundSpawner;

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject rampPrefab;

    [SerializeField] private float obstacleSpawnProbability = 0.3f;
    [SerializeField] private float rampSpawnProbability = 0.5f;
    [SerializeField] private float coinHeight = 1f;
    [SerializeField] private float laneOffset = 3f;

    private HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

    public void Initialize(GroundSpawner spawner)
    {
        groundSpawner = spawner;
    }

    private void OnTriggerExit(Collider other)
    {
        if (groundSpawner != null)
        {
            groundSpawner.SpawnTile(true);
        }
        Invoke(nameof(DeactivateTile), 3f);
    }

    private void DeactivateTile()
    {
        gameObject.SetActive(false);
    }

    public void SpawnObstacle()
    {
        if (Random.value > obstacleSpawnProbability)
            return;

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
                (laneIndex - 1) * laneOffset,
                0f,
                transform.position.z
            );

            if (!IsPositionOccupied(new Vector2Int(laneIndex, Mathf.RoundToInt(transform.position.z))))
            {
                Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
                usedPositions.Add(new Vector2Int(laneIndex, Mathf.RoundToInt(transform.position.z)));
            }
        }
    }

    public void SpawnRamps()
    {
        if (Random.value > rampSpawnProbability)
            return;

        int laneIndex = Random.Range(0, 3);
        Vector3 spawnPosition = new Vector3(
            (laneIndex - 1) * laneOffset,
            0f,
            transform.position.z + Random.Range(-2f, 2f)
        );

        if (!IsPositionOccupied(new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z))))
        {
            Instantiate(rampPrefab, spawnPosition, Quaternion.identity, transform);
            usedPositions.Add(new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z)));
        }
    }

    public void SpawnCoins()
    {
        int coinsToSpawn = Random.Range(1, 4);

        for (int i = 0; i < coinsToSpawn; i++)
        {
            int laneIndex = Random.Range(0, 3);
            Vector3 spawnPosition = new Vector3(
                (laneIndex - 1) * laneOffset + Random.Range(-0.3f, 0.3f),
                coinHeight,
                transform.position.z + Random.Range(-1f, 1f)
            );

            if (!IsPositionOccupied(new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z))))
            {
                GameObject coin = Instantiate(coinPrefab, transform);
                coin.transform.position = spawnPosition;
                usedPositions.Add(new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z)));
            }
        }
    }

    private bool IsPositionOccupied(Vector2Int position)
    {
        return usedPositions.Contains(position);
    }
}