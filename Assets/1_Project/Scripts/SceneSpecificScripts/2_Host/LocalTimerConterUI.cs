using UnityEngine;
using TMPro;

public class LocalTimerConterUI : MonoBehaviour
{
    public TextMeshProUGUI timerText; 

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        int seconds = Mathf.FloorToInt(timer);
        timerText.text = timer.ToString("F1") + "s"; // "F1" = one decimal place
    }
}
//  C6EADE
//  407B41