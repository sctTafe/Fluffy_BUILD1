using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FMODUnity;

public class ProximityHeartbeatEffect : MonoBehaviour
{
    public float maxDistance = 50f;

    [Header("Vignette Settings")]
    public Volume volume;
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0.3f, 1, 0.6f);
    public float pulseSpeed = 1f;
    private Vignette vignette;

    [Header("FMOD Settings")]
    public EventReference heartbeatEvent;
    private FMOD.Studio.EventInstance heartbeatInstance;

    private Transform target;
    private float pulseTimer;

    void Start()
    {
        heartbeatInstance.setParameterByName("DistanceToMutant", 999); //Abitrary distance away to start with
        GameObject mutant = GameObject.FindWithTag("Mutant");
        if (mutant != null)
            target = mutant.transform;
        else
        {
            this.enabled = false; // Turn this off if no mutant found for now
        }

        if (volume.profile.TryGet(out vignette))
        {
            vignette.intensity.overrideState = true;
        }

        if (!heartbeatEvent.IsNull)
        {
            heartbeatInstance = RuntimeManager.CreateInstance(heartbeatEvent);
            heartbeatInstance.start();
        }
    }

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        float normalizedDistance = Mathf.Clamp01(distance / maxDistance);
        float proximity = 1f - normalizedDistance;

        if (heartbeatInstance.isValid())
            heartbeatInstance.setParameterByName("DistanceToMutant", proximity);

        pulseTimer += Time.deltaTime * Mathf.Lerp(0.5f, 2f, proximity) * pulseSpeed;
        float pulseValue = pulseCurve.Evaluate(pulseTimer % 1f);
        float intensity = Mathf.Lerp(0.1f, 0.5f, proximity) * pulseValue;
        vignette.intensity.value = intensity;
    }

    void OnDestroy()
    {
        if (heartbeatInstance.isValid())
        {
            heartbeatInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            heartbeatInstance.release();
        }
    }
}
