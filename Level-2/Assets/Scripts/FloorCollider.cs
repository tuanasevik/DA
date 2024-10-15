using UnityEngine;

public class FloorCollider : MonoBehaviour
{
    private bool isPlayerOnGround = false;
    private float timer = 0f;
    public float timeToReduceScore = 3f; // Zeit in Sekunden, bevor der Score verringert wird
    public int scorePenalty = 1; // Die Menge an Punkten, die abgezogen wird

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnGround = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnGround = false;
            timer = 0f; // Timer zurücksetzen, wenn der Spieler den Boden verlässt
        }
    }

    void Update()
    {
        if (isPlayerOnGround)
        {
            timer += Time.deltaTime;
            if (timer >= timeToReduceScore)
            {
                ScoreManager.Instance.AddScore(-scorePenalty); // Verringere den Score
                timer = 0f; // Timer zurücksetzen, um den Score alle 3 Sekunden zu verringern
            }
        }
    }
}
