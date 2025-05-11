using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Bite Class for the Mutant
/// 
/// Contributors: 
///     Base on Braedon's, 'GrabPlayer' Script
///     Updated by Amber - Stamina Usage
/// 
/// Part of a Two Part System with 'Bite_Activator' & 'Bite_Receiver'
/// </summary>
public class ScottsBackup_PlayerAction_BiteActivator : PlayerActionBase, IHudAbilityBinder
{
    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;

    public Action OnInsufficientStamina;
    public Action OnAttemptToBite;
    public Action OnBiteSuccessful;

    [Header("Resource Use")]
    [SerializeField] private ScottsBackup_ResourceMng _resourceSystem;
    [SerializeField] private float _minimumUsageStamina = 10;
    [SerializeField] private float _biteActivationCost = 5;
    [SerializeField] private float _bitePerSecCost = 2;

    //private MutantStamina _mutantStaminaSystem;

    [Header("Bite Setting")]
    // Bite Holding Point
    public Transform _holdingPoint;

    //Bite Box Collider
    [Header("Bite Zone Collider")]
    public BoxCollider triggerBox; // Set this in the Inspector
    public string targetTag = "Player"; // Set this as needed
    public LayerMask targetLayer; // Assign in Inspector (as a mask)

    // Bite Ref
    [Header("Bite Debugging Refs")]
    public bool isBiting = false;
    public GameObject grabedPlayerGO;

    // UnBite Time / Auto Release
    public float grabTime = 5f;
    public float grabbedTime;

    // Bite CoolDown
    public bool PostBiteCooldownActive { get { return isBiteOnCooldown; } } // Used for animation post Bite (Panting) 
    private bool isBiteOnCooldown = false;
    private float biteCooldown;
    [SerializeField] private float biteCooldownLenght = 5f;

    // Interaction Delay
    private bool isInteractionDelayed = false;
    private float interactionCooldown;
    private float interactionCooldownLenght = 0.2f;

    // GrabRelease Delay
    private bool isBiteReleaseOnCooldown = false;
    private float biteReleaseCooldown;
    private float biteReleaseCooldownLenght = 1f;


    // Player Input 
    private bool _isInput;

    void Start()
    {
    }

