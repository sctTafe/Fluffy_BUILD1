using TMPro;
using UnityEngine;

public class GameTimerUI : MonoBehaviour
{
    [SerializeField] private NetworkGameTimer gameTimer;
    [SerializeField] private TextMeshProUGUI timerText;  // Reference to TextMeshProUGUI

    private void Update()
    {
        if (gameTimer != null && timerText != null)
        {
            // Update the TextMeshPro text with the formatted game time
            timerText.text = $"Game Timer: {gameTimer.GetCurrentTimeFormatted()}";
        }
    }

}
