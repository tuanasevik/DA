using UnityEngine;

public class Collectible : MonoBehaviour
{
    public float collectionTime; // Setze dies im Inspector für jedes Prefab
    private bool isPlayerNearby = false;
    private float timer = 0f;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            // Deaktiviere die Kollision zwischen Player und Ground
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Floor"), true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            timer = 0f;

            // Aktiviere die Kollision zwischen Player und Ground wieder
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Floor"), false);
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
                Destroy(gameObject); // Zerstört dieses Objekt
                isPlayerNearby = false;

                // Aktiviere die Kollision zwischen Player und Ground nach Zerstörung des Objekts
                Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Floor"), false);
            }
        }
    }
}
