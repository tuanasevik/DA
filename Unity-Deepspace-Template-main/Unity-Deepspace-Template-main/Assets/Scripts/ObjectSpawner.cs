using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject woodPrefab; // Das Prefab, das gespawnt werden soll
    public int numberOfPieces = 10; // Anzahl der Holzstücke
    public Vector2 spawnAreaMin = new Vector2(-10, -10); // Minimum-Koordinaten des Spawn-Bereichs
    public Vector2 spawnAreaMax = new Vector2(10, 10); // Maximum-Koordinaten des Spawn-Bereichs


    void Start()
    {
        SpawnObjectPieces();
    }
    void SpawnObjectPieces() {
    List<Vector3> spawnedPositions = new List<Vector3>();
    
    for (int i = 0; i < numberOfPieces; i++)
    {
        Vector3 spawnPosition;
        int attempts = 0;
        do
        {
            spawnPosition = GetRandomPosition();
            attempts++;
        } while (IsPositionOccupied(spawnPosition, spawnedPositions) && attempts < 10);

        if (attempts < 10)
        {
            spawnedPositions.Add(spawnPosition);
            Instantiate(woodPrefab, spawnPosition, Quaternion.identity);
        }
    }
}

bool IsPositionOccupied(Vector3 position, List<Vector3> positions) {
    foreach (Vector3 pos in positions)
    {
        if (Vector3.Distance(pos, position) < 3.0f) // Überprüfen, ob Position nahe einer anderen Position ist
        {
            return true;
        }
    }
    return false;
}

Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomZ = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector3(randomX, 0, randomZ); // Y bleibt 0, da die Objekte auf dem Boden gespawnt werden sollen
    }
}