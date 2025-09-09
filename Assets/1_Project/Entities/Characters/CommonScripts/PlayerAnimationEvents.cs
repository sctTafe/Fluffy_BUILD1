using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Unity.Netcode;
using StarterAssets;
using UnityEngine.VFX;

public class PlayerAnimationEvents : MonoBehaviour
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
    public EventReference jumpEvent;
    public EventReference landEvent;


    [Header("Playback")]
    [Tooltip("If true, sounds will only be played on the owning client")]
    public bool playOnlyForOwner = true;
    [Tooltip("Use 3D positioned playback (follows this GameObject).")]
    public bool play3D = true;

    // Visual Effect graph reference for VFX Graph events (e.g. claw attack)
    [Header("VFX")]
    public VisualEffect clawGraph;

    // Player type to control FMOD labeled parameter
    public enum PlayerType { Mutant = 1, Fluffy = 0 }
    [Tooltip("Set the player type so FMOD can adapt (Mutant or Fluffy)")]
    public PlayerType playerType = PlayerType.Fluffy;

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

    // Helper to play a one-shot FMOD event and set a labeled parameter before starting
    private void PlayOneShotWithLabel(EventReference ev, string parameterName, string label)
    {
        if (!ShouldPlay()) return;
        if (ev.IsNull) return;

        EventInstance instance = RuntimeManager.CreateInstance(ev);
        if (play3D)
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        // Set labeled parameter by name (FMOD supports setParameterByNameWithLabel)
        try
        {
            instance.setParameterByNameWithLabel(parameterName, label);
        }
        catch
        {
            // if setParameterByNameWithLabel isn't available or fails, fall back to numeric set
            instance.setParameterByName(parameterName, (float)playerType);
        }

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

    // Jump/Land now send FMOD parameter label indicating player type on the jump/land events
    public void OnJump()
    {
        // Use labeled FMOD parameter 'PlayerType' so the event can branch by label
        PlayOneShotWithLabel(jumpEvent, "PlayerType", GetPlayerTypeLabel());
    }
    public void OnLand()
    {
        PlayOneShotWithLabel(landEvent, "PlayerType", GetPlayerTypeLabel());
    }

    private string GetPlayerTypeLabel()
    {
        return playerType == PlayerType.Mutant ? "Mutant" : "Fluffy";
    }

    public void PlayFootstep()
    {
        if (!ShouldPlay()) return;
        if (footstepEvent.IsNull) return;

        // Check if character is grounded before playing footstep sounds
        if (!IsGrounded()) return;

        // Get terrain type at current position
        int surfaceType = terrainDetector.GetSurfaceType(transform.position);

        //Debug.Log(surfaceType);
        // Create the instance
        EventInstance instance = RuntimeManager.CreateInstance(footstepEvent);
        
        // Apply 3D positioning consistently with other sounds
        if (play3D) 
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

        // Set the parameter for surface step type
        instance.setParameterByName("FootstepSurfaceType", surfaceType);

        Debug.Log("Footstep on surface type: " + surfaceType);

        instance.start();
        instance.release(); // Release immediately after starting (safe for one-shots)
    }

    /// <summary>
    /// Checks if the character is grounded by looking for movement controllers on this GameObject
    /// </summary>
    private bool IsGrounded()
    {
        // Try to find a ThirdPersonController (Standard Unity Starter Assets)
        var thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        if (thirdPersonController != null)
            return thirdPersonController.Grounded;

        // Try to find the Netcode version
        var thirdPersonControllerNetcode = GetComponent<StarterAssets.ThirdPersonController_Netcode>();
        if (thirdPersonControllerNetcode != null)
            return thirdPersonControllerNetcode.Grounded;

        // Try to find Scott's backup controller
        var scottsBackupController = GetComponent<ScottsBackup_ThirdPersonController>();
        if (scottsBackupController != null)
            return scottsBackupController.Grounded;

        // Try to find AnimalCharacter controller
        var animalCharacter = GetComponent<AnimalCharacter>();
        if (animalCharacter != null)
            return animalCharacter.IsGrounded;

        // If no movement controller found, default to true (fail-safe)
        Debug.LogWarning($"No movement controller found on {gameObject.name} for grounded check. Defaulting to grounded = true.");
        return true;
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


    // Convenience method specifically for claw attack from animation events
    public void OnClawAttackVFX()
    {
        if (!ShouldPlay()) return;
        if (clawGraph == null) return;

        clawGraph.SendEvent("Attack");
    }

}