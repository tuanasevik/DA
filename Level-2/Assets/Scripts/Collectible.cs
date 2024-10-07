using UnityEngine;
using System.Collections;

public class Collectible : MonoBehaviour
{
    public float collectionTime;
    private bool isPlayerNearby = false;
    private float timer = 0f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            timer = 0f; // Timer zurücksetzen, wenn der Spieler weggeht
        }
    }


    void Update()
    {
        if (isPlayerNearby)
        {
            timer += Time.deltaTime;
            if (timer >= collectionTime)
            {
                ScoreManager.Instance.IncreaseScore();
                ObjectSpawner.Instance.decreaseCurrentObjectCount();
                Destroy(gameObject); // Zerstört nur dieses Objekt
            }
        }
    }
}
