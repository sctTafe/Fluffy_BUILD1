using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Local Client Bite_Receiver
/// 
/// Base on Braedon's, 'GrabPlayer' Script
/// Part of a Two Part System with 'Bite_Activator' & 'Bite_Receiver'
/// </summary>
public class ScottsBackup_Receiver_Bite : NetworkBehaviour
{
    private const bool ISDEBUGGING = true;

    public UnityEvent OnBiteStart;
    public UnityEvent OnBiteStop;

    [SerializeField] private ScottsBackup_ThirdPersonController controler;   // The Character Controller
    [SerializeField] private ScottsBackup_ResourceMng healthControler;
    [SerializeField] private Vector3 _positionOffset;

    public bool IsGrabbed { get; private set; }
    private bool isGrabbed;
    public float damage = 0.34f;

    //public Transform skeleton;
    //public CapsuleCollider coll;

    private void Awake()
    {
        if (ISDEBUGGING) Debug.Log("ScottsBackup_Receiver_Bite: Awake Called");

        if (controler == null)
            controler = GetComponent<ScottsBackup_ThirdPersonController>();
        if (healthControler == null)
        {
            Debug.LogError("No Health Resrouce Manager Attached!");
            //healthControler = GetComponent<ScottsBackup_ResourceMng>();
        }           
    }

 
    public void fn_SetBiteMode(bool isBitten, Vector3 pos)
    {
        if (ISDEBUGGING) Debug.Log("ScottsBackup_Receiver_Bite: fn_SetBiteMode Called");

        if (isBitten) {
            ActivateBiteModeRpc(pos);
        }
        else 
        {
            DisableBiteModeRPC();
        }
            
    }

    [Rpc(SendTo.Everyone)]
    private void ActivateBiteModeRpc(Vector3 pos)
    {
        // Reposition the bite target transform position to that of the bitter
        var nt = gameObject.GetComponent<NetworkTransform>();
        this.transform.position = pos;
        this.transform.localEulerAngles = new Vector3(0f, 0f, 90f); // set local child rotation (sideways)
        this.transform.position += _positionOffset;

        // Snap state across the network before disabling sync while being carried
        if (nt != null && nt.enabled)
        {
            nt.Teleport(this.transform.position, this.transform.rotation, this.transform.localScale);
        }

        if (nt != null)
            nt.enabled = false;
        controler.fn_IsMovementInputDisabled(true);

        if (IsOwner)
        {
            if (ISDEBUGGING) Debug.Log("ScottsBackup_Receiver_Bite: ActivateBiteModeRpc Called");
            healthControler.fn_ForceReduceValue(damage);
        }

        OnBiteStart?.Invoke();
        isGrabbed = true;
    }

    [Rpc(SendTo.Everyone)]
    private void DisableBiteModeRPC()
    {
        // Re-enable network sync and force-correct orientation for all clients
        var nt = gameObject.GetComponent<NetworkTransform>();
        // Reset to upright in local space (works regardless of parent state)
        this.transform.localEulerAngles = Vector3.zero;

        if (nt != null)
        {
            nt.enabled = true;
            // Force a network snap so every client gets the corrected rotation immediately
            nt.Teleport(this.transform.position, this.transform.rotation, this.transform.localScale);
        }
        controler.fn_IsMovementInputDisabled(false);
        isGrabbed = false;
        OnBiteStop?.Invoke();
    }

}
