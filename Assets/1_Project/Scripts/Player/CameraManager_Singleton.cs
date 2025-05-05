using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    // This script simply turns on the required camera type for players depending on whether they are friendly or mutant, and sets some variables for the cameras
    public static CameraManager Instance { get; private set; }

    public GameObject ThirdPersonCamObject;
    public GameObject FirstPersonCamObject;

    private void Awake()
    {
        // Enforce Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Prevent duplicate instances
    }

    public void SetThirdPersonCamera(Transform target)
    {
        if (ThirdPersonCamObject == null || target == null) { Debug.LogWarning("Camera or target is null!"); return; }

        ThirdPersonCamObject.SetActive(true);

        CinemachineCamera virtualCam = ThirdPersonCamObject.GetComponentInChildren<CinemachineCamera>();
        if (virtualCam != null)
        {
            virtualCam.Follow = target;
            //virtualCam.LookAt = target;
        }
        else { Debug.LogWarning($"Camera '{ThirdPersonCamObject.name}' does not have a CinemachineVirtualCamera component!"); }
    }

    public void SetFirstPersonCamera(MutantCharacterOld character, Transform target)
    {
        if (FirstPersonCamObject == null || target == null) { Debug.LogWarning("Camera or target is null!"); return; }

        FirstPersonCamObject.SetActive(true);

        CinemachineCamera virtualCam = FirstPersonCamObject.GetComponent<CinemachineCamera>();
        if (virtualCam != null)
        {
            virtualCam.Follow = target;
        }
        else { Debug.LogWarning($"Camera '{FirstPersonCamObject.name}' does not have a CinemachineVirtualCamera component!"); }

        character.cinemachineCamera = FirstPersonCamObject.transform; // pass camera reference back for the mesh to follow the local cam
    }

}