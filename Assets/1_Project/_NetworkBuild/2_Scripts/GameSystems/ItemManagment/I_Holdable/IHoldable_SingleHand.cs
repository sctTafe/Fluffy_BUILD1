using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Holdable Item, fits in a single hand
/// </summary>
public class IHoldable_SingleHand : NetworkBehaviour, IHoldable
{
    private NetworkObject _refParentNetworkObject;
    private Transform _refHoldingTransform;
    private IHolder _refCurrentHolder;


    public IHolder CurrentHolder { get { return _refCurrentHolder; } }


    private NetworkObject _thisObjectsNetworkObject;

    private void Awake()
    {
        _thisObjectsNetworkObject = GetComponent<NetworkObject>();
    }

    private void LateUpdate()
    {
        MatchItemPositionAndRotationToHoldPosition();
    }

    //Auto Updates Network
    public void fn_Destroy()
    {
        if (IsServer || IsOwner)
        {
            NetworkObject networkObject = GetComponent<NetworkObject>();
            networkObject.Despawn(true);  //True = Destory it aswell
        }
    }

    // Server Authoritative
    #region fn_SetUnheld()
    public void fn_Drop()
    {
        SetUnheldServerRPC();
    }
    [ServerRpc(RequireOwnership = false)]
    void SetUnheldServerRPC()
    {
        this.transform.parent = null;
        SetUnheldClientRPC();
    }
    [ClientRpc]
    void SetUnheldClientRPC()
    {
        fn_ClearHolder();
    }
    #endregion


    // Server Authoritative
    #region fn_BindToHolder
    public void fn_BindToHolder(IHolder holder)
    {
        BindToHolderServerRPC(holder.fn_GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    void BindToHolderServerRPC(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject toBeParent))
        {
            this.transform.parent = toBeParent.transform;
        }
            

        BindToHolderClientRPC(networkObjectReference);
    }

    [ClientRpc]
    void BindToHolderClientRPC(NetworkObjectReference networkObjectReference)
    {
        // TODO -> use network variables for this stuff

        if (networkObjectReference.TryGet(out NetworkObject toBeParent))
        {
            if (_refCurrentHolder != null)
            {
                fn_ClearHolder();
            }

            _refCurrentHolder = toBeParent.GetComponent<IHolder>();
            _refParentNetworkObject = toBeParent;

            //Assign the Holdable Item to the Holder, and get the _holdingTransform
            _refCurrentHolder.fn_AssignNextAvailableHoldingPosition(this);
            _refHoldingTransform = _refCurrentHolder.fn_GetTransfromOfHoldingPosition(this);
        }
        else
        {
            Debug.LogError("Could Not Find NetworkObject from NetworkObjectRef!");
        }
    }
    #endregion

    // Server Authoritative
    #region fn_SetPosition()
    public void fn_SetPosition(Vector3 position)
    {
        SetPositionServerRPC(position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPositionServerRPC(Vector3 position)
    {
        SetPositionClientRPC(position);
    }

    [ClientRpc]
    private void SetPositionClientRPC(Vector3 position)
    {
        this.transform.position = position;
    }
    #endregion




    #region Attempt 1
    public void fn_TryAssignNetworkObjectParentAndHoldingPosition(IHolder parent)
    {
        Debug.LogError("This Function is not in use! -> Use fn_BindToHolder");
        //AssignNetworkObjectParentAndHoldingPositionServerRpc(parent.fn_GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignNetworkObjectParentAndHoldingPositionServerRpc(NetworkObjectReference networkObjectReference)
    {
        AssignNetworkObjectParentAndHoldingPositionClientRpc(networkObjectReference);
    }

    [ClientRpc]
    private void AssignNetworkObjectParentAndHoldingPositionClientRpc(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject Holder) && Holder.TryGetComponent<IHolder>(out IHolder iholder))
        {
            // Check if IHolder has space for this IHoldable
            if (iholder.fn_CheckIfHoldingSlotIsAvailable())
            {
                // -> If has space to hold an object, Call the Client RPC to make it so on clients
                _refCurrentHolder = iholder.fn_AssignNextAvailableHoldingPosition(this);
            }
        }
    }
    #endregion


    #region Attemp2
    public void fn_AssignHolder(IHolder parent)
    {
        Debug.LogError("This Function is not in use! -> Use fn_BindToHolder");
        //AssignHolderServerRpc(parent.fn_GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void AssignHolderServerRpc(NetworkObjectReference networkObjectReference)
    {
        AssignHolderClientRpc(networkObjectReference);
    }

    [ClientRpc]
    private void AssignHolderClientRpc(NetworkObjectReference networkObjectReference)
    {
        if (networkObjectReference.TryGet(out NetworkObject Holder) && Holder.TryGetComponent<IHolder>(out IHolder iholder))
        {
            // Check if IHolder has space for this IHoldable
            if (iholder.fn_CheckIfHoldingSlotIsAvailable())
            {
                // -> If has space to hold an object, Call the Client RPC to make it so on clients
                _refCurrentHolder = iholder.fn_AssignNextAvailableHoldingPosition(this);
            }
        }
    }
    #endregion




    public void fn_DeassignNetworkObjectParent()
    {
        throw new System.NotImplementedException();
    }

    public void fn_DestoryHoldableNetworkObject()
    {
        throw new System.NotImplementedException();
    }
    // Transform Rotaton & Position matching
    public void SetTargetTransform(Transform targetTransform)
    {
        this._refHoldingTransform = targetTransform;
    }





    private void MatchItemPositionAndRotationToHoldPosition()
    {
        if (_refHoldingTransform == null)
        {
            return;
        }
        transform.position = _refHoldingTransform.position;
        transform.rotation = _refHoldingTransform.rotation;
    }




    public void fn_SetNetworkObjectParentToHolder(NetworkObject toBeParent)
    {
        SetParentTransfromServerRPC(toBeParent);
    }

    [ServerRpc(RequireOwnership = false)]
    void SetParentTransfromServerRPC(NetworkObjectReference networkObjectReference)
    {
        SetParentTransfromClientRPC(networkObjectReference);
    }

    [ClientRpc]
    void SetParentTransfromClientRPC(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject toBeParent);
        this.transform.parent = toBeParent.transform;
    }











    public NetworkObject fn_GetNetworkObject()
    {
        return _thisObjectsNetworkObject;
    }

    public void fn_ClearHolder()
    {
        _refCurrentHolder = null;
        _refParentNetworkObject = null;
        _refHoldingTransform = null;
    }

    /// <summary>
    /// Sends 'this' to the Holder to check which transform its currently assigned & updated '_refHoldingTransform' accordingly
    /// </summary>
    public void fn_UpdateHoldingTransformPosition()
    {
        if (_refCurrentHolder == null)
        {
            Debug.LogError("No Current Holding Object!");
            return;
        }
                  
        UpdateHoldingTransformPositionServerRPC();
    }


    [ServerRpc(RequireOwnership = false)]
    void UpdateHoldingTransformPositionServerRPC()
    {
        UpdateHoldingTransformPositionClientRPC();
    }

    [ClientRpc]
    void UpdateHoldingTransformPositionClientRPC()
    {
        _refHoldingTransform = _refCurrentHolder.fn_GetTransfromOfHoldingPosition(this);
    }
}
