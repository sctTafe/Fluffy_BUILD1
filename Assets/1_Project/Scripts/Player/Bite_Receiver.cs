using StarterAssets;
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
public class Bite_Receiver : NetworkBehaviour
{
    public UnityEvent OnBiteStart;
    public UnityEvent OnBiteStop;

    [SerializeField] private AnimalCharacter controler;
    [SerializeField] private Vector3 _positionOffset;

    public bool IsGrabbed {  get; private set; }
    private bool isGrabbed;
    public float damage = 0.34f;

    public Transform skeleton;
    public CapsuleCollider coll;

    private void Awake()
    {
        if (controler == null)
            controler = GetComponent<AnimalCharacter>();    // The Character Controller
    }

    //
    public void fn_SetBiteMode(bool isBitten, Vector3 pos)
    {
        if (isBitten)
            ActivateBiteModeRpc(pos);
        else
            DisableBiteModeRPC();
    }
  
    [Rpc(SendTo.Everyone)]
    private  void ActivateBiteModeRpc(Vector3 pos)
    {
        // Reposition the bite target transform position to that of the bitter
        this.transform.position = pos;
        skeleton.localEulerAngles = new Vector3(0f, 0f, 90f); //set local child rotation
        this.transform.position += _positionOffset;


        gameObject.GetComponent<NetworkTransform>().enabled = false;
        controler.fn_IsMovementInputDisabled(true);
        if (IsOwner) gameObject.GetComponent<PlayerHealth>().ChangePlayerHealth(damage);
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        coll.enabled = false;
        OnBiteStart?.Invoke();
        isGrabbed = true;
    }

    [Rpc(SendTo.Everyone)]
    private void DisableBiteModeRPC()
    {
        gameObject.GetComponent<NetworkTransform>().enabled = true;
        skeleton.eulerAngles = new Vector3(0f, 0f, 0f);
        controler.fn_IsMovementInputDisabled(false);
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        coll.enabled = true;
        isGrabbed = false;
        OnBiteStop?.Invoke();
    }
}
