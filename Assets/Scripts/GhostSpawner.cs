using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int maxGhosts = 15;
    [SerializeField] private float spawnRadius = 50f; // How far out they can spawn
    [SerializeField] private float minSpawnRadiusFromPlayer = 20f; // How close they can spawn

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = PlayerHealth.Instance.transform;
        }

        StartCoroutine(InitialSpawn());
    }

    private IEnumerator InitialSpawn()
    {
        int spawnedCount = 0;
        while (spawnedCount < maxGhosts)
        {
            // Get a random direction
            Vector2 randomDir = Random.insideUnitCircle.normalized;

            // Get a random distance within the allowed range
            float randomDistance = Random.Range(minSpawnRadiusFromPlayer, spawnRadius);

            // Calculate spawn position
            Vector3 spawnPos = playerTransform.position + new Vector3(randomDir.x, 0, randomDir.y) * randomDistance;

            // TODO: Ideally, you'd check if this position is on the ground (NavMesh)
            // For now, we'll just spawn it at Y=1
            spawnPos.y = 1f;

            Instantiate(ghostPrefab, spawnPos, Quaternion.identity);
            spawnedCount++;

            // Wait a tiny bit between spawns to avoid lag spikes
            yield return new WaitForSeconds(0.1f);
        }
    }

    // TODO: You could add a function here to periodically check
    // if the ghost count is low and spawn more.
}
