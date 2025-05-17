using FMODUnity;
using UnityEngine;

public class Sounds_PlayTreeSoundFX : MonoBehaviour
{
    public void fn_PlaySounds()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Wildlife/BirdsScared");
    }
}
