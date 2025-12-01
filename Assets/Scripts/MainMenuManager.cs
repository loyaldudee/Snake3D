using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 
using System.Collections.Generic;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu UI")]
    public GameObject mainMenuPanel;
    public TMP_InputField nameInputField; 
    public Button submitButton;
    public TMP_Text nameDisplayText; 
    public Button playButton;
    public Button resetButton;

    [Header("Leaderboard UI")]
    public GameObject leaderboardPanel;
    public Button openLeaderboardButton;
    public TMP_Text leaderboardText; 

    [Header("Recent Scores UI")]
    public GameObject recentScorePanel;
    public Button openRecentScoresButton;
    public TMP_Text recentScoreText; 

    [Header("Scene Settings")]
    public string gameSceneName = "GameScene"; 

    private string playerCodeKey = "PlayerCode"; 
    private string playerNameKey = "PlayerName";
    private string recentScoresKey = "RecentScores"; // Added back

    void Start()
    {
        if (PlayerPrefs.HasKey(playerCodeKey))
        {
            string savedName = PlayerPrefs.GetString(playerNameKey, "Player");
            OnLoginSuccess(savedName);
        }
        else
        {
            ResetUI();
        }

        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (resetButton != null) resetButton.onClick.AddListener(ResetLocalData);
        if (submitButton != null) submitButton.onClick.AddListener(OnSubmitPressed);
        
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(ValidateInput);
            ValidateInput(nameInputField.text);
        }

        if (openLeaderboardButton != null) openLeaderboardButton.onClick.AddListener(ToggleLeaderboard);
        if (openRecentScoresButton != null) openRecentScoresButton.onClick.AddListener(ToggleRecentScores);

        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (recentScorePanel != null) recentScorePanel.SetActive(false);
        SetMainMenuVisible(true);
    }

    // --- LEADERBOARD (API) ---
    public void ToggleLeaderboard()
    {
        bool isActive = leaderboardPanel.activeSelf;
        if (isActive)
        {
            leaderboardPanel.SetActive(false);
            SetMainMenuVisible(true);
        }
        else
        {
            leaderboardPanel.SetActive(true);
            if (recentScorePanel != null) recentScorePanel.SetActive(false);
            SetMainMenuVisible(false);
            
            leaderboardText.text = "Loading Global Ranks...";
            
            // Call API
            APIManager.Instance.GetLeaderboard((scores) => {
                string finalString = "--- GLOBAL LEADERBOARD ---\n\n";
                int rank = 1;
                foreach(var s in scores)
                {
                    finalString += $"{rank}. {s.name}: {s.score}\n";
                    rank++;
                }
                leaderboardText.text = finalString;
            });
        }
    }

    // --- RECENT SCORES (LOCAL PLAYER PREFS) ---
    public void ToggleRecentScores()
    {
        bool isActive = recentScorePanel.activeSelf;
        if (isActive)
        {
            recentScorePanel.SetActive(false);
            SetMainMenuVisible(true);
        }
        else
        {
            recentScorePanel.SetActive(true);
            if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
            SetMainMenuVisible(false);

            // Read from Local PlayerPrefs
            UpdateRecentScoresTextLocal();
        }
    }

    void UpdateRecentScoresTextLocal()
    {
        string saved = PlayerPrefs.GetString(recentScoresKey, "");
        if (string.IsNullOrEmpty(saved))
        {
            recentScoreText.text = "No games played yet";
        }
        else
        {
            string finalString = "--- LOCAL HISTORY ---\n\n";
            string[] scores = saved.Split(',');
            
            // Show latest scores first (reverse loop)
            for (int i = scores.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(scores[i]))
                    finalString += "Score: " + scores[i] + "\n";
            }
            recentScoreText.text = finalString;
        }
    }

    void SetMainMenuVisible(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(show);
    }

    void ValidateInput(string input) { if (submitButton != null) submitButton.interactable = !string.IsNullOrEmpty(input); }
    
    public void OnSubmitPressed() {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text)) {
            string enteredName = nameInputField.text;
            submitButton.interactable = false;
            
            APIManager.Instance.RegisterPlayer(enteredName, 
                (playerCode) => {
                    PlayerPrefs.SetString(playerCodeKey, playerCode);
                    PlayerPrefs.SetString(playerNameKey, enteredName);
                    PlayerPrefs.Save();
                    OnLoginSuccess(enteredName);
                },
                (error) => {
                    Debug.LogError("Registration Failed: " + error);
                    submitButton.interactable = true;
                }
            );
        }
    }

    void OnLoginSuccess(string name) {
        if (nameDisplayText != null) { nameDisplayText.text = "Player: " + name; nameDisplayText.gameObject.SetActive(true); }
        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (playButton != null) playButton.interactable = true; 
        if (resetButton != null) resetButton.gameObject.SetActive(true);
    }

    public void PlayGame() { if (!string.IsNullOrEmpty(gameSceneName)) SceneManager.LoadScene(gameSceneName); }
    
    public void ResetLocalData() { 
        PlayerPrefs.DeleteKey(playerCodeKey); 
        PlayerPrefs.DeleteKey(playerNameKey);
        PlayerPrefs.DeleteKey(recentScoresKey); // Clear history too
        ResetUI(); 
    }

    void ResetUI() {
        if (nameDisplayText != null) { nameDisplayText.text = ""; nameDisplayText.gameObject.SetActive(false); }
        if (nameInputField != null) { nameInputField.text = ""; nameInputField.gameObject.SetActive(true); }
        if (submitButton != null) { submitButton.gameObject.SetActive(true); submitButton.interactable = false; }
        if (playButton != null) playButton.interactable = false;
        if (resetButton != null) resetButton.gameObject.SetActive(false);
    }
}