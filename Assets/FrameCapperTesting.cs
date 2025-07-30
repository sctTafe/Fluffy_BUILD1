using UnityEngine;

public class FrameCapperTesting : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = 60; // Or lower, depending on your needs
        QualitySettings.vSyncCount = 1;   // Enable VSync to match monitor refresh rate
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
