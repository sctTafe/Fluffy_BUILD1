using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Bite Class for the Mutant
/// 
/// Base on Braedon's, 'GrabPlayer' Script
/// Part of a Two Part System with 'Bite_Activator' & 'Bite_Receiver'
/// </summary>
public class Bite_Activator : NetworkBehaviour
{
    public Transform _holdingPoint;

    //Bite Box Collider
    public BoxCollider triggerBox; // Set this in the Inspector
    public string targetTag = "Player"; // Set this as needed
    public LayerMask targetLayer; // Assign in Inspector (as a mask)

    // Bite Ref
    public bool isGrabbing = false;
    public GameObject grabedPlayerGO;

    // UnBite Time
    public float grabTime = 5f;
    public float grabbedTime;

    // Bite CoolDown
    private bool isBiteOnCooldown = false;
    private float biteCooldown;
    [SerializeField] private float biteCooldownLength = 2f;

    // Interaction Delay
    private bool isInteractionDelayed = false;
    private float interactionCooldown;
    private float interactionCooldownLength = 0.5f;

    //cooldown general
    private bool OnCooldown = false;
    private float Cooldown;
    //private float CooldownLenght = 0.5f;

	// Added by Amber, mutant stamina code	
	private MutantStamina s;
	private GameObject mutant_bite_fill;

	void Start()
	{
		s = GetComponent<MutantStamina>();
		mutant_bite_fill = GameObject.FindWithTag("mutant_bite_fill");
	}
	// End of portion by Amber

    void Update()
    {
        if (!IsOwner) 
            return;

        //Temp Input
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleGrabInput();
        }

