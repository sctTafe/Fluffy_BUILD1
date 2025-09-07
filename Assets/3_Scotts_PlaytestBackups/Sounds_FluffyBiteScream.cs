using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Sounds_FluffyBiteScream : MonoBehaviour
{
    [SerializeField]
    public EventReference fmodEventReference;

    private EventInstance eventInstance;
    private bool isPlaying = false;

    // Call this to start looping the sound
    public void fn_StartLoopingSound()
    {
        if (isPlaying) return;

        eventInstance = RuntimeManager.CreateInstance(fmodEventReference);
        eventInstance.setParameterByName("Loop", 1f); // Optional: if you have a loop param
        eventInstance.start();
        isPlaying = true;
    }

    // Call this to stop the looping sound
    public void fn_StopLoopingSound()
    {
        if (!isPlaying) return;

        eventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        eventInstance.release();
        isPlaying = false;
    }
}
