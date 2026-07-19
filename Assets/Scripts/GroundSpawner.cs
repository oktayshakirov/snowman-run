using UnityEngine;
using System.Collections.Generic;

public class GroundSpawner : MonoBehaviour
{
    [SerializeField] private GameObject groundTile;
    [SerializeField] private int initialTiles = 15;
    [SerializeField] private float coinSpawnProbability = 0.3f;
    [SerializeField] private float rampSpawnProbability = 0.03f;
    [SerializeField] private float obstacleSpawnProbability = 0.3f;
    [SerializeField] private float gogglesSpawnProbability = 0.03f;

    private Vector3 nextSpawnPoint;
    private Vector3 initialSpawnPoint;
    private bool gogglesActive = false;
    private bool isResetting = false;

    private readonly Queue<GroundTile> tilePool = new Queue<GroundTile>();

    // Trigger-exit driven spawning; ignored while a reset is recycling tiles,
    // since deactivating a tile under the player can fire its OnTriggerExit.
    public void RequestNextTile()
    {
        if (isResetting)
            return;

        SpawnTile(true);
    }

    public void SpawnTile(bool spawnItems)
    {
        GroundTile tile;
        if (tilePool.Count > 0)
        {
            tile = tilePool.Dequeue();
            tile.PrepareForReuse();
            tile.transform.position = nextSpawnPoint;
            tile.gameObject.SetActive(true);
        }
        else
        {
            GameObject temp = Instantiate(groundTile, nextSpawnPoint, Quaternion.identity, transform);
            tile = temp.GetComponent<GroundTile>();
            tile.Initialize(this);
        }

        nextSpawnPoint = tile.transform.GetChild(1).position;

        if (spawnItems && Application.isPlaying)
        {
            if (Random.value < obstacleSpawnProbability)
            {
                tile.SpawnObstacle();
            }
            if (Random.value < rampSpawnProbability)
            {
                tile.SpawnRamps();
            }
            if (Random.value < coinSpawnProbability)
            {
                tile.SpawnCoins();
            }
            if (!gogglesActive && Random.value < gogglesSpawnProbability)
            {
                tile.SpawnGoggles();
            }
        }
    }

    public void RecycleTile(GroundTile tile)
    {
        if (!tile.gameObject.activeSelf)
            return;

        tile.gameObject.SetActive(false);
        tile.ClearSpawnedItems();
        tilePool.Enqueue(tile);
    }

    // Recycles every active tile and rebuilds the starting stretch, so a new run
    // can begin without reloading the scene.
    public void ResetRun()
    {
        isResetting = true;

        foreach (Transform child in transform)
        {
            GroundTile tile = child.GetComponent<GroundTile>();
            if (tile != null)
            {
                RecycleTile(tile);
            }
        }

        gogglesActive = false;
        nextSpawnPoint = initialSpawnPoint;
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnTile(i >= 3);
        }

        isResetting = false;
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
            initialSpawnPoint = nextSpawnPoint;
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
