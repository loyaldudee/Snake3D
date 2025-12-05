using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TMP_Text scoreText; 

    private int currentScore = 0;
    private int foodEatenCount = 0;
    
    // Keys for PlayerPrefs
    private const string RecentScoresKey = "RecentScores";
    private const string PlayerCodeKey = "PlayerCode"; 

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints()
    {
        // OLD: int pointsToAdd = GetFibonacci(foodEatenCount + 1);
        
        // NEW: Quadratic Scaling
        // 1st apple: 105 pts
        // 10th apple: 1,500 pts
        // 50th apple: 17,500 pts
        // 100th apple: 60,000 pts
        // Max (Full Grid ~100): Total Score approx 2-3 Million (Safe from 1 Billion limit)
        foodEatenCount++;
        
        int pointsToAdd = (foodEatenCount * 100) + (foodEatenCount * foodEatenCount * 5);
        
        currentScore += pointsToAdd;
        
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore;
        }
    }

    // --- HYBRID SAVE SYSTEM ---
    public void SaveScore()
    {
        // 1. LOCAL SAVE (Managed by PlayerPrefs)
        string savedScores = PlayerPrefs.GetString(RecentScoresKey, "");
        if (string.IsNullOrEmpty(savedScores))
        {
            savedScores = currentScore.ToString();
        }
        else
        {
            // Append new score to the end (comma separated)
            savedScores += "," + currentScore;
        }
        PlayerPrefs.SetString(RecentScoresKey, savedScores);
        PlayerPrefs.Save();
        Debug.Log("Score Saved Locally.");

        // 2. BACKEND SUBMISSION (For Global Leaderboard)
        if (PlayerPrefs.HasKey(PlayerCodeKey))
        {
            string playerCode = PlayerPrefs.GetString(PlayerCodeKey);
            
            if (APIManager.Instance != null)
            {
                APIManager.Instance.SubmitScore(playerCode, currentScore);
            }
        }
    }
    
    // REMOVED: GetFibonacci function is no longer needed.
}