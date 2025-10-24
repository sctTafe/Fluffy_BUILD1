using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using FMODUnity;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class ProximityHeartbeatEffect_MultiTarget : NetworkBehaviour
{
    [Header("General Settings")]
    public float _maxDistance = 20f;

    [Header("Target Settings")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private float targetScanInterval = 1f; // How often to search for new targets
    [SerializeField] private float targetSortInterval = 0.5f; // How often to resort by distance
    private float targetScanTimer = 0f;
    private float targetSortTimer = 0f;
    private List<Transform> targets = new List<Transform>();
    private Transform closestTarget;

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

    private float pulseTimer;

    void Start()
    {
        if (!IsOwner)
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

        // Initial scan for targets
        ScanForTargets();
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
        // Periodically scan for new targets
        targetScanTimer += Time.deltaTime;
        if (targetScanTimer >= targetScanInterval)
        {
            ScanForTargets();
            targetScanTimer = 0f;
        }

        // Periodically sort targets by distance
        targetSortTimer += Time.deltaTime;
        if (targetSortTimer >= targetSortInterval)
        {
            UpdateClosestTarget();
            targetSortTimer = 0f;
        }

        // If no valid target, reset effects and return
        if (closestTarget == null)
        {
            if (vignette != null)
                vignette.intensity.value = 0f;
            if (heartbeatInstance.isValid())
                heartbeatInstance.setVolume(_minVol);
            return;
        }

        // Calculate distance to closest target
        float distance = Vector3.Distance(transform.position, closestTarget.position);
        float normalizedDistance = Mathf.Clamp01(distance / _maxDistance);
        float proximity = 1f - normalizedDistance; // 0 = far, 1 = close

        if (heartbeatInstance.isValid())
        {
            // Update 3D position of the heartbeat
            heartbeatInstance.set3DAttributes(RuntimeUtils.To3DAttributes(closestTarget.position));

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

    private void ScanForTargets()
    {
        GameObject[] foundObjects = GameObject.FindGameObjectsWithTag(targetTag);

        // Clear the list and repopulate
        targets.Clear();

        foreach (GameObject obj in foundObjects)
        {
            if (obj != null)
            {
                targets.Add(obj.transform);
            }
        }

        Debug.Log($"Found {targets.Count} target(s) with tag '{targetTag}'");
    }

    private void UpdateClosestTarget()
    {
        // Remove any null/destroyed targets from the list
        targets.RemoveAll(t => t == null);

        if (targets.Count == 0)
        {
            closestTarget = null;
            return;
        }

        // Sort by distance and get the closest
        closestTarget = targets
            .OrderBy(t => Vector3.Distance(transform.position, t.position))
            .FirstOrDefault();
    }
}