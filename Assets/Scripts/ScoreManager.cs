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

    int GetFibonacci(int n)
    {
        if (n <= 0) return 0;
        if (n == 1) return 1;
        int a = 0, b = 1;
        for (int i = 2; i <= n; i++) { int temp = a + b; a = b; b = temp; }
        return b;
    }
}