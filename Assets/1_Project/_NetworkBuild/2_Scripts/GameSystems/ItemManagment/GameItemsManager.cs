using Unity.Netcode;
using UnityEngine;

public class GameItemsManager : NetworkSingleton<GameItemsManager>
{
    [SerializeField] private NetworkObjectsListType1SO networkObjectsListType1SO; //List of network objects, for providing the index value

    #region Basic
    public void fn_SpawnNetworkObjectType1(NetworkObjectsType1SO networkObjectType1SO)
    {
        SpawnNetworkObjectType1ServerRpc(GetType1NetObjSOIndex(networkObjectType1SO));
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnNetworkObjectType1ServerRpc(int netObjType1SOIndex)
    {
        NetworkObject netObjType1NetObj = CreateNewNetworkObjectOfIndexType(netObjType1SOIndex);
        netObjType1NetObj.Spawn(true);
    }
    #endregion

    #region Basic With Position
    public void fn_SpawnNetworkObjectType1(NetworkObjectsType1SO networkObjectType1SO, Vector3 position)
    {
        SpawnNetworkObjectType1ServerRpc(GetType1NetObjSOIndex(networkObjectType1SO), position);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnNetworkObjectType1ServerRpc(int netObjType1SOIndex, Vector3 position)
    {
        NetworkObject netObjType1NetObj = CreateNewNetworkObjectOfIndexType(netObjType1SOIndex);

        netObjType1NetObj.Spawn(true);
        netObjType1NetObj.transform.position = position;
        netObjType1NetObj.transform.rotation = Quaternion.identity;
    }
    #endregion

    #region Basic With Parent
    public void fn_SpawnNetworkObjectType1(NetworkObjectsType1SO networkObjectType1SO, NetworkObject toBeParentNetObj)
    {
        SpawnNetworkObjectType1ServerRpc(GetType1NetObjSOIndex(networkObjectType1SO), toBeParentNetObj.NetworkObjectId);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnNetworkObjectType1ServerRpc(int netObjType1SOIndex, ulong parentNetObjId)
    {
        NetworkObject netObjType1NetObj = CreateNewNetworkObjectOfIndexType(netObjType1SOIndex);
        netObjType1NetObj.Spawn(true);

        //Set parent object
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetObjId, out NetworkObject toBeParentNetObj);
        if (toBeParentNetObj != null)
        {
            netObjType1NetObj.transform.parent = toBeParentNetObj.transform;
        }
        else
        {
            Debug.LogWarning("Couldnt find parent network ID");
        }
    }
    #endregion

    public void fn_SpawnHoldableItem(NetworkObjectsType1SO networkObjectType1SO, NetworkObject toBeParentNetObj)
    {
        // check if has the prefab has an iholdable component
        if(networkObjectType1SO.prefab.TryGetComponent<IHoldable>(out IHoldable holdableItem))
        {
            SpawnHoldableItemServerRpc(GetType1NetObjSOIndex(networkObjectType1SO), toBeParentNetObj.NetworkObjectId);
        }
        else
        {
            Debug.LogWarning("Trying to spawn a non holdable Item");
        }     
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnHoldableItemServerRpc(int netObjType1SOIndex, ulong parentNetObjId)
    {
        //Parent Object -> IHolder
        IHolder holder = null;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(parentNetObjId, out NetworkObject toBeParentNetObj))
        {
            holder = toBeParentNetObj.transform.GetComponent<IHolder>();
        }
        //GUARD
        else
        {
            Debug.LogError("Parent Network Object ID cannot be found!");
            return;
        }
                 
        //GUARD
        if (holder == null)
        {
            Debug.LogError("IHolder component not found on parent network Obj!");
            return;
        }
            

        // Network Obj Item -> IHoldable
        NetworkObject newHoldableNetworkObject = CreateNewNetworkObjectOfIndexType(netObjType1SOIndex);     
        IHoldable holdable = newHoldableNetworkObject.transform.GetComponent<IHoldable>();
        newHoldableNetworkObject.Spawn(true);



        // Check if the is space on the IHolder
        if (holder.fn_CheckIfHoldingSlotIsAvailable()) //Note: Server Authratative
        {
            // Holder Has Space
            // -> Spawn Held by the Holder
            holdable.fn_BindToHolder(holder);
        }
        else
        {
            // Holder Dose Not Have Space
            // -> Spawn item into the envrionment around the parent Object 
            Debug.LogWarning("Holder Dose Not have space!");
            holdable.fn_Drop();
            holdable.fn_SetPosition(toBeParentNetObj.transform.position);
        }
    }




    #region Support Functions
    private NetworkObject CreateNewNetworkObjectOfIndexType(int netObjType1SOIndex)
    {
        NetworkObjectsType1SO type1NetworkObjectSO = GetType1NetObjSOFromIndex(netObjType1SOIndex);
        Transform NetObjType1Prefab = Instantiate(type1NetworkObjectSO.prefab);
        NetworkObject netObjType1NetObj = NetObjType1Prefab.GetComponent<NetworkObject>();
        return netObjType1NetObj;
    }

    public int GetType1NetObjSOIndex(NetworkObjectsType1SO type1NetObjSO)
    {
        Debug.Log("Bleep2");
        if(networkObjectsListType1SO == null)
            Debug.LogError("NetworkObjectList Missing");
       
        if(!networkObjectsListType1SO.TryGetIndexMatch(type1NetObjSO, out int index))
        {
            Debug.LogWarning("Network Object Match Missing");
            return -1;
        }
        else
        {
            Debug.Log($"NetworkObjectType Index Match = {index}");
            return index;
        }
    }

    public NetworkObjectsType1SO GetType1NetObjSOFromIndex(int kitchenObjectSOIndex)
    {
        if (networkObjectsListType1SO == null)
            Debug.LogError("NetworkObjectList Missing");

        return networkObjectsListType1SO.notworkObjectType1SOList[kitchenObjectSOIndex];
    }
    #endregion
}
