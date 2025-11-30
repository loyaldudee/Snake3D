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
    public TMP_Text welcomeText; 

    [Header("Leaderboard UI")]
    public GameObject leaderboardPanel;
    public Button openLeaderboardButton;
    public TMP_Text leaderboardText; 

    [Header("Recent Scores UI")]
    public GameObject recentScorePanel;
    public Button openRecentScoresButton;
    public TMP_Text recentScoreText; 

    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene"; 

    private string playerNameKey = "PlayerName";
    private string recentScoresKey = "RecentScores";

    void Start()
    {
        if (PlayerPrefs.HasKey(playerNameKey))
        {
            string savedName = PlayerPrefs.GetString(playerNameKey);
            OnNameSubmitted(savedName);
        }
        else
        {
            ResetUI();
        }

        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (resetButton != null) resetButton.onClick.AddListener(ResetName);
        if (submitButton != null) submitButton.onClick.AddListener(OnSubmitPressed);
        
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(ValidateInput);
            ValidateInput(nameInputField.text);
        }

        if (openLeaderboardButton != null) openLeaderboardButton.onClick.AddListener(ToggleLeaderboard);
        if (openRecentScoresButton != null) openRecentScoresButton.onClick.AddListener(ToggleRecentScores);

        // Initial State
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (recentScorePanel != null) recentScorePanel.SetActive(false);
        SetMainMenuVisible(true);
    }

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
            UpdateLeaderboardText();
        }
    }

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
            UpdateRecentScoresText();
        }
    }

    void SetMainMenuVisible(bool show)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(show);
    }

    // --- TEXT POPULATION ---

    void UpdateRecentScoresText()
    {
        if (recentScoreText == null) return;

        string saved = PlayerPrefs.GetString(recentScoresKey, "");
        if (string.IsNullOrEmpty(saved))
        {
            recentScoreText.text = "No games played yet";
        }
        else
        {
            string finalString = "--- RECENT SCORES ---\n\n";
            string[] scores = saved.Split(',');
            
            // Show latest scores first
            for (int i = scores.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(scores[i]))
                    finalString += "Score: " + scores[i] + "\n";
            }
            recentScoreText.text = finalString;
        }
    }

    void UpdateLeaderboardText()
    {
        if (leaderboardText == null) return;
        
        // Static dummy leaderboard for now since we don't have online features
        string finalString = "--- GLOBAL LEADERBOARD ---\n\n";
        finalString += "1. SnakeKing: 5000\n";
        finalString += "2. ProGamer: 3000\n";
        finalString += "3. Viper: 1000\n";
        finalString += "4. Noob: 10\n";
        
        leaderboardText.text = finalString;
    }

    // --- STANDARD LOGIC ---
    void ValidateInput(string input) { if (submitButton != null) submitButton.interactable = !string.IsNullOrEmpty(input); }
    public void OnSubmitPressed() {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text)) {
            string enteredName = nameInputField.text;
            PlayerPrefs.SetString(playerNameKey, enteredName);
            PlayerPrefs.Save();
            OnNameSubmitted(enteredName);
        }
    }
    void OnNameSubmitted(string name) {
        if (nameDisplayText != null) { nameDisplayText.text = "Player: " + name; nameDisplayText.gameObject.SetActive(true); }
        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (playButton != null) playButton.interactable = true; 
        if (resetButton != null) resetButton.gameObject.SetActive(true);
    }
    public void PlayGame() { if (!string.IsNullOrEmpty(gameSceneName)) SceneManager.LoadScene(gameSceneName); else Debug.LogError("Scene Name Missing"); }
    public void ResetName() { PlayerPrefs.DeleteKey(playerNameKey); ResetUI(); }
    void ResetUI() {
        if (nameDisplayText != null) { nameDisplayText.text = ""; nameDisplayText.gameObject.SetActive(false); }
        if (nameInputField != null) { nameInputField.text = ""; nameInputField.gameObject.SetActive(true); }
        if (submitButton != null) { submitButton.gameObject.SetActive(true); submitButton.interactable = false; }
        if (playButton != null) playButton.interactable = false;
        if (resetButton != null) resetButton.gameObject.SetActive(false);
    }
}