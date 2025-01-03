using UnityEngine;
using System.Collections.Generic;

public class GroundTile : MonoBehaviour
{
    private GroundSpawner groundSpawner;

    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject rampPrefab;
    [SerializeField] private GameObject gogglesPrefab;

    [SerializeField] private float coinHeight = 1f;
    [SerializeField] private float gogglesHeight = 2f;
    [SerializeField] private float laneOffset = 3f;

    private HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

    private const float RampZSize = 10f;
    private const float ObstacleZSize = 3f;
    private const float CoinZSize = 1f;
    private const float GogglesZSize = 1f;

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
                transform.position.z + Random.Range(5f, 7.5f)
            );

            Vector2Int gridPosition = new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z));

            if (!IsPositionOccupied(gridPosition, ObstacleZSize))
            {
                Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
                MarkPositionOccupied(gridPosition, ObstacleZSize);
            }
        }
    }

    public void SpawnRamps()
    {
        int laneIndex = Random.Range(0, 3);
        Vector3 spawnPosition = new Vector3(
            (laneIndex - 1) * laneOffset,
            0f,
            transform.position.z + Random.Range(10f, 12f)
        );

        Vector2Int gridPosition = new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z));

        if (!IsPositionOccupied(gridPosition, RampZSize))
        {
            Instantiate(rampPrefab, spawnPosition, Quaternion.identity, transform);
            MarkPositionOccupied(gridPosition, RampZSize);
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
                transform.position.z + Random.Range(1f, 1.5f)
            );

            Vector2Int gridPosition = new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z));

            if (!IsPositionOccupied(gridPosition, CoinZSize))
            {
                GameObject coin = Instantiate(coinPrefab, transform);
                coin.transform.position = spawnPosition;
                MarkPositionOccupied(gridPosition, CoinZSize);
            }
        }
    }

    public void SpawnGoggles()
    {
        int laneIndex = Random.Range(0, 3);
        Vector3 spawnPosition = new Vector3(
            (laneIndex - 1) * laneOffset,
            gogglesHeight,
            transform.position.z + Random.Range(2f, 5f)
        );

        Vector2Int gridPosition = new Vector2Int(laneIndex, Mathf.RoundToInt(spawnPosition.z));

        if (!IsPositionOccupied(gridPosition, GogglesZSize))
        {
            GameObject goggles = Instantiate(gogglesPrefab, transform);
            goggles.transform.position = spawnPosition;
            MarkPositionOccupied(gridPosition, GogglesZSize);
        }
    }

    private bool IsPositionOccupied(Vector2Int position, float zSize)
    {
        for (int zOffset = 0; zOffset < Mathf.CeilToInt(zSize); zOffset++)
        {
            Vector2Int checkPosition = new Vector2Int(position.x, position.y + zOffset);
            if (usedPositions.Contains(checkPosition))
            {
                return true;
            }
        }
        return false;
    }

    private void MarkPositionOccupied(Vector2Int position, float zSize)
    {
        for (int zOffset = 0; zOffset < Mathf.CeilToInt(zSize); zOffset++)
        {
            Vector2Int occupyPosition = new Vector2Int(position.x, position.y + zOffset);
            usedPositions.Add(occupyPosition);
        }
    }
}
