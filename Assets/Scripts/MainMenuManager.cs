using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Required for TextMeshPro

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nameInputField; 
    public Button submitButton;
    public TMP_Text nameDisplayText; // Field to show the submitted name
    public Button playButton;
    public Button resetButton;
    public TMP_Text welcomeText; // Optional: Keep for legacy welcome text or remove

    [Header("Scene Settings")]
    public string gameSceneName = "SampleScene"; // Field to input scene name in Editor

    private string playerNameKey = "PlayerName";

    void Start()
    {
        // 1. Check if we already have a saved name
        if (PlayerPrefs.HasKey(playerNameKey))
        {
            string savedName = PlayerPrefs.GetString(playerNameKey);
            OnNameSubmitted(savedName);
        }
        else
        {
            // Initial State: No name
            ResetUI();
        }

        // 2. Setup Button Listeners
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (resetButton != null) resetButton.onClick.AddListener(ResetName);
        if (submitButton != null) submitButton.onClick.AddListener(OnSubmitPressed);

        // 3. Setup Input Field Listener (Optional: Check continuously)
        if (nameInputField != null)
        {
            nameInputField.onValueChanged.AddListener(ValidateInput);
        }
        
        // Initial Validation
        ValidateInput(nameInputField != null ? nameInputField.text : "");
    }

    // Called when user types in the input field
    void ValidateInput(string input)
    {
        // Only allow submitting if text is not empty
        if (submitButton != null) 
            submitButton.interactable = !string.IsNullOrEmpty(input);
    }

    public void OnSubmitPressed()
    {
        if (nameInputField != null && !string.IsNullOrEmpty(nameInputField.text))
        {
            string enteredName = nameInputField.text;
            PlayerPrefs.SetString(playerNameKey, enteredName);
            PlayerPrefs.Save();
            
            OnNameSubmitted(enteredName);
        }
    }

    void OnNameSubmitted(string name)
    {
        // Update Display Text
        if (nameDisplayText != null)
        {
            nameDisplayText.text = "Player: " + name;
            nameDisplayText.gameObject.SetActive(true);
        }

        // Hide Input UI
        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        if (submitButton != null) submitButton.gameObject.SetActive(false);

        // Enable Play Button
        if (playButton != null) 
        {
            playButton.interactable = true; // Enable button
            playButton.gameObject.SetActive(true); // Show button if hidden
        }
        
        if (resetButton != null) resetButton.gameObject.SetActive(true);
    }

    public void PlayGame()
    {
        // Load the configured Game Scene
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("Game Scene Name is empty in MainMenuManager!");
        }
    }

    public void ResetName()
    {
        PlayerPrefs.DeleteKey(playerNameKey);
        ResetUI();
        Debug.Log("Name Reset!");
    }

    void ResetUI()
    {
        // Clear name display
        if (nameDisplayText != null) 
        {
            nameDisplayText.text = "";
            nameDisplayText.gameObject.SetActive(false);
        }

        // Show Input UI
        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.gameObject.SetActive(true);
        }
        if (submitButton != null) 
        {
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = false; // Disable until typed
        }

        // Disable Play Button until name is submitted
        if (playButton != null) 
        {
            playButton.interactable = false; 
            // Optional: Hide it completely -> playButton.gameObject.SetActive(false);
        }
        
        if (resetButton != null) resetButton.gameObject.SetActive(false);
    }
}