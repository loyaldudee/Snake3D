using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    [Header("Buttons")]
    public Button gameOverRestartButton;
    public Button gameWinRestartButton;

    void Start()
    {
        // Ensure panels are hidden at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameWinPanel != null) gameWinPanel.SetActive(false);

        // Hook up buttons automatically if assigned
        if (gameOverRestartButton != null) 
            gameOverRestartButton.onClick.AddListener(RestartGame);
        
        if (gameWinRestartButton != null) 
            gameWinRestartButton.onClick.AddListener(RestartGame);
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(true);
            // Optional: Show cursor if hidden
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void ShowGameWin()
    {
        if (gameWinPanel != null) 
        {
            gameWinPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void RestartGame()
    {
        // Reload current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}