using UnityEngine;
using Unity.Cinemachine;

public class ScottsBackup_3RDPersonCamMng : Singleton<ScottsBackup_3RDPersonCamMng>
{
    private CinemachineCamera cinemachineVirtualCamera;

    private void Awake()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineCamera>();
    }

    public void fn_BindChracterToCam(Transform transform)
    {
        cinemachineVirtualCamera.Follow = transform;
    }

}
