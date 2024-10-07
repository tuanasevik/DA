using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public static ObjectSpawner Instance { get; private set; }

    public GameObject[] objectPrefabs;
    public int numberOfObjects; // Maximale Anzahl der Objekte
    public float minDistanceBetweenObjects;
    public Vector3 spawnAreaMin;
    public Vector3 spawnAreaMax;
    public float fixedYPosition = 0f;
    public float spawnInterval = 5f; // Zeitintervall zwischen den Spawns

    private List<Vector3> spawnedPositions = new List<Vector3>();
    private int currentObjectCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Wenn der Spawner nicht zerstört werden soll
        }
    }

    void Start()
    {
        // Alle Objekte beim Start spawnen
        for (int i = 0; i < numberOfObjects; i++)
        {
            SpawnObject();
            currentObjectCount++;
        }

        // Coroutine starten, die überwacht, ob neue Objekte gespawnt werden müssen
        StartCoroutine(SpawnObjectsWhenNecessary());
    }

    public void decreaseCurrentObjectCount()
    {
        currentObjectCount--;
    }

    // Diese Coroutine spawnt neue Objekte, wenn currentObjectCount unter der maximalen Anzahl liegt
    IEnumerator SpawnObjectsWhenNecessary()
    {
        while (true)
        {
            // Wenn die aktuelle Anzahl an Objekten kleiner als die maximale ist, spawne ein neues Objekt
            if (currentObjectCount < numberOfObjects)
            {
                SpawnObject();
                currentObjectCount++;
            }

            yield return new WaitForSeconds(spawnInterval); // Wartezeit vor dem nächsten Check
        }
    }

    void SpawnObject()
    {
        Vector3 spawnPosition;
        int attempts = 0;

        do
        {
            spawnPosition = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                fixedYPosition,
                Random.Range(spawnAreaMin.z, spawnAreaMax.z)
            );
            attempts++;
        } while (!IsPositionValid(spawnPosition) && attempts < 100);

        if (attempts < 1000)
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
