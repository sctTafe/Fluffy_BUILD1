using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FMODUnity;
using Unity.Netcode;

public class ProximityHeartbeatEffect : NetworkBehaviour
{
    [Header("General Settings")]
    public float maxDistance = 50f;

    [Header("Target Settings")]
    [SerializeField] private string targetTag = "Mutant";
    private float targetCheckInterval = 5f;
    private float targetCheckTimer = 0f;

    [Header("Vignette Settings")]
    public Volume volume;
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0.3f, 1, 0.6f);
    [SerializeField] private AnimationCurve pulseSpeedCurve = AnimationCurve.Linear(0, 0.5f, 1, 2f);
    public float pulseSpeed = 1f;
    public float _maxIntensity = 0.5f;
    private Vignette vignette;

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

        GameObject mutant = GameObject.FindWithTag(targetTag);
        if (mutant != null)
            target = mutant.transform;
        else
        {
            enabled = false;
            return;
        }

        if (volume.profile.TryGet(out vignette))
        {
            vignette.intensity.overrideState = true;
        }

        if (!heartbeatEvent.IsNull)
        {
            heartbeatInstance = RuntimeManager.CreateInstance(heartbeatEvent);
            heartbeatInstance.setVolume(_minVol);
            heartbeatInstance.start();
        }
        else
        {
            Debug.LogWarning("You need to add a heartbeat sound event in the inspector");
        }
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
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        float proximity = 1f - normalizedDistance; // 0 = far, 1 = close

        if (heartbeatInstance.isValid())
        {
            // Update 3D position of the heartbeat
            heartbeatInstance.set3DAttributes(RuntimeUtils.To3DAttributes(target.position));

            // Volume scales with proximity
            float volume = Mathf.Lerp(_minVol, _maxVol, proximity);
            heartbeatInstance.setVolume(volume);

            // Pitch scales with proximity (optional, can be linear or via curve in FMOD)
            heartbeatInstance.setParameterByName("DistanceToMutant", proximity);

            // Pulse speed for vignette scales with proximity
            float pulseRate = pulseSpeedCurve.Evaluate(proximity);
            pulseTimer += Time.deltaTime * pulseRate * pulseSpeed;

            // Vignette intensity also scales with proximity
            float pulseValue = pulseCurve.Evaluate(pulseTimer % 1f);
            float intensity = Mathf.Lerp(0.0f, _maxIntensity, proximity) * pulseValue;
            vignette.intensity.value = intensity;
        }
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
