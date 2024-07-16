using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolzBhv : MonoBehaviour
{
    public float collectionTime = 3f;
    private float timer = 0f;
    private bool isPlayerNearby = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called");
        HolzBhv holzBhv = other.GetComponent<HolzBhv>();
        if (holzBhv != null && other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger zone of Holz");
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit called");
        HolzBhv holzBhv = other.GetComponent<HolzBhv>();
        if (holzBhv != null && other.CompareTag("Player"))
        {
            Debug.Log("Player exited the trigger zone of Holz");
            isPlayerNearby = false;
            timer = 0f;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isPlayerNearby)
        {
            timer += Time.deltaTime;
            Debug.Log("Player is still in the trigger zone. Timer: " + timer);
            if (timer >= collectionTime)
            {
                Debug.Log("Player collected the object");
                // Objekt einsammeln und zerstören
                ScoreManager.Instance.AddScore(1); // Beispielhafte Erhöhung des Scores um 1
                Destroy(other.gameObject);
                isPlayerNearby = false;
            }
        }
    }
}
