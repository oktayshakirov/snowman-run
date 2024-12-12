using UnityEngine;

[ExecuteInEditMode]
public class GroundSpawner : MonoBehaviour
{
    [SerializeField] private GameObject groundTile;
    [SerializeField] private int initialTiles = 15;
    [SerializeField] private float coinSpawnProbability = 0.5f;
    [SerializeField] private float rampSpawnProbability = 0.1f;

    private Vector3 nextSpawnPoint;

    public void SpawnTile(bool spawnItems)
    {
        GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity, transform);
        temp.GetComponent<GroundTile>().Initialize(this); // Assign GroundSpawner to GroundTile
        nextSpawnPoint = temp.transform.GetChild(1).transform.position;

        if (spawnItems && Application.isPlaying)
        {
            var groundTileScript = temp.GetComponent<GroundTile>();
            groundTileScript.SpawnObstacle();
            if (Random.value < coinSpawnProbability)
            {
                groundTileScript.SpawnCoins();
            }
            if (Random.value < rampSpawnProbability)
            {
                groundTileScript.SpawnRamps();
            }
        }
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            ClearTiles();
            for (int i = 0; i < initialTiles; i++)
            {
                SpawnTile(i >= 3);
            }
        }
    }

    private void ClearTiles()
    {
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}