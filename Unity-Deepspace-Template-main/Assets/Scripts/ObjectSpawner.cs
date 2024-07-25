using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] objectPrefabs; // Array für die drei Prefabs
    public int numberOfObjects;
    public float minDistanceBetweenObjects;
    public Vector3 spawnAreaMin; // Für X- und Z-Koordinaten
    public Vector3 spawnAreaMax; // Für X- und Z-Koordinaten
    public float fixedYPosition = 0f; // Feste Y-Position für alle Objekte

    private List<Vector3> spawnedPositions = new List<Vector3>();

    void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 spawnPosition;
            int attempts = 0;

            do
            {
                spawnPosition = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x), // Variierende X-Position
                    fixedYPosition, // Feste Y-Position
                    Random.Range(spawnAreaMin.z, spawnAreaMax.z)  // Variierende Z-Position
                );
                attempts++;
            } while (!IsPositionValid(spawnPosition) && attempts < 100);

            if (attempts < 100)
            {
                int prefabIndex = Random.Range(0, objectPrefabs.Length);
                Instantiate(objectPrefabs[prefabIndex], spawnPosition, Quaternion.identity);
                spawnedPositions.Add(spawnPosition);
            }
            else
            {
                Debug.LogWarning("Failed to place object due to lack of space.");
            }
        }
    }

    bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 pos in spawnedPositions)
        {
            if (Vector3.Distance(pos, position) < minDistanceBetweenObjects)
            {
                return false;
            }
        }
        return true;
    }
}
