using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Just a nice each component to check for and a value to check for to find if the trigger is for a local player - used in combo with OnTrigerEnterUnityEvent_LocalPlayer
/// </summary>
public class ScottsBackup_IsLocalPlayerCheck : NetworkBehaviour
{
    public bool _isLocalPlayer {  get; private set; }

    void Start()
    {
        _isLocalPlayer = false;

        //Disable this script unless it on the owner
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }

        _isLocalPlayer = true;
    }
}
