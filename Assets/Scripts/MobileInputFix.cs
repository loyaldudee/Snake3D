using UnityEngine;
using TMPro;

public class MobileWebGLKeyboard : MonoBehaviour
{
    public TMP_InputField inputField;
    private TouchScreenKeyboard keyboard;

    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        inputField.shouldHideMobileInput = true;
#endif
    }

    // Called from OnSelect() event on the input field
    public void OnClickInputField()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        keyboard = TouchScreenKeyboard.Open(inputField.text,
                                            TouchScreenKeyboardType.Default,
                                            false, false, false, false);
#endif
    }

    void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (keyboard != null)
        {
            // Live sync text with keyboard input
            inputField.text = keyboard.text;

            if (keyboard.status == TouchScreenKeyboard.Status.Done ||
                keyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                keyboard = null;
                inputField.DeactivateInputField();
            }
        }
#endif
    }
}
