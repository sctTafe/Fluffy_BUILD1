using StarterAssets;
using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// Local Client Bite_Receiver
/// 
/// Base on Braedon's, 'GrabPlayer' Script
/// Part of a Two Part System with 'Bite_Activator' & 'Bite_Receiver'
/// </summary>
public class Bite_Receiver : NetworkBehaviour
{
    public ThirdPersonController_Netcode controler;
    

    public bool IsGrabbed {  get; private set; }
    private bool isGrabbed;
    //void Start()
    //{        
    //}

    //void Update()
    //{ 
    //}
    private void Awake()
    {
        if (controler == null)
            controler = GetComponent<ThirdPersonController_Netcode>();
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
        this.transform.eulerAngles = new Vector3(0f, 0f, 90f);

        gameObject.GetComponent<NetworkTransform>().enabled = false;
        controler.fn_IsMovementInputDisabled(true);
        isGrabbed = true;
    }

    [Rpc(SendTo.Everyone)]
    private void DisableBiteModeRPC()
    {
        gameObject.GetComponent<NetworkTransform>().enabled = true;
        this.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        controler.fn_IsMovementInputDisabled(false);
        isGrabbed = false;
    }
}
