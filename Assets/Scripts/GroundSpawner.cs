using UnityEngine;

[ExecuteInEditMode]
public class GroundSpawner : MonoBehaviour
{
    [SerializeField] GameObject groundTile;
    Vector3 nextSpawnPoint;

    public void SpawnTile(bool spawnItems)
    {
        GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity, transform);
        nextSpawnPoint = temp.transform.GetChild(1).transform.position;
        if (spawnItems && Application.isPlaying)
        {
            var groundTileScript = temp.GetComponent<GroundTile>();
            groundTileScript.SpawnObstacle();
            if (Random.Range(0f, 1f) < 0.5f)
            {
                groundTileScript.SpawnCoins();
            }
            if (Random.Range(0f, 1f) < 0.1f) 
            {
                groundTileScript.SpawnRamps();
            }
        }
    }

    private void Start()
    {
        ClearTiles();
        for (int i = 0; i < 15; i++)
        {
            SpawnTile(Application.isPlaying && i >= 3);
        }
    }

    private void ClearTiles()
    {
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }
    }
}
