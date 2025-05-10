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
public class ScottsBackup_BiteReceiver : NetworkBehaviour
{
    public UnityEvent OnBiteStart;
    public UnityEvent OnBiteStop;

    [SerializeField] private ScottsBackup_ThirdPersonController controler;   // The Character Controller
    [SerializeField] private Vector3 _positionOffset;

    public bool IsGrabbed { get; private set; }
    private bool isGrabbed;
    public float damage = 0.34f;

    //public Transform skeleton;
    //public CapsuleCollider coll;

    private void Awake()
    {
        if (controler == null)
            controler = GetComponent<ScottsBackup_ThirdPersonController>();
    }
   
    public void fn_SetBiteMode(bool isBitten, Vector3 pos)
    {
        if (isBitten)
            ActivateBiteModeRpc(pos);
        else
            DisableBiteModeRPC();
    }

    [Rpc(SendTo.Everyone)]
    private void ActivateBiteModeRpc(Vector3 pos)
    {
        // Reposition the bite target transform position to that of the bitter
        this.transform.position = pos;
        this.transform.localEulerAngles = new Vector3(0f, 0f, 90f); //set local child rotation
        this.transform.position += _positionOffset;


        gameObject.GetComponent<NetworkTransform>().enabled = false;
        controler.fn_IsMovementInputDisabled(true);

        if (IsOwner) 
            gameObject.GetComponent<PlayerHealth>().ChangePlayerHealth(damage);
       
        OnBiteStart?.Invoke();
        isGrabbed = true;
    }

    [Rpc(SendTo.Everyone)]
    private void DisableBiteModeRPC()
    {
        gameObject.GetComponent<NetworkTransform>().enabled = true;
        this.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        controler.fn_IsMovementInputDisabled(false);
        isGrabbed = false;
        OnBiteStop?.Invoke();
    }
}
