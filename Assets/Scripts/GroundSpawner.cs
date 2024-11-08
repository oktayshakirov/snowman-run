using UnityEngine;

[ExecuteInEditMode]
public class GroundSpawner : MonoBehaviour {

    [SerializeField] GameObject groundTile;
    Vector3 nextSpawnPoint;

    public void SpawnTile(bool spawnItems)
    {
        GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity, transform);
        nextSpawnPoint = temp.transform.GetChild(1).transform.position;
        if (spawnItems && Application.isPlaying)
        {
            temp.GetComponent<GroundTile>().SpawnObstacle();
            if (Random.Range(0f, 1f) < 0.5f)
            {
                temp.GetComponent<GroundTile>().SpawnCoins();
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