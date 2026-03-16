using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable] 
    public class Wave
    {
        // Enemy to spawn
        public GameObject enemyPrefab;
        // Spawn interval for enemy
        public float spawnInterval;
        // Number of enemies per wave
        public int enemiesPerWave;
    }

    // List of enemies to spawn
    public List<Wave> waves;
    //  wave number we are on
    public int waveNumber;
    // Count up timer
    public float spawnTimer;
    // Count of spawned enemies for wave
    private int enemiesSpawned;
       
    // Update is called once per frame
    void Update()
    {
        // Increment the spawn Timer
        spawnTimer += Time.deltaTime;
        // if the spawn timer is greater then the spawn interval for the wave then reset the spawn timer and spawn an enemy 
        if (spawnTimer > waves[waveNumber].spawnInterval)
        {
            spawnTimer = 0;
            SpawnEnemy();
        }
        // if the number of enemies spawned is greater then the wave number of enemies then reset enemies spawned and increas the wave
        if (enemiesSpawned >= waves[waveNumber].enemiesPerWave)
        {
            enemiesSpawned = 0;
            waveNumber++;
        }
        // if the wave is more then the max number of waves reset the wave number to 0 
        if (waveNumber >= waves.Count)
            waveNumber = 0;
    }

    //spawn a creature and increase spawn count
    private void SpawnEnemy()
    {
        Instantiate(waves[waveNumber].enemyPrefab, transform.position, transform.rotation);
        enemiesSpawned++;
    }
}
