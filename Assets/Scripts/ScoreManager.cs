using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    //public Text scoreText; // Legacy Text
    public TMPro.TMP_Text scoreText; // Uncomment for TextMeshPro

    private int currentScore = 0;
    private int foodEatenCount = 0;
    private const string RecentScoresKey = "RecentScores";

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints()
    {
        // Fibonacci Points: 1, 1, 2, 3, 5, 8...
        int pointsToAdd = GetFibonacci(foodEatenCount + 1);
        currentScore += pointsToAdd;
        foodEatenCount++;
        
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    // Call this when Game Over happens
    public void SaveScore()
    {
        // 1. Get existing scores string (e.g., "100,50,20")
        string savedScores = PlayerPrefs.GetString(RecentScoresKey, "");

        // 2. Add new score to the end
        if (string.IsNullOrEmpty(savedScores))
        {
            savedScores = currentScore.ToString();
        }
        else
        {
            savedScores += "," + currentScore;
        }

        // 3. Save it back
        PlayerPrefs.SetString(RecentScoresKey, savedScores);
        PlayerPrefs.Save();
        
        Debug.Log("Score Saved: " + currentScore);
    }

    int GetFibonacci(int n)
    {
        if (n <= 0) return 0;
        if (n == 1) return 1;
        int a = 0, b = 1;
        for (int i = 2; i <= n; i++) { int temp = a + b; a = b; b = temp; }
        return b;
    }
}