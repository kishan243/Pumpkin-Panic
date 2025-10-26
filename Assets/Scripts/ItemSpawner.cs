using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject[] itemPrefabs; // Assign your item prefabs (like a Cube) here
    public int amountToSpawn = 10;

    [Header("Spawn Area")]
    public float spawnRadius = 15f;
    public LayerMask whatIsGround; // Set this to your ground layer

    void Start()
    {
        SpawnItems();
    }

    void SpawnItems()
    {
        if (itemPrefabs.Length == 0)
        {
            Debug.LogWarning("No item prefabs assigned to spawner!");
            return;
        }

        for (int i = 0; i < amountToSpawn; i++)
        {
            // Pick a random prefab
            GameObject randomPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

            // Pick a random spot in a circle
            Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection.y = 0; // Keep it on the same plane
            Vector3 spawnPos = transform.position + randomDirection;

            // Raycast down to find the ground
            if (Physics.Raycast(spawnPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, whatIsGround))
            {
                // Spawn the item on the ground
                Instantiate(randomPrefab, hit.point, Quaternion.identity);
            }
        }
    }
}
