using FMODUnity;
using UnityEngine;

public static class UISoundPlayer
{
    public static void PlayHoverSound()
    {
        RuntimeManager.PlayOneShot("event:/UI/Buttons/ButtonHover");
    }

    public static void PlayClickSound()
    {
        RuntimeManager.PlayOneShot("event:/UI/Buttons/ButtonClick");
    }

    public static void PlayDisabledSound()
    {
        RuntimeManager.PlayOneShot("event:/UI/Buttons/ButtonDisabled");
    }
}
