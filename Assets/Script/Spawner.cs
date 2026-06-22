using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public float timeBetweenSpawns;
    float nextSpawnTime;

    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;

    public Transform player;


    private void Update()
    {
        if (Time.time > nextSpawnTime)
        {
            nextSpawnTime = Time.time + timeBetweenSpawns;

            List<Transform> validSpawnPoints = new List<Transform>();

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint.position.x >= -20f && spawnPoint.position.x <= 20f
                    && spawnPoint.position.y >= -20f && spawnPoint.position.y <= 20f)
                {
                    validSpawnPoints.Add(spawnPoint); // Hanya spawn point di dalam arena yang valid
                }
            }

            if (validSpawnPoints.Count > 0)
            {
                int rand = Random.Range(0, enemyPrefabs.Length);
                GameObject enemyToSpawn = enemyPrefabs[rand];

                Transform randomSpawnPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];

                if (player)
                {
                    Vector3 spawnerPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
                    transform.position = spawnerPosition;

                    Instantiate(enemyToSpawn, randomSpawnPoint.position, Quaternion.identity);
                }
            }
        }
    }
}
