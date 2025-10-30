using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private int maxGhosts = 15;
    [SerializeField] private float spawnRadius = 50f;
    [SerializeField] private float minSpawnRadiusFromPlayer = 20f;

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
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnRadiusFromPlayer, spawnRadius);
            Vector3 spawnPos = playerTransform.position + new Vector3(randomDir.x, 0, randomDir.y) * randomDistance;
            spawnPos.y = 1f;
            Instantiate(ghostPrefab, spawnPos, Quaternion.identity);
            spawnedCount++;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
