using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerSounds : MonoBehaviour
{
    public EventReference footstepEvent;
    public TerrainTypeDetector terrainDetector;

    private void Start()
    {
        terrainDetector = new TerrainTypeDetector();
    }

    public void PlayFootstep()
    {
        if (footstepEvent.IsNull) return;

        // Get terrain type at current position
        int surfaceType = terrainDetector.GetSurfaceType(transform.position);

        //Debug.Log(surfaceType);
        // Create the instance
        EventInstance instance = RuntimeManager.CreateInstance(footstepEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        // Set the parameter for surface step type
        instance.setParameterByName("FootstepSurfaceType", surfaceType);

        instance.start();
        instance.release(); // Release immediately after starting (safe for one-shots)
    }
}