using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    [SerializeField] private GameObject groundTile;
    [SerializeField] private int initialTiles = 15;
    [SerializeField] private float coinSpawnProbability = 0.3f;
    [SerializeField] private float rampSpawnProbability = 0.03f;
    [SerializeField] private float obstacleSpawnProbability = 0.3f;
    [SerializeField] private float gogglesSpawnProbability = 0.03f;

    private Vector3 nextSpawnPoint;
    private bool gogglesActive = false;

    public void SpawnTile(bool spawnItems)
    {
        GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity, transform);
        temp.GetComponent<GroundTile>().Initialize(this);
        nextSpawnPoint = temp.transform.GetChild(1).transform.position;

        if (spawnItems && Application.isPlaying)
        {
            var groundTileScript = temp.GetComponent<GroundTile>();

            if (Random.value < obstacleSpawnProbability)
            {
                groundTileScript.SpawnObstacle();
            }
            if (Random.value < rampSpawnProbability)
            {
                groundTileScript.SpawnRamps();
            }
            if (Random.value < coinSpawnProbability)
            {
                groundTileScript.SpawnCoins();
            }
            if (!gogglesActive && Random.value < gogglesSpawnProbability)
            {
                groundTileScript.SpawnGoggles();
            }
        }
    }

    public void SetGogglesActive(bool isActive)
    {
        gogglesActive = isActive;
    }

    public void HideAllGoggles()
    {
        foreach (Transform tile in transform)
        {
            foreach (Transform child in tile)
            {
                if (child.CompareTag("Goggles"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowAllGoggles()
    {
        foreach (Transform tile in transform)
        {
            foreach (Transform child in tile)
            {
                if (child.CompareTag("Goggles"))
                {
                    child.gameObject.SetActive(true);
                }
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
