using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;
    public GameObject pausePanel; // New Pause Panel

    [Header("Buttons")]
    public Button gameOverRestartButton;
    public Button gameWinRestartButton;
    
    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button menuButton;
    public Button musicToggleButton;
    public Button pauseButton; // On-screen button to trigger pause

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";

    private bool isMusicOn = true;
    private AudioManager audioManager;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();

        // Initial State
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Hook up Restart buttons
        if (gameOverRestartButton != null) 
            gameOverRestartButton.onClick.AddListener(RestartGame);
        
        if (gameWinRestartButton != null) 
            gameWinRestartButton.onClick.AddListener(RestartGame);

        // Hook up Pause Menu buttons
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (menuButton != null) menuButton.onClick.AddListener(GoToMainMenu);
        if (musicToggleButton != null) musicToggleButton.onClick.AddListener(ToggleMusic);
        if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
    }

    public void PauseGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; // Freezes the game
            if (pauseButton != null) pauseButton.gameObject.SetActive(false); // Hide the pause trigger button
        }
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f; // Unfreezes the game
            if (pauseButton != null) pauseButton.gameObject.SetActive(true); // Show the pause trigger button
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time is running for next scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ToggleMusic()
    {
        if (audioManager != null)
        {
            isMusicOn = !isMusicOn;
            audioManager.SetMute(!isMusicOn);
            
            // Optional: Update button text/icon here
            Text btnText = musicToggleButton.GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = isMusicOn ? "Music: ON" : "Music: OFF";
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(true);
            DisablePausePanel(); // Ensure pause panel doesn't interfere
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ShowGameWin()
    {
        if (gameWinPanel != null) 
        {
            gameWinPanel.SetActive(true);
            DisablePausePanel();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void DisablePausePanel()
    {
        // When game over/win happens, disable pause functionality
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseButton != null) pauseButton.gameObject.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Reset time before reloading
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}