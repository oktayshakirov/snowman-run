using UnityEngine;

public class GroundTile : MonoBehaviour {

    GroundSpawner groundSpawner;
    [SerializeField] GameObject coinPrefab;
    [SerializeField] GameObject obstaclePrefab;

    private void Start () {
        groundSpawner = GameObject.FindFirstObjectByType<GroundSpawner>();
    }

    private void OnTriggerExit (Collider other)
    {
        groundSpawner.SpawnTile(true);
        Destroy(gameObject, 2);
    }

    public void SpawnObstacle()
    {
        int obstacleSpawnIndex = Random.Range(1, 5);
        Transform spawnPoint = transform.GetChild(obstacleSpawnIndex).transform;
        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.y = 0f; 

        Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity, transform);
    }

    public void SpawnCoins()
    {
        int coinsToSpawn = Random.Range(1, 4);
        float laneOffset = 2.5f;
        
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
        if (point != collider.ClosestPoint(point)) {
            point = GetRandomPointInCollider(collider);
        }

        point.y = 1;
        return point;
    }
}