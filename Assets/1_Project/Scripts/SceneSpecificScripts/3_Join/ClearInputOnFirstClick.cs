using UnityEngine;
using TMPro;

public class ClearInputOnFirstClick : MonoBehaviour
{
    private TMP_InputField inputField;
    private bool hasBeenClicked = false;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();

        // Subscribe to the onSelect event
        inputField.onSelect.AddListener(OnInputFieldSelected);
    }

    private void OnInputFieldSelected(string currentText)
    {
        // Clear only on first click
        if (!hasBeenClicked)
        {
            inputField.text = "";
            hasBeenClicked = true;
        }
    }

    // Optional: Reset the flag if you want to allow clearing again
    public void ResetClearFlag()
    {
        hasBeenClicked = false;
    }

    void OnDestroy()
    {
        // Clean up the listener
        if (inputField != null)
        {
            inputField.onSelect.RemoveListener(OnInputFieldSelected);
        }
    }
}