using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshPro scoreText;

    public int Score { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateScoreboard();
    }

    // Adds points to the scoreboard
    public void AddPoints(int points)
    {
        Score += points;
        UpdateScoreboard();
    }

    // Resets the scoreboard points
    public void ResetScore()
    {
        Score = 0;
        UpdateScoreboard();
    }


    // Updates the current scoreboard text
    private void UpdateScoreboard()
    {
        if (scoreText != null)
        {
            scoreText.text = "Points: " + Score.ToString();
        }
    }
}