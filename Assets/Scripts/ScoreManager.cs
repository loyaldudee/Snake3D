using UnityEngine;
using UnityEngine.UI; // Or TMPro if using TextMeshPro

public class ScoreManager : MonoBehaviour
{
    [Header("UI Reference")]
    // public Text scoreText; // Assign a legacy UI Text object here
    public TMPro.TMP_Text scoreText; // Uncomment if using TextMeshPro

    private int currentScore = 0;
    private int foodEatenCount = 0;

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddPoints()
    {
        // Calculate points based on Fibonacci
        // Sequence: 1, 1, 2, 3, 5, 8, 13... based on foodEatenCount
        // foodEatenCount starts at 0. 
        // 1st food (count=0) -> Fib(1) = 1
        // 2nd food (count=1) -> Fib(2) = 1
        // 3rd food (count=2) -> Fib(3) = 2
        
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

    // Recursive or Iterative Fibonacci function
    int GetFibonacci(int n)
    {
        if (n <= 0) return 0;
        if (n == 1) return 1;
        
        int a = 0;
        int b = 1;
        
        for (int i = 2; i <= n; i++)
        {
            int temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }

}