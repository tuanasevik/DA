using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public Text scoreText; // Referenz zum UI-Text-Element
    private int score = 0; // Der aktuelle Score

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Falls du willst, dass der ScoreManager nicht bei Szenenwechsel zerstört wird
        }
    }

    private void Start()
    {
        UpdateScoreText();
    }

    // Methode zum Hinzufügen von Punkten
    public void AddScore(int points)
    {
        ScoreManager.Instance.score += points;
        UpdateScoreText();
    }

    // Methode zum Aktualisieren des Score-Texts
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + ScoreManager.Instance.score.ToString();
    }
}