        Update_GrabTimedRelease();
        //Update_BiteCooldown();
        //Update_InteractionCooldown();
        Update_Cooldown();
    }

    // Optional: draw the overlap box in scene view
    private void OnDrawGizmosSelected()
    {
        if (triggerBox == null) return;

        Gizmos.color = Color.green;
        Vector3 center = triggerBox.transform.TransformPoint(triggerBox.center);
        Vector3 size = Vector3.Scale(triggerBox.size, triggerBox.transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(center, triggerBox.transform.rotation, size);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private void HandleGrabInput()
    {
		// Added by Amber, reducing Mutant stamina when they bite
		if(s.get_stamina() < 40)
		{
			return;
		}

		s.reduce_stamina(40);
		//End of code added by Amber

        //if(IsInteractionOnCooldown()) return;
        if (IsOnCooldown()) return;
        //TriggerInteractionCooldown();
        if (!isGrabbing)
        {
            Debug.Log("Trying To Grab A Player");
            TryGrab();
        }
        else
        {
            //if (IsOnCooldown()) return;
            
            Debug.Log("Trying To Release A Grabed Player");
            TryRelease();
        }
    }

    private void TryGrab()
    {
        if (!IsOwner) 
            return;

        if(!IsATargetInsideBiteCollider(out GameObject biteTarget))
        {
            Debug.Log("Bite_Activator: Nothing To Grab!");
            return;
        }
        //why is there a cooldown check here and one also in habdle grab input (shou;d not be trying to grab when is grab so 
        /*
        if (IsBiteOnCooldown())
        {
            Debug.Log("Bite is on cooldown!");
            return;
        }
        */    
        //why is the start for the grab cooldown here (its gonna countdown while the player is grabbed)
        //it should be starting the trigger cool down. (so can't trigger again)
        //bite cooldown shoul start in the release
        //TriggerBiteCooldown();
        grabedPlayerGO = biteTarget;
        isGrabbing = true; //Activated Update Loop
        //SetCooldown(biteCooldownLenght);
        SetCooldown(interactionCooldownLength);
        Debug.Log($"{this.gameObject.name} Trying to Grab {biteTarget.gameObject.name}");
        BiteTargetPlayerServerRpc(biteTarget.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId);    
    }

    private void TryRelease()
    {
        if (!IsOwner)
            return;

        //isGrabbing = false;
        //grabbedTime = 0; // reset bite hold timer

        if (IsGrabbed())//grabedPlayerGO == null)
        {
            ReleasePlayer();
        }
        else
        {
            Debug.LogWarning("Bite_Activator Trying To Release null player!");
            return;
        }
        //ReleaseTargetPlayerServerRpc(grabedPlayerGO.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId);
        //grabedPlayerGO = null;
        //SetCooldown(biteCooldownLenght);
    }


    public void Update_GrabTimedRelease()
    {
        if (IsGrabbed()) //grabedPlayerGO != null
        {
            grabbedTime += Time.deltaTime;

            //relase player if the player was grabed for a certain amount of time
            if (grabbedTime >= grabTime)
            {
                //isGrabbing = false;
                //grabbedTime = 0;
                //Debug.Log("release player");
                //ReleaseTargetPlayerServerRpc(grabedPlayerGO.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId);
                //grabedPlayerGO = null;
                ReleasePlayer();
            }
        }
        //else
        //{
            //else check just in case the player gamebject dissappears
            //(ensure if for example the object is destroyed that the mutant will be immediatly free of the grab (won't have to wait for the grab timer))
            //isGrabbing = false;
            //grabbedTime = 0;
        //}
    }

    /// <summary>
    /// runs the releasetarget rpc to release player from grab and resets all grab related variables
    /// and set a cooldown on bite
    /// </summary>
    private void ReleasePlayer()
    {
        isGrabbing = false;
        grabbedTime = 0;
        Debug.Log("player released");
        ReleaseTargetPlayerServerRpc(grabedPlayerGO.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId);
        grabedPlayerGO = null;
        SetCooldown(biteCooldownLength);
    }

    /// <summary>
    /// checks if grabedPlayerGO is not null when isgrabbing is true
    /// if grabedPlayerGO is null then reset grab variables and return false
    /// if grabedPlayerGO is not null then return true
    /// </summary>
    /// <returns>true if grabedPlayerGO is not null </returns>
    private bool IsGrabbed()
    {
        if (grabedPlayerGO != null)
        {
            return true;
        }
        isGrabbing = false;
        grabbedTime = 0;
        return false;
    }

    /// <summary>
    /// Server Authorative Bite Mode Activation
    /// 
    /// NOTE/QUESTION What is happening to network ownership in this interaction???
    /// 
    /// </summary>
    /// <param name="targetPlayerId"></param>
    /// <param name="bitterPlayerId"></param>
    [ServerRpc]
    public void BiteTargetPlayerServerRpc(ulong targetPlayerId, ulong bitterPlayerId)
    {
        // Reparent bitte target Network Object
        Transform bitterTrans = NetworkManager.Singleton.ConnectedClients[bitterPlayerId].PlayerObject.gameObject.transform;
        Transform targetTrans = NetworkManager.Singleton.ConnectedClients[targetPlayerId].PlayerObject.gameObject.transform;
        targetTrans.parent = bitterTrans;

        // Reposition the bite target transform position to that of the bitter - THis is done on Bitten players end prior to network transform disable
        //  targetTrans.position = new Vector3(0, 0, 0);

        // Server Authorative - On the server, tell it to set the grabbed player's Bite_Receiver to call Is Bitten.
        NetworkManager.Singleton.ConnectedClients[targetPlayerId].PlayerObject.gameObject.GetComponent<Bite_Receiver>().fn_SetBiteMode(true, _holdingPoint.position);


        //find and runs a function on the grabbed players animal chaaracter script that disables the players network transformer and movement in the script
        //NetworkManager.Singleton.ConnectedClients[PlayerId].PlayerObject.gameObject.GetComponent<AnimalCharacter>().DisableMovementRpc();

    }

    [ServerRpc]
    public void ReleaseTargetPlayerServerRpc(ulong PlayerId, ulong id)
    {
        GameObject p = NetworkManager.Singleton.ConnectedClients[PlayerId].PlayerObject.gameObject;
        p.transform.parent = null;

        //find and runs a function on the grabbed players animal chaaracter script that disables the players network transformer and movement in the script
        p.GetComponent<Bite_Receiver>().fn_SetBiteMode(false, Vector3.zero);
    }




    #region Bite Box Collider Check

    /// <summary>
    /// Checks if anything with the target tag or layer is inside the triggerBox.
    /// </summary>
    public bool IsATargetInsideBiteCollider(out GameObject? biteTarget)
    {
        biteTarget = null;

        if (triggerBox == null || !triggerBox.isTrigger)
        {
            Debug.LogWarning("BoxCollider is null or not set as trigger!");
            return false;
        }

        // Calculate world-space bounds of the box
        Vector3 center = triggerBox.transform.TransformPoint(triggerBox.center);
        Vector3 halfExtents = Vector3.Scale(triggerBox.size * 0.5f, triggerBox.transform.lossyScale);
        Quaternion orientation = triggerBox.transform.rotation;

        // Check all overlapping colliders
        Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation);

        foreach (Collider hit in hits)
        {
            if (hit == triggerBox) continue; // Skip self

            if ((targetLayer.value == hit.gameObject.layer) || hit.CompareTag(targetTag))
            {
                biteTarget = hit.gameObject;
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Timers
    /*
    private bool IsBiteOnCooldown() => isBiteOnCooldown;  
    private void TriggerBiteCooldown()
    {
        biteCooldown = Time.time + biteCooldownLenght;
        isBiteOnCooldown = true;
    }
    private void Update_BiteCooldown()
    {
        if (!isBiteOnCooldown)
            return;

        // End of Cooldown
        if (biteCooldown <= Time.time)
        {
            isBiteOnCooldown = false;
        }
    }

    private bool IsInteractionOnCooldown() => isInteractionDelayed;
    private void TriggerInteractionCooldown()
    {
        interactionCooldown = Time.time + interactionCooldownLenght;
        isInteractionDelayed = true;
    }
    private void Update_InteractionCooldown()
    {
        if (!isInteractionDelayed)
            return;

        // End of Cooldown
        if (interactionCooldown <= Time.time)
        {
            isInteractionDelayed = false;
        }
    }

    */



    private bool IsOnCooldown() => OnCooldown;
    private void SetCooldown(float CooldownLength)
    {
        Cooldown = Time.time + CooldownLength;
        OnCooldown = true;
    }
    private void Update_Cooldown()
    {
        if (!OnCooldown)
            return;

        // End of Cooldown
        if (Cooldown <= Time.time)
        {
            OnCooldown = false;
        }

		// Added by Amber, cooldown UI	
        if (mutant_bite_fill != null)
			mutant_bite_fill.transform.localScale = new Vector3(1, Mathf.Clamp(biteCooldownLength - (Cooldown - Time.time), 0, biteCooldownLength) / biteCooldownLength, 1);
		//End of code added by Amber
    }
    #endregion END: Timers
}



