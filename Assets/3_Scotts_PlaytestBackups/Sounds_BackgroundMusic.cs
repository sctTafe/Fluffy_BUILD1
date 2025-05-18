using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Sounds_BackgroundMusic : Singleton<Sounds_BackgroundMusic>
{
    private EventInstance musicInstance;
    private bool musicStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy any duplicate
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Create the instance
        musicInstance = RuntimeManager.CreateInstance("event:/Music/BackgroundMusic");

        // Start playing
        musicInstance.start();

        // Optional: Set it to loop (should already be set in FMOD Studio)
        musicInstance.release(); // Allow instance to be garbage collected when it stops
        musicStarted = true;
    }

    public void fn_StopBackgroundMusicTrack()
    {
        if (musicStarted)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
            musicStarted = false;
        }
    }


    void OnDestroy()
    {
        // Only stop if this is the one that started music
        if (musicStarted)
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }
}
