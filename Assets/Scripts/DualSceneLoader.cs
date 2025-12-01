using UnityEngine;
using UnityEngine.SceneManagement;

public class DualSceneLoader : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Name of the scene for the first button")]
    public string sceneOneName;

    [Tooltip("Name of the scene for the second button")]
    public string sceneTwoName;

    // Assign this to the first Button's OnClick() event
    public void LoadSceneOne()
    {
        if (!string.IsNullOrEmpty(sceneOneName))
        {
            SceneManager.LoadScene(sceneOneName);
        }
        else
        {
            Debug.LogError("Scene One name is empty! Please set it in the Inspector.");
        }
    }

    // Assign this to the second Button's OnClick() event
    public void LoadSceneTwo()
    {
        if (!string.IsNullOrEmpty(sceneTwoName))
        {
            SceneManager.LoadScene(sceneTwoName);
        }
        else
        {
            Debug.LogError("Scene Two name is empty! Please set it in the Inspector.");
        }
    }
}