    void Update()
    {
        if (!IsOwner)
            return;

        if (_isInput)
        {
            HandleGrabInput();
            _isInput =false; // Clear Input Received Variable
        }
            
        
        // Input Cooldowns
        Update_BiteCooldown();
        Update_InteractionCooldown();
        Update_BiteReleaseCooldown();

        // Timed Release
        Update_GrabTimedRelease();
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


    public override bool fn_ReceiveActivationInput(bool b)
    {
        _isInput = b;
        return false;
    }


    private void HandleGrabInput()
    {

        //// Check For Sufficient Stamina
        //if (_mutantStaminaSystem.get_stamina() < _minimumUsageStamina)
        //{
        //    OnInsufficientStamina?.Invoke();
        //    return;
        //}

        // Check For Sufficient Stamina
        if (_resourceSystem.fn_GetCurrentValue() < _minimumUsageStamina) 
        {
            OnInsufficientStamina?.Invoke();
            return;
        }




        // Check if general interaction on cooldown (0.2s)
        if(IsInteractionOnCooldown()) 
            return;
        TriggerInteractionCooldown();



        if (!isBiting)
        {
            OnAttemptToBite?.Invoke();
            Debug.Log("Trying To Grab A Player");
            TryGrab();
        }
        else
        {
            Debug.Log("Trying To Release A Grabed Player");
            TryRelease();
        }
    }

    private void TryGrab()
    {
        if (!IsOwner)
            return;

        if (!IsATargetInsideBiteCollider(out GameObject biteTarget))
        {
            Debug.Log("ScottsBackup_PlayerAction_BiteActivator: Nothing To Grab!");
            return;
        }

        if (IsBiteOnCooldown())
        {
            Debug.Log("Bite is on cooldown!");
            return;
        }

        //NOTE: - Bite Is Sucessfull! -

        OnBiteSuccessful?.Invoke();

        //_mutantStaminaSystem.reduce_stamina(40);
        _resourceSystem.fn_TryReduceValue(_biteActivationCost); // Bite Start Cost

        
        TriggerBiteReleaseCooldown();

        grabedPlayerGO = biteTarget;
        isBiting = true; //Activated Update Loop

        // RPC Call To Target Player to Notify them they have been Bitten 
        Debug.Log($"{this.gameObject.name} Trying to Grab {biteTarget.gameObject.name}");
        BiteTargetPlayerServerRpc(biteTarget.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId);
    }

    
    private void TryRelease()
    {
        if (!IsOwner)
            return;

        // Minimum Bite Time Not Passed
        if (IsBiteReleaseOnCooldown()) 
            return;

   
        if (IsGrabbed()) //grabedPlayerGO != null
        {
            ReleasePlayer();
        }
        else
        {
            Debug.LogWarning("ScottsBackup_PlayerAction_BiteActivator Trying To Release null player!");
            return;
        }
    }


    // Release if out of Time 
    public void Update_GrabTimedRelease()
    {
        if (IsGrabbed()) //grabedPlayerGO != null
        {
            grabbedTime += Time.deltaTime;

            //DOSE: releases player that was grabbed after a certain amount of time
            if (grabbedTime >= grabTime)
            {              
                ReleasePlayer();
            }
        }
    }

    // Release if out of energy 
    private void Update_OngoingBiteCost()
    {
        if (!isBiting)
            return;

        // Try Deduct Ongoing resrouce cost, if it runs out, drop
        bool isEnoughEnergy = _resourceSystem.fn_TryReduceValue(_bitePerSecCost);

        // Release Player if out of energy
        if (!isEnoughEnergy)
            ReleasePlayer();
    }


    /// <summary>
    /// runs the release target rpc to release player from grab and resets all grab related variables
    /// and set a cooldown on bite
    /// </summary>
    private void ReleasePlayer()
    {
        Debug.Log("player released");
        ReleaseTargetPlayerServerRpc(grabedPlayerGO.GetComponent<NetworkObject>().OwnerClientId, gameObject.GetComponent<NetworkObject>().OwnerClientId);

        TriggerBiteCooldown();

        isBiting = false;
        grabedPlayerGO = null;
        grabbedTime = 0;
    }

    /// <summary>
    /// checks if grabedPlayerGO is not null when isgrabbing is true
    /// if grabedPlayerGO is null then reset grab variables and return false
    /// if grabedPlayerGO is not null then return true
    /// </summary>
    /// <returns> true if grabedPlayerGO is not null </returns>
    private bool IsGrabbed()
    {
        //NOTE: Is a grabbed Player
        if (grabedPlayerGO != null)
            return true;

        // NOTE: Isn't a grabbed Player, reset variables
        isBiting = false;
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

        // Server Authorative - On the server, tell it to set the grabbed player's Bite_Receiver to call Is Bitten.
        NetworkManager.Singleton.ConnectedClients[targetPlayerId].PlayerObject.gameObject.GetComponent<ScottsBackup_Receiver_Bite>().fn_SetBiteMode(true, _holdingPoint.position);
    }

    [ServerRpc]
    public void ReleaseTargetPlayerServerRpc(ulong PlayerId, ulong id)
    {
        GameObject p = NetworkManager.Singleton.ConnectedClients[PlayerId].PlayerObject.gameObject;
        p.transform.parent = null;

        //find and runs a function on the grabbed players animal chaaracter script that disables the players network transformer and movement in the script
        p.GetComponent<ScottsBackup_Receiver_Bite>().fn_SetBiteMode(false, Vector3.zero);
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

    // Bite Action CoolDown - Triggers On Sucessfull Bite Started
    private bool IsBiteOnCooldown() => isBiteOnCooldown;
    private void TriggerBiteCooldown()
    {
        OnCooldownWithLengthTriggered?.Invoke(biteCooldownLenght);
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

    // Minimum Time Between Each Attempt To Bite
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

    // Minimum Bite Time, Cannot Release before minimum time
    private bool IsBiteReleaseOnCooldown() => isBiteReleaseOnCooldown;
    private void TriggerBiteReleaseCooldown()
    {
        OnCooldownWithLengthTriggered?.Invoke(biteReleaseCooldownLenght);
        biteReleaseCooldown = Time.time + biteReleaseCooldownLenght;
        isBiteReleaseOnCooldown = true;
    }
    private void Update_BiteReleaseCooldown()
    {
        if (!isBiteReleaseOnCooldown)
            return;

        // End of Cooldown
        if (biteReleaseCooldown <= Time.time)
        {
            isBiteReleaseOnCooldown = false;
        }
    }
    #endregion END: Timers
}

