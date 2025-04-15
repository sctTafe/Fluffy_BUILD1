using System;
using System.Collections.Specialized;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Info:
/// Ownership - if atached to the player character, then likly owned by the player
/// </summary>
public class IHolder_Player : NetworkBehaviour, IHolder
{
    
    [SerializeField] Transform[] _holdingPositionTansforms;  //Set In Inspector
    public IHoldable[] _heldNetworkObjects; // Holds the Holdable Items

    [SerializeField] NetworkObject _playersNetworkObject;

    private void Awake()
    {
        if (IsOwner)
        {
            _playersNetworkObject = GetComponent<NetworkObject>();
            _heldNetworkObjects = new IHoldable[_holdingPositionTansforms.Length];
        }

    }

    public IHolder fn_AssignNextAvailableHoldingPosition(IHoldable holdableNetworkObject)
    {
        if(fn_CheckIfHoldingSlotIsAvailable() == false)
        {
            Debug.LogWarning("No holding Positions Avalible");
            return null;
        }

        for (int i = 0; i < _heldNetworkObjects.Length; i++)
        {
            if (_heldNetworkObjects[i] == null)
            {
                _heldNetworkObjects[i] = holdableNetworkObject;
                return this;
            }
        }

        // Return Null If no free spaces to assign holdable to
        return null;
    }


    public bool fn_CheckIfHoldingSlotIsAvailable()
    {
        foreach (var item in _heldNetworkObjects)
        {
            if (item == null)
            {
                return true; // Empty Slot Found
            }
        }

        return false; // All Slots Full
    }

    public Transform fn_GetTransfromOfHoldingPosition(IHoldable holdableNetworkObject)
    {
        for (int i = 0; i < _heldNetworkObjects.Length; i++)
        {
            if (_heldNetworkObjects[i] == holdableNetworkObject)
            {
                return _holdingPositionTansforms[i];
            }
        }

        // No matching IHoldable
        return null;
    }

    public NetworkObject fn_GetNetworkObject()
    {
        return _playersNetworkObject;
    }


    /// <summary>
    /// Communicate with the Item(Holdable) to drop itself
    /// </summary>
    public void fn_DropSlot0Item()
    {
        if (_heldNetworkObjects[0] == null)
            return;

        _heldNetworkObjects[0].fn_Drop();
        _heldNetworkObjects[0] = null;       
    }

    //[ServerRpc(RequireOwnership = false)]
    //public void DropSlot0ServerRPC()
    //{
    //    DropSlot0ClientRPC();
    //}
    //[ClientRpc]
    //public void DropSlot0ClientRPC()
    //{
    //    if (_heldNetworkObjects[0] == null)
    //        return;

    //    _heldNetworkObjects[0].fn_Drop();
    //    _heldNetworkObjects[0] = null;
    //}



    // TODO: THIS NEEDS UPDATING TO TAKE OWNERSHIP INTO ACCOUNT
    /// <summary>
    /// Communicate with the Item(Holdable) to Destory itself
    /// </summary>
    public void fn_DestroySlot0Item()
    {
        if (IsOwner)
        {
            _heldNetworkObjects[0].fn_Destroy();
            _heldNetworkObjects[0] = null;
        }
    }

    public void fn_CycleItemSlots()
    {
        if (IsOwner) 
        {
            fn_ShiftHoldingArray();
        }
    }


    #region Shift Holding Array

    void fn_ShiftHoldingArray()
    {
        ShiftArrayRight(_heldNetworkObjects);
        UpdateTransformOfHeldObjects();
    }

    void UpdateTransformOfHeldObjects()
    {
        foreach (var holdableItem in _heldNetworkObjects)
        {
            if (holdableItem != null)
            {
                holdableItem.fn_UpdateHoldingTransformPosition();
            }
        }
    }

    void ShiftArrayRight<T>(T[] array)
    {
        if (array == null || array.Length < 2) return;

        T lastElement = array[array.Length - 1];

        for (int i = array.Length - 1; i > 0; i--)
        {
            array[i] = array[i - 1];
        }

        array[0] = lastElement;
    }
    #endregion

}
