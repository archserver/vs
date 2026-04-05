using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

// Randomly spawns XP gems and health hearts on the map each frame based on chance
public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner OSInstance;
    [SerializeField] private Tilemap tilemap;               // tilemap to scan for valid spawn positions
    [SerializeField] private List<TileBase> spawnTiles;     // only spawn on these tiles
    private List<Vector3> validSpawnPositions = new List<Vector3>(); // list of valid world positions

    [SerializeField] private GameObject gemPrefab;          // XP gem prefab to spawn
    [SerializeField] private GameObject heartPrefab;        // health heart prefab to spawn
    [SerializeField] private float gemSpawnChance = 0.01f;  // 1% chance per tick to spawn a gem
    [SerializeField] private float heartSpawnChance = 0.001f; // 0.1% chance per tick to spawn a heart
    [SerializeField] private int maxGems = 20;              // cap on gems in the scene at once
    [SerializeField] private int maxHearts = 5;             // cap on hearts in the scene at once

    private int currentGems = 0;        // tracks how many gems are currently in the scene
    private int currentHearts = 0;      // tracks how many hearts are currently in the scene

    private void Awake()
    {
        OSInstance = this;
    }

    // scan the tilemap at startup and collect valid spawn positions
    void Start()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (spawnTiles.Contains(tilemap.GetTile(pos)))
                validSpawnPositions.Add(tilemap.CellToWorld(pos) + tilemap.cellSize / 2);
        }
    }

    void Update()
    {
        // only the server spawns objects
        if (!NetworkManager.Singleton.IsServer && !NetworkGameManager.IsSolo) return;

        // stop spawning if the player is dead, not yet spawned, or the game is paused
        if (PlayerController.PlayerInstance == null || !PlayerController.PlayerInstance.isActiveAndEnabled) return;
        if (Time.timeScale == 0f) return;

        // check each frame for a gem spawn — 1% chance, capped at maxGems
        if (currentGems < maxGems && Random.value < gemSpawnChance)
            SpawnObject(gemPrefab, ref currentGems);

        // roll each frame for a heart spawn — 0.1% chance, capped at maxHearts
        if (currentHearts < maxHearts && Random.value < heartSpawnChance)
            SpawnObject(heartPrefab, ref currentHearts);
    }

    // pick a random valid position and spawn the object 
    private void SpawnObject(GameObject prefab, ref int counter)
    {
        if (validSpawnPositions.Count == 0) return;

        // try up to 3 times to find a position not visible to the camera
        int maxAttempts = 3;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 spawnPos = validSpawnPositions[Random.Range(0, validSpawnPositions.Count)];
            if (!IsVisibleToCamera(spawnPos) || i == maxAttempts - 1) // if on the last attempt it will force spawn
            {
                if (NetworkGameManager.IsSolo)
                {
                    Instantiate(prefab, spawnPos, transform.rotation, transform);
                }
                else
                {
                    var obj = Instantiate(prefab, spawnPos, transform.rotation);
                    obj.GetComponent<NetworkObject>().Spawn();
                }
                counter++;
                return;
            }
        }
    }

    // called by XPGem and HeartHealth when they are picked up to keep the counter accurate
    public void OnObjectPickedUp(bool isGem)
    {
        if (isGem) currentGems--;
        else currentHearts--;
    }

    // returns true if the world position is within the camera's visible area
    private bool IsVisibleToCamera(Vector3 worldPos)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
        return viewportPos.x > -0.05f && viewportPos.x < 1.05f &&
               viewportPos.y > -0.05f && viewportPos.y < 1.05f;
    }
}
