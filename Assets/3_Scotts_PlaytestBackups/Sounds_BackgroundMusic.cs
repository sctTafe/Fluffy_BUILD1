using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Sounds_BackgroundMusic : Singleton<Sounds_BackgroundMusic>
{
    private EventInstance musicInstance;

    void Start()
    {
        // Create the instance
        musicInstance = RuntimeManager.CreateInstance("event:/Music/BackgroundMusic");

        // Start playing
        musicInstance.start();

        // Optional: Set it to loop (should already be set in FMOD Studio)
        musicInstance.release(); // Allow instance to be garbage collected when it stops

        DontDestroyOnLoad(gameObject);
    }

    public void fn_StopBackgroundMusicTrack()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }


    void OnDestroy()
    {
        // Stop and release if the object is destroyed
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }
}
