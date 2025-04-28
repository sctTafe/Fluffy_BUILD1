using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraFollow_Singleton : Singleton<PlayerCameraFollow_Singleton> {
    [SerializeField]
    private float amplitudeGain = 0.5f;

    [SerializeField]
    private float frequencyGain = 0.5f;

    private CinemachineCamera cinemachineVirtualCamera;

    private void Awake() {
        cinemachineVirtualCamera = GetComponent<CinemachineCamera>();
    }

    public void AssignToFollowPlayer(Transform transform) {
        // not all scenes have a cinemachine virtual camera so return in that's the case
        if (cinemachineVirtualCamera == null) return;

        cinemachineVirtualCamera.Follow = transform;

        var noiseComponent = cinemachineVirtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (noiseComponent != null) {
            noiseComponent.AmplitudeGain = amplitudeGain;
            noiseComponent.FrequencyGain = frequencyGain;
        }

    }
}

