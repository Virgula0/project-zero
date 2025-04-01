using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn; // Assign your prefab in the Inspector
    [SerializeField] private Camera mainCamera; // Assign your main camera in the Inspector

    void Start()
    {
        if (prefabToSpawn != null && mainCamera != null)
        {
            SpawnPrefabInRandomPosition();
        }
        else
        {
            Debug.LogError("Prefab or Main Camera is not assigned in RandomSpawner.");
        }
    }

    void SpawnPrefabInRandomPosition()
    {
        // Get the camera's visible bounds
        Vector2 randomPosition = GetRandomPositionInView();

        // Instantiate the prefab at the random position
        Instantiate(prefabToSpawn, randomPosition, Quaternion.identity);
    }

    Vector2 GetRandomPositionInView()
    {
        // Get camera boundaries
        float minX = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        float maxX = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
        float minY = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float maxY = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

        // Generate a random position within the screen bounds
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        return new Vector2(randomX, randomY);
    }
}
