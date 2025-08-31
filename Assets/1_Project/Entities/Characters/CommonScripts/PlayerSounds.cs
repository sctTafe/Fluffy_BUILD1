using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.Netcode;

public class PlayerSounds : MonoBehaviour
{
    public EventReference footstepEvent;
    public TerrainTypeDetector terrainDetector;

    [Header("FMOD Events")]
    public EventReference hurtEvent;
    public EventReference biteEvent;
    public EventReference swipeEvent;
    public EventReference revealEvent;
    public EventReference mutantBreathEvent;
    public EventReference pickupEvent;
    public EventReference objectiveCompleteEvent;


    [Header("Playback")]
    [Tooltip("If true, sounds will only be played on the owning client")]
    public bool playOnlyForOwner = true;
    [Tooltip("Use 3D positioned playback (follows this GameObject).")]
    public bool play3D = true;

    // Internal instance for looping sprint sound
    private EventInstance sprintInstance;

    // Cached NetworkObject if present (Unity.Netcode)
    private NetworkObject networkObject;

    private void Start()
    {
        terrainDetector = new TerrainTypeDetector();
    }
    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }

    private bool ShouldPlay()
    {
        if (!playOnlyForOwner) return true;
        if (networkObject == null) return true; // no networking component -> play locally
        return networkObject.IsOwner;
    }

    // Generic helper to play one-shot events (3D attached to this transform if play3D true)
    private void PlayOneShot(EventReference ev)
    {
        if (!ShouldPlay()) return;
        if (ev.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(ev);
        if (play3D)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        instance.release();
    }

    // Animation event entry points (use these names on Animation Events)
    public void OnHurt() => PlayOneShot(hurtEvent);
    public void OnBite() => PlayOneShot(biteEvent);
    public void OnSwipe() => PlayOneShot(swipeEvent);
    public void OnReveal() => PlayOneShot(revealEvent);
    public void OnMutantBreath() => PlayOneShot(mutantBreathEvent);
    public void OnPickup() => PlayOneShot(pickupEvent);
    public void OnObjectiveComplete() => PlayOneShot(objectiveCompleteEvent);

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

    private void OnDisable()
    {
        // Ensure loop is stopped & released if object is disabled/destroyed
        if (sprintInstance.isValid())
        {
            sprintInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            sprintInstance.release();
            sprintInstance.clearHandle();
        }
    }

}