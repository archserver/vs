using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

// Spawns enemies in waves at valid tilemap positions outside the camera view
public class EnemySpawner : MonoBehaviour
{
    // defines a single wave of enemies to spawn
    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;      // which enemy to spawn
        public float spawnInterval;         // seconds between each spawn in this wave
        public int enemiesPerWave;          // how many enemies before moving to the next wave
    }

    [SerializeField] private Tilemap tilemap;               // the tilemap to scan for spawn locations
    [SerializeField] private List<TileBase> spawnTiles;     // only spawn on these tiles
    private List<Vector3> _validSpawnPositions = new List<Vector3>(); // list of valid world positions to spawn at

    public List<Wave> waves;            // a list of waves configured in the Inspector
    public int waveNumber;              // which wave is currently active
    public float spawnTimer;            // counts up to the next spawn
    private int enemiesSpawned;         // how many enemies have spawned in the current wave

    // scan the tilemap at startup and collect valid spawn positions
    void Start()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (spawnTiles.Contains(tilemap.GetTile(pos)))
                _validSpawnPositions.Add(tilemap.CellToWorld(pos) + tilemap.cellSize / 2);
        }
    }

    void Update()
    {
        // only the server spawns enemies
        if (!NetworkManager.Singleton.IsServer && !NetworkGameManager.IsSolo) return;

        // stop spawning if the player is dead or not yet spawned
        if (PlayerController.PlayerInstance == null || !PlayerController.PlayerInstance.isActiveAndEnabled) return;

        // count up and spawn when the interval is reached
        spawnTimer += Time.deltaTime;
        if (spawnTimer > waves[waveNumber].spawnInterval)
        {
            spawnTimer = 0;
            SpawnEnemy();
        }

        // move to the next wave once enough enemies have spawned
        if (enemiesSpawned >= waves[waveNumber].enemiesPerWave)
        {
            enemiesSpawned = 0;

            // speed up spawning slightly each wave, down to a minimum of 0.3s
            if (waves[waveNumber].spawnInterval >= 0.3f)
                waves[waveNumber].spawnInterval *= 0.95f;

            waveNumber++;
        }

        // loop back to the first wave after the last wave is complete
        if (waveNumber >= waves.Count)
            waveNumber = 0;
    }

    // pick a random valid position outside the camera view and spawn an enemy
    private void SpawnEnemy()
    {
        if (_validSpawnPositions.Count == 0) return;

        // try up to 3 times to find a position not visible to the camera
        int maxAttempts = 3;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 spawnPos = _validSpawnPositions[Random.Range(0, _validSpawnPositions.Count)];
            if (!IsVisibleToCamera(spawnPos) || i == maxAttempts-1) // if last loop time spawn enemy 
            {
                if (NetworkGameManager.IsSolo)
                {
                    Instantiate(waves[waveNumber].enemyPrefab, spawnPos, transform.rotation, transform);
                }
                else
                {
                    var enemy = Instantiate(waves[waveNumber].enemyPrefab, spawnPos, transform.rotation);
                    enemy.GetComponent<NetworkObject>().Spawn();
                }
                enemiesSpawned++;
                return;
            }
        }
    }

    // returns true if the world position is within the camera's visible area
    private bool IsVisibleToCamera(Vector3 worldPos)
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
        return viewportPos.x > -0.05f && viewportPos.x < 1.05f &&
               viewportPos.y > -0.05f && viewportPos.y < 1.05f;
    }
}
