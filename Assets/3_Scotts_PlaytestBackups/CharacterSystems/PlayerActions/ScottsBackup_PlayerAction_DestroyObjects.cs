using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 
/// Note
/// </summary>
public class ScottsBackup_PlayerAction_DestroyObjects : PlayerActionBase, IHudAbilityBinder
{
    private const bool ISDEBUGGING = true;

    public event Action<float> OnCooldownWithLengthTriggered;
    public event Action OnCooldownCanceled;

    public Action OnActivationSuccess;
    public UnityEvent OnActivationSuccess_UE;

    public Action OnActivationFail_NotEnoughEnergy;


    //interaction collider
    [SerializeField] private BoxCollider colliderBox;

    //stamina 
    [SerializeField] private ScottsBackup_ResourceMng _staminaSystem;
    [SerializeField] private float _enegryCost = 10f;

    //Cooldown
    [SerializeField] private float _abilityCooldownLength = 10f;

    //Delay In Bush Destuction To give time for effects
    [SerializeField] private float _destructionDelay = 1.5f;


    private bool _inputRecived;

    private void Start()
    {
        // Disable Self If Not Owner
        if (!IsOwner)
        {
            this.enabled = false;
            return;
        }
    }

    public override bool fn_ReceiveActivationInput(bool b)
    {
        if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_DestroyObjects: ActionInput Recived");
        _inputRecived = b;

        Handle_InputRecived();

        return false;
    }

    private void Handle_InputRecived()
    {
        //check if theres sufficent Stamina 
        if(_staminaSystem.fn_GetCurrentValue() < _enegryCost)
        {
            if (ISDEBUGGING) Debug.Log("ScottsBackup_PlayerAction_DestroyObjects: OnActivationFail NotEnoughEnergy");
            OnActivationFail_NotEnoughEnergy?.Invoke();
            return;
        }
           
        // If sucessfull
        if (TryDestoryObject())
        {
            _staminaSystem.fn_TryReduceValue(_enegryCost);
            OnActivationSuccess?.Invoke();
            OnActivationSuccess_UE?.Invoke();
            OnCooldownWithLengthTriggered?.Invoke(_abilityCooldownLength);
        }
    }

    /// <summary>
    /// Trys to trigger DestructibleObject_Reciver
    /// </summary>
    public bool TryDestoryObject()
    {

        if (colliderBox == null || !colliderBox.isTrigger)
        {
            Debug.LogWarning("BoxCollider is null or not set as trigger!");
            return false;
        }

        // Calculate world-space bounds of the box
        Vector3 center = colliderBox.transform.TransformPoint(colliderBox.center);
        Vector3 halfExtents = Vector3.Scale(colliderBox.size * 0.5f, colliderBox.transform.lossyScale);
        Quaternion orientation = colliderBox.transform.rotation;

        // Check all overlapping colliders
        Collider[] hits = Physics.OverlapBox(center, halfExtents, orientation);

        foreach (Collider hit in hits)
        {
            if (hit == colliderBox) continue; // Skip self

            if(hit.TryGetComponent<DestructibleObject_Reciver>(out DestructibleObject_Reciver dr))
            {
                dr.fn_TriggerDestruction(_destructionDelay);
                return true;
            }
        }

        return false;
    }


}
