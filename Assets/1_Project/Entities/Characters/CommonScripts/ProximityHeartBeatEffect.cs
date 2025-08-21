using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FMODUnity;
using Unity.Netcode; // Add this at the top

public class ProximityHeartbeatEffect : NetworkBehaviour
{
    public float maxDistance = 50f;

    [Header("Target Settings")]
    [SerializeField] private string targetTag = "Mutant"; // Tag to look for
    private float targetCheckInterval = 10f;
    private float targetCheckTimer = 0f;

    [Header("Vignette Settings")]
    public Volume volume;
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0.3f, 1, 0.6f);
    public float pulseSpeed = 1f;
    private Vignette vignette;
    public float _maxIntensity = 0.5f;

    [Header("Sound Volume Settings")]
    public float _minVol = 0.3f;
    public float _maxVol = 1f;


    [Header("FMOD Settings")]
    public EventReference heartbeatEvent;
    private FMOD.Studio.EventInstance heartbeatInstance;




    private Transform target;
    private float pulseTimer;



    void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        GameObject mutant = GameObject.FindWithTag("Mutant");
        if (mutant != null)
            target = mutant.transform;
        else
        {
            this.enabled = false; // Turn this off if no mutant found for now
            return;
        }

        if (volume.profile.TryGet(out vignette))
        {
            vignette.intensity.overrideState = true;
        }

        if (!heartbeatEvent.IsNull)
        {
            heartbeatInstance = RuntimeManager.CreateInstance(heartbeatEvent);
            heartbeatInstance.setVolume(0.3f); // Start at 30% volume
            heartbeatInstance.start();
            heartbeatInstance.setParameterByName("DistanceToMutant", 100); //Abitrary distance away to start with
        }
        else { Debug.Log("You need to add a heartbeat sound event in the inspector"); }
    }

    public override void OnDestroy()
    {
        
        if (heartbeatInstance.isValid())
        {
            heartbeatInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            heartbeatInstance.release();
        }
        base.OnDestroy();
    }

    void Update()
    {
        if (target == null)
        {
            targetCheckTimer += Time.deltaTime;
            if (targetCheckTimer >= targetCheckInterval)
            {
                CheckForTarget();
                targetCheckTimer = 0f;
            }

            return; // No target to process heartbeat or visuals
        }

        float distance = Vector3.Distance(transform.position, target.position);
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        float proximity = 1f - normalizedDistance;

        if (heartbeatInstance.isValid())
        {
            heartbeatInstance.setParameterByName("DistanceToMutant", proximity);

            // ?? Lerp volume from 0.3 (far) to 1.0 (close)
            float volume = Mathf.Lerp(_minVol, _maxVol, proximity);
            heartbeatInstance.setVolume(volume);
        }
        else
        {
            //Debug.LogWarning("Distance Param Invalid");
        }

        pulseTimer += Time.deltaTime * Mathf.Lerp(0.5f, 2f, proximity) * pulseSpeed;
        float pulseValue = pulseCurve.Evaluate(pulseTimer % 1f);
        float intensity = Mathf.Lerp(0.1f, _maxIntensity, proximity) * pulseValue;
        vignette.intensity.value = intensity;
    }



    private void CheckForTarget()
    {
        GameObject found = GameObject.FindWithTag(targetTag);
        if (found != null)
        {
            target = found.transform;
            Debug.Log("Target reacquired: " + target.name);
        }
    }
}
